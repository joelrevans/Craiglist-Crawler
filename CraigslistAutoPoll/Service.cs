using System;
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

namespace CraigslistAutoPoll
{
    public partial class Service : ServiceBase
    {

        private struct AsyncRequestStruct
        {
            public HttpWebRequest request;
            public object parameters;
            public Stopwatch stopwatch;
        }

        public LinkedList<Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>> FeedQueue = new LinkedList<Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>>();
        public Queue<Listing> ListingInfoQueue = new Queue<Listing>();
        public Queue<Listing> ListingBodyQueue = new Queue<Listing>();
        public Dictionary<Listing, int> ListingFailures = new Dictionary<Listing, int>();

        public Queue<WebProxy> Proxies = new Queue<WebProxy>();
        public int AvailableConnections = 0;

        public List<long> PostingIds = null;
        DataAccessDataContext dadc = new DataAccessDataContext();
        object key = new object();

#if DEBUG
        int AllConnectionsCount = 0;
        TimeSpan AllConnectionsTime = new TimeSpan();
#endif

        
        public Service()
        {
            InitializeComponent();            
        }

        protected override void OnStart(string[] args)
        {
            init(); //We put the init into a function so that Tester project may debug the code.
        }

        protected override void OnStop() {}

        public void init()
        {
            EventLog.Source = "Craigslist Crawler";
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            //ServicePointManager.MaxServicePointIdleTime = 0;// Properties.Settings.Default.ConnectionTimeout;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;

            AvailableConnections = Properties.Settings.Default.MaxConcurrentConnections;

            try
            {
                foreach (Proxy prox in dadc.Proxies)
                {
                    if (prox.Enabled)
                        Proxies.Enqueue(new WebProxy(prox.IP, prox.Port));
                }

                PostingIds = dadc.Listings.Select(x => x.Id).ToList();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("An error occurred while attempting to retrieve database records.\n\n" + ex.Message, EventLogEntryType.Error);
            }

            try
            {
                FetchNextWhatchamacallit();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message);
            }
        }

