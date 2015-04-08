using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CraigslistAutoPoll;
using System.Net;
using HtmlAgilityPack;
using System.Data.Linq;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Specialized;
using System.IO;

namespace SubCityGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            GetDomainAddresses();
        }

        static void GetDomainAddresses()
        {
            DataAccessDataContext dadc = new DataAccessDataContext();
            StringBuilder sb = new StringBuilder();
            foreach(CLCity city in dadc.CLCities)
            {
                IPHostEntry ipe = Dns.GetHostEntry(city.Name + ".craigslist.org");
                sb.AppendLine(city.Name.PadRight(15) + string.Join<IPAddress>(", ", ipe.AddressList));
                city.IP = ipe.AddressList.FirstOrDefault().ToString();
            }
            dadc.SubmitChanges();
            File.WriteAllText("CityAddresses.txt", sb.ToString());
        }

        static void GetSubCities()
        {
            DataAccessDataContext dadc = new DataAccessDataContext();
            StringBuilder sb = new StringBuilder();

            foreach (CLCity city in dadc.CLCities)
            {
                WebClient wc = new WebClient();
                string ret = wc.DownloadString("http://" + city.Name + ".craigslist.org/search/sss");
                HtmlDocument hd = new HtmlDocument();
                hd.LoadHtml(ret);
                HtmlNodeCollection nodes = hd.DocumentNode.SelectNodes("//select[@id='subArea']/option[@value and string-length(@value)]");

                if (nodes == null)
                    continue;

                foreach (HtmlNode hn in nodes)
                {
                    sb.AppendLine(hn.Attributes["value"].Value + ", " + city.Name);
                }
                File.WriteAllText("SubCities.txt", sb.ToString());
            }
        }
    }
}
