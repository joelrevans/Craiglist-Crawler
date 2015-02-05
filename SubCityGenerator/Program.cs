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
            DataAccessDataContext dadc = new DataAccessDataContext();
            StringBuilder sb = new StringBuilder();

            foreach(CLCity city in dadc.CLCities)
            {
                Console.WriteLine(city.Name);
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
                File.WriteAllText("citybits.txt", sb.ToString());
            }
        }
    }
}