        private void SubmitData()
        {
            try
            {
                if (dadc.GetChangeSet().Inserts.OfType<Listing>().Count() >= Properties.Settings.Default.MinSubmissionBundleSize)
                {
                    dadc.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                StringBuilder debugmsg = new StringBuilder();
                /*The following is too large to use normally and may surpass the event log character limit.*/
                foreach (Listing li in dadc.GetChangeSet().Inserts.OfType<Listing>())
                {
                    debugmsg.Append("[" + li.Id + "]:  " + li.Title + "\n");
                    foreach (ListingAttribute la in li.ListingAttributes.OrderBy(x => x.Name))
                    {
                        debugmsg.Append("\t[" + la.Name + "]:  " + la.Value + "\n");
                    }
                    debugmsg.Append("\n");
                }
                if (debugmsg.Length > short.MaxValue - 200)
                {
                    debugmsg.Remove(short.MaxValue - 200, debugmsg.Length - (short.MaxValue-200));
                }

                EventLog.WriteEntry("Failed to submit new listings to database.\n\n" + ex.Message + "\n\n" + debugmsg.ToString(), EventLogEntryType.Error);
            }
        }

        public void FetchNextWhatchamacallit()
        {

            while (ListingInfoQueue.Count > 0 && AvailableConnections > 0)
            {
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://" + ListingInfoQueue.Peek().CLCity.Name + ".craigslist.org/" + (ListingInfoQueue.Peek().CLSubCity == null ? "" : (ListingInfoQueue.Peek().CLSubCity.SubCity + "/")) + ListingInfoQueue.Peek().CLSiteSection.Name + "/" + ListingInfoQueue.Peek().Id.ToString() + ".html");
                {
                    if (Proxies.Count > 0)
                    {
                        hwr.Proxy = Proxies.Peek();
                        Proxies.Enqueue(Proxies.Dequeue());
                    }
                    else
                        hwr.Proxy = null;
                    hwr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    hwr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                    AsyncRequestStruct ars = new AsyncRequestStruct() { request = hwr, parameters = ListingInfoQueue.Peek() };
#if DEBUG
                    ars.stopwatch = new Stopwatch();
                    ars.stopwatch.Start();
#endif
                    hwr.BeginGetResponse(new AsyncCallback(ParseListingInfo), ars);
                }
                AvailableConnections--;
                ListingInfoQueue.Dequeue();
            }

            while (AvailableConnections > 0)
            {
                if (FeedQueue.Count == 0)
                {
                    try
                    {
                        var feedlist = dadc.GetFeedList().OrderBy(x=>(x.Timestamp==null?((DateTime)SqlDateTime.MinValue):x.Timestamp));
                        CLSubCity[] subcities = dadc.CLSubCities.ToArray();
                        CLCity[] cities = dadc.CLCities.ToArray();
                        CLSiteSection[] sitesections = dadc.CLSiteSections.ToArray();

                        foreach (var item in feedlist)
                            FeedQueue.AddLast(new Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>(sitesections.First(x => x.Name == item.SiteSection), cities.First(x => x.Name == item.City), 0, item.Timestamp, subcities.FirstOrDefault(x => x.SubCity == item.SubCity)));
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("An error occurred while attempting to retrieve database records.\n\n" + ex.Message, EventLogEntryType.Error);
                        return;
                    }
                }

                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://" + FeedQueue.First.Value.Item2.Name + ".craigslist.org/search/" + (FeedQueue.First.Value.Item5 == null ? "" : (FeedQueue.First.Value.Item5.SubCity + "/")) + FeedQueue.First.Value.Item1.Name + "?s=" + FeedQueue.First.Value.Item3.ToString());
                {
                    if (Proxies.Count > 0)
                    {
                        hwr.Proxy = Proxies.Peek();
                        Proxies.Enqueue(Proxies.Dequeue());
                    }
                    else
                        hwr.Proxy = null;
                    hwr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    hwr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                    AsyncRequestStruct ars = new AsyncRequestStruct() { request = hwr, parameters = FeedQueue.First.Value };
#if DEBUG
                    ars.stopwatch = new Stopwatch();
                    ars.stopwatch.Start();
#endif
                    hwr.BeginGetResponse(new AsyncCallback(ParseFeed), ars);
                }
                AvailableConnections--;
                FeedQueue.RemoveFirst();
            }       
        }

        private void ParseFeed(IAsyncResult AsyncResult)
        {
            AvailableConnections++;
            lock (key)
            {
                try
                {

                    AsyncRequestStruct ars = (AsyncRequestStruct)AsyncResult.AsyncState;
#if DEBUG
                    ars.stopwatch.Stop();
                    AllConnectionsCount++;
                    AllConnectionsTime += ars.stopwatch.Elapsed;
#endif

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
                        if (ex.Message.Contains("(403)") && Properties.Settings.Default.DisableForbiddenProxies)    //If a 403 occurs, remove the proxy from operation.
                        {
                            WebProxy wp = (WebProxy)ars.request.Proxy;
                            for (int i = 0; i < Proxies.Count; ++i)
                            {
                                if (Proxies.Peek() == wp)
                                {
                                    Proxy selectedProxy = dadc.Proxies.FirstOrDefault(x => x.IP == Proxies.Peek().Address.Host && x.Port == Proxies.Peek().Address.Port);
                                    if (selectedProxy != null)
                                    {
                                        selectedProxy.Enabled = false;
                                        dadc.SubmitChanges();
                                    }

                                    Proxies.Dequeue();
                                    if (Proxies.Count == 0)
                                        return;
                                    Proxies.Enqueue(Proxies.Dequeue());
                                    break;
                                }
                                Proxies.Enqueue(Proxies.Dequeue());
                            }
                        }

                        EventLog.WriteEntry("An error has occurred while retreiving a feed:\n\n" + ex.Message + "\n\n" + (ex.InnerException == null ? "" : ex.InnerException.Message)
                            + "\n\n" + SiteSection.Name + "\n" + City.Name + "\n" + depth.ToString(), EventLogEntryType.Warning);
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
                            EventLog.WriteEntry("Failure to parse listing update time.\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Error);
                            return;
                        }

                        //ID
                        try
                        {
                            listingSource.Id = long.Parse(row.Attributes["data-pid"].Value);
                        }
                        catch (Exception ex)
                        {
                            EventLog.WriteEntry("Listing has an invalid ID:\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                            continue;
                        }

                        if (PostingIds.Contains(listingSource.Id))
                        {
                            continue;
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
                                    EventLog.WriteEntry("Error parsing price listing data:  " + hn.InnerText.Replace("$", "").Replace(@"&#x0024;", "") + "\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
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
                                    EventLog.WriteEntry("Error parsing listing locale data:  " + hn.InnerText.Replace("(", "").Replace(")", "").Trim() + "\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                                }
                            }
                        }

                        //HAS PICTURE, HAS MAP
                        try{
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
                            EventLog.WriteEntry("Error while parsing pic and map presence:\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                        }

                        //TITLE
                        try
                        {
                            listingSource.Title = row.SelectSingleNode("span[@class='txt']/span[@class='pl']/a[@class='hdrlnk']").InnerText;
                        }
                        catch (Exception ex)
                        {
                            EventLog.WriteEntry("Listing skipped.  Error parsing title from listing.\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                            continue;
                        }

                        ListingInfoQueue.Enqueue(listingSource);
                    }
                    //If the for loop completes without a return, the next page needs to be looked at
                    if(NoUpdateTimesPosted == false)
                        FeedQueue.AddFirst(new Tuple<CLSiteSection, CLCity, int, DateTime, CLSubCity>(feedResource.Item1, feedResource.Item2, feedResource.Item3 + 100, feedResource.Item4, feedResource.Item5));
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("An error has occurred while parsing the feed:\n\n" + PrintException(ex), EventLogEntryType.Error);
                }
                finally
                {
                    try
                    {
                        FetchNextWhatchamacallit();
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Error fetching requests from next queue.\n\n" + PrintException(ex));
                    }
                }
            }
        }

        public void ParseListingInfo(IAsyncResult AsyncResult)
        {
            AvailableConnections++;
            lock (key)
            {
                try
                {
                    AsyncRequestStruct ars = (AsyncRequestStruct)AsyncResult.AsyncState;
#if DEBUG
                    ars.stopwatch.Stop();
                    AllConnectionsCount++;
                    AllConnectionsTime += ars.stopwatch.Elapsed;
#endif

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
                        if (ex.Message.Contains("(403)") && Properties.Settings.Default.DisableForbiddenProxies)    //If a 403 occurs, remove the proxy from operation.
                        {
                            WebProxy wp = (WebProxy)ars.request.Proxy;
                            for (int i = 0; i < Proxies.Count; ++i)
                            {
                                if (Proxies.Peek() == wp)
                                {
                                    Proxy selectedProxy = dadc.Proxies.FirstOrDefault(x => x.IP == Proxies.Peek().Address.Host && x.Port == Proxies.Peek().Address.Port);
                                    if (selectedProxy != null)
                                        selectedProxy.Enabled = false;

                                    Proxies.Dequeue();
                                    if (Proxies.Count == 0)
                                        return;
                                    Proxies.Enqueue(Proxies.Dequeue());
                                    break;
                                }
                                Proxies.Enqueue(Proxies.Dequeue());
                            }
                        }

                        if (ListingFailures.Keys.Contains(listingSource))
                        {
                            ListingFailures[listingSource]++;
                            if (ListingFailures[listingSource] > Properties.Settings.Default.MaxListingRetries)
                            {
                                ListingFailures.Remove(listingSource);
                            }
                            else
                            {
                                ListingInfoQueue.Enqueue(listingSource);
                            }
                        }
                        else
                        {
                            ListingFailures.Add(listingSource, 1);
                            ListingInfoQueue.Enqueue(listingSource);
                        }

                        EventLog.WriteEntry("An error has occurred while retrieving the listing info:\n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                        return;
                    }

                    //LISTING WAS DELETED
                    {
                        HtmlNode hn = response.DocumentNode.SelectSingleNode("//div[@class='removed']");
                        if (hn != null)
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
                            EventLog.WriteEntry("Error parsing GPS coordinates:\n\n" + PrintException(ex) + PrintListing(listingSource));
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
                                EventLog.WriteEntry("Error parsing map address: \n\n" + ex.Message);
                            }
                        }
                    }

                    //POSTDATE
                    try
                    {
                        listingSource.PostDate = DateTime.Parse(response.DocumentNode.SelectSingleNode("//p[@id='display-date']/time").Attributes["datetime"].Value);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Listing skipped.  Post date could not be identified or parsed.\n\n" + PrintException(ex) + PrintListing(listingSource) + ex.Message, EventLogEntryType.Warning);
                        return;
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
                        EventLog.WriteEntry("An error has occurred while parsing the listing body: \n\n" + PrintException(ex) + PrintListing(listingSource), EventLogEntryType.Warning);
                    }

                    if (PostingIds.Contains(listingSource.Id) == false)
                    {
                        dadc.Listings.InsertOnSubmit(listingSource);
                        PostingIds.Add(listingSource.Id);
                        SubmitData();
                    }
                    
                }
                finally
                {
                    try
                    {
                        FetchNextWhatchamacallit();
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Error fetching requests from next queue.\n\n" + ex.Message);
                    }
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
