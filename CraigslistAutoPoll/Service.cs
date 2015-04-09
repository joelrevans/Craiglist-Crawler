using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Collections.Specialized;
using System.IO;
using System.Data.SqlTypes;
using System.Threading;

namespace CraigslistAutoPoll
{
    public partial class Service : ServiceBase
    {

        private struct AsyncRequestStruct
        {
            public HttpWebRequest request;
            public object parameters;
            public Stopwatch stopwatch;
            public IWebProxy proxy;
            public string IP;
            public DataAccessDataContext DataContext;
        }

        Dictionary<string, Dictionary<Listing, int>> ListingFailures = new Dictionary<string, Dictionary<Listing, int>>();

        int ConnectionCooldown;

        //The following four sets of properties are used to benchmark the application.
        int parseFeedProcessingCount = 0;
        TimeSpan parseFeedProcessingTime = new TimeSpan();

        int parseFeedConnectionCount = 0;
        TimeSpan parseFeedConnectionTime = new TimeSpan();

        int parseInfoProcessingCount = 0;
        TimeSpan parseInfoProcessingTime = new TimeSpan();

        int parseInfoConnectionCount = 0;
        TimeSpan parseInfoConnectionTime = new TimeSpan();

        //These dictionaries are used to cache objects by IP for each datacontext.  Each context needs its own reference to support multithreading.
        Dictionary<string, CLSubCity[]> SubCities = new Dictionary<string,CLSubCity[]>();
        Dictionary<string, CLCity[]> Cities = new Dictionary<string,CLCity[]>();
        Dictionary<string, CLSiteSection[]> SiteSections = new Dictionary<string,CLSiteSection[]>();

        //Contains all feeds to be parsed, grouped in a dictionary by IP address.
        Dictionary<string, LinkedList<Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>>> FeedQueues = new Dictionary<string, LinkedList<Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>>>();
        Dictionary<string, Queue<Listing>> ListingQueues = new Dictionary<string, Queue<Listing>>();

        //Thread lockers.
        object MasterKey = new object();
        object EventLogKey = new object();
        Dictionary<string, object> KeyChain = new Dictionary<string, object>();

        //These lists are caches like the above, but may be used globally for all datacontexts.
        string[] IPs = null;
        Dictionary<string, List<long>> CompletedListingIds = new Dictionary<string,List<long>>();    //A list of all existing posting IDs, used to prevent double-parsing.
        Dictionary<string, List<Listing>> ProcessingListings = new Dictionary<string, List<Listing>>();     //Tracks listings after they are dequeued, but not finished being parsed.
        Dictionary<string, List<Listing>> PreQueueListings = new Dictionary<string, List<Listing>>();  //Tracks listings that are initialized, but before they are added to the listing queue for future processing.

        public Service()
        {
            InitializeComponent();            
        }

        protected override void OnStart(string[] args)
        {
            init(); //We put the init into a function so that Tester project may call init to debug the code.
        }

        protected override void OnStop() {}

        public void init()
        {
            EventLog.Source = "Craigslist Crawler";
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;

            ConnectionCooldown = Properties.Settings.Default.ConnectionCooldown;
            try
            {
                DataAccessDataContext dadc = new DataAccessDataContext();
                IPs = dadc.CLCities.Where(x => x.Enabled).Select(x => x.IP).Distinct().ToArray();
                var proxies = dadc.Proxies.Where(x => x.Enabled).ToArray();
                foreach (string ip in IPs)
                {
                    DataAccessDataContext datacontext = new DataAccessDataContext();
                    FeedQueues.Add(ip, new LinkedList<Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>>());
                    ListingQueues.Add(ip, new Queue<Listing>());
                    ListingFailures.Add(ip, new Dictionary<Listing,int>());
                    ProcessingListings.Add(ip, new List<Listing>());
                    PreQueueListings.Add(ip, new List<Listing>());

                    Cities.Add(ip, datacontext.CLCities.Where(x => x.Enabled && x.IP == ip).ToArray());
                    SubCities.Add(ip, datacontext.CLSubCities.ToArray());
                    SiteSections.Add(ip, datacontext.CLSiteSections.Where(x => x.Enabled).ToArray());
                    CompletedListingIds.Add(ip, datacontext.Listings.Where(x=>x.CLCity.IP==ip).Select(x=>x.Id).ToList());
                    KeyChain.Add(ip, new object());

                    BuildFeedQueue(ip, datacontext);
                    if (proxies.Length > 0)
                    {
                        foreach (Proxy prox in proxies)
                        {
                            FetchNextWhatchamacallit(ip, new WebProxy(prox.IP, prox.Port), datacontext);
                        }
                    }
                    else
                    {
                        FetchNextWhatchamacallit(ip, null, datacontext);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (EventLogKey)
                {
                    EventLog.WriteEntry("An error occurred while attempting to retrieve database records.\n\n" + ex.Message, EventLogEntryType.Error);
                }
                return;
            }

            lock (EventLogKey)
            {
                EventLog.WriteEntry("Initialization Success!");
            }
        }

        private void SubmitData(DataAccessDataContext DataContext)
        {
            try
            {
                lock (MasterKey)
                {
                    if (DataContext.GetChangeSet().Inserts.OfType<Listing>().Count() >= Properties.Settings.Default.MinSubmissionBundleSize)
                        DataContext.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                StringBuilder debugmsg = new StringBuilder();

                var DuplicateListings = DataContext.Listings.GroupBy(x => x.Id).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
                var DuplicateAttributes = DataContext.ListingAttributes.GroupBy(x => x.AttributeID).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();

                debugmsg.AppendLine("Duplicate Listings:");
                foreach (long listingid in DuplicateListings)
                {
                    debugmsg.AppendLine("[" + listingid + "]");
                }

                debugmsg.AppendLine("Duplicate Attributes:");
                foreach (int attrid in DuplicateAttributes)
                {
                    debugmsg.AppendLine("[" + attrid + "]");
                }

                debugmsg.AppendLine("Existing Listings:");
                foreach(Listing li in DataContext.GetChangeSet().Inserts.OfType<Listing>())
                {
                    if (DataContext.Listings.Any(x=>x.Id==li.Id))
                    {
                        debugmsg.AppendLine("[" + li.Id + "]");
                    }
                }

                debugmsg.AppendLine("Existing Attributes:");
                foreach (ListingAttribute li in DataContext.GetChangeSet().Inserts.OfType<ListingAttribute>())
                {
                    if (DataContext.ListingAttributes.Any(x => x.AttributeID == li.AttributeID))
                    {
                        debugmsg.AppendLine("[" + li.AttributeID + "]");
                    }
                }

                string wholemessage = "Failed to submit new listings to database.\n\n" + ex.Message + "\n\n" + debugmsg.ToString();
                wholemessage = wholemessage.Substring(0, Math.Min(short.MaxValue, wholemessage.Length));

                lock (EventLogKey)
                {
                    EventLog.WriteEntry(wholemessage, EventLogEntryType.Error);
                }
            }
        }

        public List<long> testids = new List<long>();
        public void FetchNextWhatchamacallit(string IP, IWebProxy proxy, DataAccessDataContext DataContext)
        {
            Queue<Listing> ListingQueue = ListingQueues[IP];            

            lock (KeyChain[IP])
            {
                
                if (ListingQueue.Count > 0)
                {                    

                    HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://" + ListingQueue.Peek().CLCity.Name + ".craigslist.org/" + (ListingQueue.Peek().CLSubCity == null ? "" : (ListingQueue.Peek().CLSubCity.SubCity + "/")) + ListingQueue.Peek().CLSiteSection.Name + "/" + ListingQueue.Peek().Id.ToString() + ".html");
                    {
                        hwr.Proxy = proxy;
                        hwr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        hwr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                        hwr.KeepAlive = false;
                        hwr.UserAgent = Properties.Settings.Default.UserAgent;

                        AsyncRequestStruct ars = new AsyncRequestStruct() { request = hwr, parameters = ListingQueue.Peek(), IP = IP, proxy = proxy, DataContext = DataContext };

                        ars.stopwatch = new Stopwatch();
                        ars.stopwatch.Start();

                        hwr.BeginGetResponse(new AsyncCallback(ParseListingInfo), ars);

                    }

                    ProcessingListings[IP].Add(ListingQueue.Dequeue());
                }
                else
                {

                    if (FeedQueues[IP].Count == 0)
                        BuildFeedQueue(IP, DataContext);

                    var selectedItem = FeedQueues[IP].First.Value;
                    FeedQueues[IP].RemoveFirst();

                    HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://" + selectedItem.Item2.Name + ".craigslist.org/search/" + (selectedItem.Item5 == null ? "" : (selectedItem.Item5.SubCity + "/")) + selectedItem.Item1.Name + "?s=" + selectedItem.Item3.ToString());

                    {
                        hwr.Proxy = proxy;
                        hwr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        hwr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                        hwr.KeepAlive = false;
                        hwr.UserAgent = Properties.Settings.Default.UserAgent;
                        AsyncRequestStruct ars = new AsyncRequestStruct() { request = hwr, parameters = selectedItem, IP = IP, proxy = proxy, DataContext = DataContext };

                        ars.stopwatch = new Stopwatch();
                        ars.stopwatch.Start();

                        hwr.BeginGetResponse(new AsyncCallback(ParseFeed), ars);
                    }
                }
            }
        }

        private void ParseFeed(IAsyncResult AsyncResult)
        {    
            bool banned403 = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            AsyncRequestStruct ars = (AsyncRequestStruct)AsyncResult.AsyncState;
            try
            {
                ars.stopwatch.Stop();
                parseFeedConnectionCount++;
                parseFeedConnectionTime += ars.stopwatch.Elapsed;

                Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity> feedResource = (Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>)ars.parameters;
                CLSiteSection SiteSection = feedResource.Item1;
                CLCity City = feedResource.Item2;
                int depth = feedResource.Item3;
                DateTime lastStamp = feedResource.Item4;
                CLSubCity subCity = feedResource.Item5;

                HtmlDocument response = new HtmlDocument();
                try
                {
                    using (WebResponse wr = ars.request.EndGetResponse(AsyncResult))
                    {
                        using (Stream stream = wr.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                response.LoadHtml(sr.ReadToEnd());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(403)"))    //If a 403 occurs, remove the proxy from operation.
                    {
                        if (Properties.Settings.Default.DisabledBannedProxies)
                        {
                            WebProxy wp = (WebProxy)ars.request.Proxy;

                            Proxy selectedProxy = ars.DataContext.Proxies.FirstOrDefault(x => x.IP == wp.Address.Host && x.Port == wp.Address.Port);
                            if (selectedProxy != null)
                            {
                                selectedProxy.Enabled = false;
                                lock (KeyChain[ars.IP])
                                {
                                    lock (MasterKey)
                                    {
                                        ars.DataContext.SubmitChanges();
                                    }
                                }
                            }
                        }
                        banned403 = true;
                        return;
                    }

                    lock (EventLogKey)
                    {
                        EventLog.WriteEntry("An error has occurred while retreiving a feed:\n\n" + ex.Message + "\n\n" + (ex.InnerException == null ? "" : ex.InnerException.Message)
                            + "\n\n" + SiteSection.Name + "\n" + City.Name + "\n" + depth.ToString(), EventLogEntryType.Warning);
                    }
                    return;
                }

                HtmlNodeCollection rows = response.DocumentNode.SelectNodes("//p[@class='row' and @data-pid]");

                if (rows == null)
                {
                    //The eventlog below is not necessarily true.  It's possible that the end of the feeds has been reached and there are no more listings.
                    //EventLog.WriteEntry("Error while parsing feed.  No connection errors thrown, but no results returned either.\n" + SiteSection.Name + "\n" + City.Name + "\n" + (subCity==null?"":("\n"+subCity.SubCity)) + "\n" + depth.ToString());
                    return;
                }
                bool NoUpdateTimesPosted = false;

                foreach (HtmlNode row in rows)
                {
                        if (row.SelectSingleNode("a").Attributes["href"].Value.Contains("craigslist.org"))
                        {   //This condition deals with "Nearby Areas".  Links within the locale are relative -> /<sectionCode>/<Id>.html, whereas out of the locale are global -> http://<city.Name>/craigslist.org/<sectionCode>/<Id>.html
                            return;
                        }

                        Listing listingSource = new Listing() { CLSiteSection = SiteSection, CLCity = City, CLSubCity = subCity, Timestamp = DateTime.Now };

                        //LASTUPDATED
                        try
                        {
                            HtmlNode hn = row.SelectSingleNode("span[@class='txt']/span[@class='pl']/time");
                            if (hn != null && DateTime.Parse(hn.Attributes["datetime"].Value) <= lastStamp)
                                return;
                            else if(hn == null)
                                NoUpdateTimesPosted = true;
                        }
                        catch (Exception ex)
                        {
                            lock (EventLogKey)
                            {
                                EventLog.WriteEntry("Failure to parse listing update time.\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Error);
                            }
                            return;
                        }

                        //ID
                        try
                        {
                            listingSource.Id = long.Parse(row.Attributes["data-pid"].Value);
                        }
                        catch (Exception ex)
                        {
                            lock (EventLogKey)
                            {
                                EventLog.WriteEntry("Listing has an invalid ID:\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                            }
                            continue;
                        }

                        lock (KeyChain[ars.IP])
                        {
                            if (CompletedListingIds[ars.IP].Contains(listingSource.Id) 
                                || ListingQueues[ars.IP].Any(x => x.Id == listingSource.Id) 
                                || ProcessingListings[ars.IP].Any(x=>x.Id == listingSource.Id) 
                                || PreQueueListings[ars.IP].Any(x=>x.Id == listingSource.Id)
                            )
                            {
                                continue;
                            }
                            PreQueueListings[ars.IP].Add(listingSource);
                        }
                        //PRICE
                        {
                            HtmlNode hn = row.SelectSingleNode("span[@class='txt']/span[@class='l2']/span[@class='price']");
                            if (hn != null)
                            {
                                try
                                {
                                    string value = long.Parse(hn.InnerText.Replace("$", "").Replace(@"&#x0024;", "")).ToString();
                                    listingSource.ListingAttributes.Add(
                                        new ListingAttribute()
                                        {
                                            Name = "price",
                                            Value = value
                                        }
                                    );
                                }
                                catch (Exception ex)
                                {
                                    lock (EventLogKey)
                                    {
                                        EventLog.WriteEntry("Error parsing price listing data:  " + hn.InnerText.Replace("$", "").Replace(@"&#x0024;", "") + "\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                                    }
                                }
                            }
                        }

                        //LOCALE
                        {
                            HtmlNode hn = row.SelectSingleNode("span[@class='txt']/span[@class='l2']/span[@class='pnr']/small");
                            if (hn != null)
                            {
                                try
                                {
                                    listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Locale", Value = hn.InnerText.Replace("(", "").Replace(")", "").Trim() });
                                }
                                catch (Exception ex)
                                {
                                    lock (EventLogKey)
                                    {
                                        EventLog.WriteEntry("Error parsing listing locale data:  " + hn.InnerText.Replace("(", "").Replace(")", "").Trim() + "\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                                    }
                                }
                            }
                        }

                        //HAS PICTURE, HAS MAP
                        try
                        {
                            HtmlNode hn = row.SelectSingleNode("span[@class='txt']/span[@class='l2']/span[@class='pnr']/span[@class='px']/span[@class='p']");
                            if (hn != null)
                            {
                                if (hn.InnerText.Contains("pic"))
                                {
                                    listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Has Picture", Value = "True" });
                                }
                                if (hn.InnerText.Contains("map"))
                                {
                                    listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Has Map", Value = "True" });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (EventLogKey)
                            {
                                EventLog.WriteEntry("Error while parsing pic and map presence:\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                            }
                        }

                        //TITLE
                        try
                        {
                            listingSource.Title = row.SelectSingleNode("span[@class='txt']/span[@class='pl']/a[@class='hdrlnk']").InnerText;
                        }
                        catch (Exception ex)
                        {
                            lock (EventLogKey)
                            {
                                EventLog.WriteEntry("Listing skipped.  Error parsing title from listing.\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                            }
                            lock(KeyChain[ars.IP])
                            {
                                PreQueueListings[ars.IP].Remove(listingSource);
                            }
                            continue;
                        }

                        lock (KeyChain[ars.IP])
                        {
                            ListingQueues[ars.IP].Enqueue(listingSource);
                            ProcessingListings[ars.IP].Remove(listingSource);
                            PreQueueListings[ars.IP].Remove(listingSource);
                        }
                    }

                //If the for loop completes without a return, the next page needs to be looked at
                if (NoUpdateTimesPosted == false)
                {
                    lock (KeyChain[ars.IP])
                    {
                        FeedQueues[ars.IP].AddFirst(new Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>(feedResource.Item1, feedResource.Item2, feedResource.Item3 + 100, feedResource.Item4, feedResource.Item5));
                    }
                }
                
            }
            catch (Exception ex)
            {
                lock (EventLogKey)
                {
                    EventLog.WriteEntry("An error has occurred while parsing the feed:\n\n" + PrintException(ex), EventLogEntryType.Error);
                }
            }
            finally
            {
                sw.Stop();
                parseFeedProcessingTime += sw.Elapsed;
                parseFeedProcessingCount++;

                if (ConnectionCooldown > 0 && sw.Elapsed < TimeSpan.FromMilliseconds(ConnectionCooldown))
                    Thread.Sleep(TimeSpan.FromMilliseconds(ConnectionCooldown) - sw.Elapsed);

                try
                {
                    if (banned403 == false)
                        FetchNextWhatchamacallit(ars.IP, ars.proxy, ars.DataContext);
                }
                catch (Exception ex)
                {
                    lock (EventLogKey)
                    {
                        EventLog.WriteEntry("Error fetching requests from next queue.\n\n" + PrintException(ex));
                    }
                }
            }
        }

        public void ParseListingInfo(IAsyncResult AsyncResult)
        {
            bool banned403 = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            AsyncRequestStruct ars = (AsyncRequestStruct)AsyncResult.AsyncState;
            try
            {
                ars.stopwatch.Stop();
                parseInfoConnectionCount++;
                parseInfoConnectionTime += ars.stopwatch.Elapsed;

                Listing listingSource = (Listing)ars.parameters;

                HtmlDocument response = new HtmlDocument();
                try
                {
                    using (WebResponse wr = ars.request.EndGetResponse(AsyncResult))
                    {
                        using (Stream stream = wr.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                response.LoadHtml(sr.ReadToEnd());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("(403)"))    //If a 403 occurs, remove the proxy from operation.
                    {
                        if (Properties.Settings.Default.DisabledBannedProxies)
                        {
                            WebProxy wp = (WebProxy)ars.request.Proxy;
                            lock (KeyChain[ars.IP])
                            {
                                Proxy selectedProxy = ars.DataContext.Proxies.FirstOrDefault(x => x.IP == wp.Address.Host && x.Port == wp.Address.Port);
                                if (selectedProxy != null)
                                {
                                    selectedProxy.Enabled = false;

                                    lock (MasterKey)
                                    {
                                        ars.DataContext.SubmitChanges();
                                    }
                                }
                            }
                        }

                        lock (KeyChain[ars.IP])
                        {
                            ListingQueues[ars.IP].Enqueue(listingSource);
                            ProcessingListings[ars.IP].Remove(listingSource);
                        }

                        lock (EventLogKey)
                        {
                            EventLog.WriteEntry("Proxy has been banned." + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                        }

                        banned403 = true;
                    }
                    else
                    {
                        lock (KeyChain[ars.IP])
                        {
                            if (ListingFailures[ars.IP].Keys.Contains(listingSource))
                            {
                                ListingFailures[ars.IP][listingSource]++;
                                if (ListingFailures[ars.IP][listingSource] > Properties.Settings.Default.MaxConnectionRetries)
                                {
                                    ListingFailures[ars.IP].Remove(listingSource);
                                }
                                else
                                {
                                    ListingQueues[ars.IP].Enqueue(listingSource);
                                    ProcessingListings[ars.IP].Remove(listingSource);
                                }
                            }
                            else
                            {
                                ListingFailures[ars.IP].Add(listingSource, 1);
                                ListingQueues[ars.IP].Enqueue(listingSource);
                                ProcessingListings[ars.IP].Remove(listingSource);
                            }
                        }

                        lock (EventLogKey)
                        {
                            EventLog.WriteEntry("An error has occurred while retrieving the listing info:\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                        }
                    }
                    
                    return;
                }

                //LISTING WAS DELETED
                {
                    HtmlNode hn = response.DocumentNode.SelectSingleNode("//div[@class='removed']");
                    if (hn != null)
                    {
                        lock (KeyChain[ars.IP])
                        {
                            ProcessingListings[ars.IP].Remove(listingSource);
                        }
                        return;
                    }
                }

                //POSTDATE
                try
                {
                    listingSource.PostDate = DateTime.Parse(response.DocumentNode.SelectSingleNode("//p[@id='display-date']/time").Attributes["datetime"].Value);
                }
                catch (Exception ex)
                {
                    lock (EventLogKey)
                    {
                        EventLog.WriteEntry("Listing skipped.  Post date could not be identified or parsed.\n\n" + PrintException(ex) + PrintListing(listingSource) + ex.Message, EventLogEntryType.Warning);
                    }
                    lock (KeyChain[ars.IP])
                    {
                        ProcessingListings[ars.IP].Remove(listingSource);
                    }
                    return;
                }

                //GPS COORDINATES
                if (listingSource.ListingAttributes.Any(x => x.Name == "Has Map"))
                {
                    try
                    {
                        HtmlNode hn = response.DocumentNode.SelectSingleNode("//div[@id='map' and @data-latitude and @data-longitude and @data-accuracy]");
                        listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Latitude", Value = hn.Attributes["data-latitude"].Value });
                        listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Longitude", Value = hn.Attributes["data-longitude"].Value });
                        listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Location Accuracy", Value = hn.Attributes["data-accuracy"].Value });
                    }
                    catch (Exception ex)
                    {
                        lock (EventLogKey)
                        {
                            EventLog.WriteEntry("Error parsing GPS coordinates:\n\n" + PrintException(ex) + PrintListing(listingSource));
                        }
                    }
                }

                //ADDRESS
                {
                    HtmlNode hn = response.DocumentNode.SelectSingleNode("//div[@class='mapaddress']");
                    if (hn != null)
                    {
                        try
                        {
                            listingSource.ListingAttributes.Add(new ListingAttribute() { Name = "Address", Value = hn.InnerText });
                        }
                        catch (Exception ex)
                        {
                            lock (EventLogKey)
                            {
                                EventLog.WriteEntry("Error parsing map address: \n\n" + ex.Message);
                            }
                        }
                    }
                }

                int[] yearList = Enumerable.Range(1900, 120).ToArray();
                HtmlNodeCollection hnc = response.DocumentNode.SelectNodes("//p[@class='attrgroup']/span");
                if (hnc == null) return;  //returns NULL if no nodes exist.

                foreach (HtmlNode hn in hnc)
                {
                    string[] parts = hn.InnerText.Split(':');
                    if (parts.Length == 2)
                    {
                        listingSource.ListingAttributes.Add(
                            new ListingAttribute()
                            {
                                Name = parts[0].Trim(),
                                Value = parts[1].Trim()
                            }
                        );
                    }
                    else if (parts.Length == 1)     //More ads by this user is styled like an attribute, but is actually just a link.  It provides no info besides userid.
                    {
                        listingSource.ListingAttributes.Add(
                            new ListingAttribute()
                            {
                                Name = "Unspecified",
                                Value = parts[0].Trim()
                            }
                        );
                    }
                }

                //BODY
                try
                {
                    HtmlNode hn = response.DocumentNode.SelectSingleNode("//section[@id='postingbody']");
                    listingSource.Body = hn.InnerText;
                }
                catch (Exception ex)
                {
                    lock (KeyChain[ars.IP])
                    {
                        ProcessingListings[ars.IP].Remove(listingSource);
                    }

                    lock (EventLogKey)
                    {
                        EventLog.WriteEntry("An error has occurred while parsing the listing body: \n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                    }
                    return;
                }

                lock (KeyChain[ars.IP])
                {
                    ars.DataContext.Listings.InsertOnSubmit(listingSource);
                    CompletedListingIds[ars.IP].Add(listingSource.Id);
                    ProcessingListings[ars.IP].Remove(listingSource);
                    SubmitData(ars.DataContext);
                }
            }
            finally
            {
                sw.Stop();
                parseInfoProcessingTime += sw.Elapsed;
                parseInfoProcessingCount++;

                if (ConnectionCooldown > 0 && sw.Elapsed < TimeSpan.FromMilliseconds(ConnectionCooldown))
                    Thread.Sleep(TimeSpan.FromMilliseconds(ConnectionCooldown) - sw.Elapsed);

                try
                {
                    if (banned403 == false)
                        FetchNextWhatchamacallit(ars.IP, ars.proxy, ars.DataContext);
                }
                catch (Exception ex)
                {
                    lock (EventLogKey)
                    {
                        EventLog.WriteEntry("Error fetching requests from next queue.\n\n" + ex.Message);
                    }
                }
            }
        }

        public void BuildFeedQueue(string IP, DataAccessDataContext DataContext)
        {
            try
            {
                foreach (var item in DataContext.GetFeedList(IP).OrderBy(x => x.Timestamp))
                {
                    FeedQueues[IP].AddLast(new Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>(SiteSections[IP].First(x => x.Name == item.SiteSection), Cities[IP].First(x => x.Name == item.City), 0, item.Timestamp, SubCities[IP].FirstOrDefault(x => x.SubCity == item.SubCity)));
                }
            }
            catch (Exception ex)
            {
                lock (EventLogKey)
                {
                    EventLog.WriteEntry("An error occurred while attempting to retrieve database records.\n\n" + ex.Message, EventLogEntryType.Error);
                }
            }
        }

        public string PrintListing(Listing listingSource)
        {
            return listingSource.CLSiteSection.Name + (listingSource.CLSubCity == null ? "" : ("\n" + listingSource.CLSubCity.SubCity)) + "\n" + listingSource.CLCity.Name + "\n" + listingSource.Id.ToString();
        }

        public string PrintException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            do
            {
                sb.Append(ex.Message);
                sb.Append("\n\n");
                ex = ex.InnerException;
            } while (ex != null);
            return sb.ToString();
        }
    }
}
