using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Services;
using System.Xml;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace wsgenerales_master_csharp.App_Code.Maps
{
    public class HereMapsNew
    {
        public string ProviderGeo = WebConfigurationManager.AppSettings.Get("hereNewProviderGeo");
        public string ProviderDist = WebConfigurationManager.AppSettings.Get("hereNewProviderDist");
        private string ApiKey = WebConfigurationManager.AppSettings.Get("hereNewApiKey");
        private string ApiKey2 = WebConfigurationManager.AppSettings.Get("hereNewApiKey2");

        public string GetApiKey()
        {
            string getApiKey = "";
            try
            {
                switch (new Random().Next(1, 3))
                {
                    case 1:
                        getApiKey = ApiKey;
                        break;
                    case 2:
                        getApiKey = ApiKey2;
                        break;
                }
            }
            catch
            {
            }

            return getApiKey;
        }

        public string GetGeo(string url)
        {
            string GetGeoByHere = "";
            HttpWebRequest request;
            HttpWebResponse response = null;

            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                response = request.GetResponse() as HttpWebResponse;

                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    string rawresp = reader.ReadToEnd();

                    JObject outerObject = JToken.Parse(rawresp).ToObject<JObject>();
                    JToken inner = outerObject["items"];
                    JToken position = inner[0]["position"];
                    JToken latitude = position["lat"];
                    JToken longitude = position["lng"];
                    GetGeoByHere = latitude.ToString() + "/" + longitude.ToString();
                }
            }
            catch
            {
                GetGeoByHere = "0/0";
            }
            return GetGeoByHere;
        }

        public string GetGeoUrlProvider(string pDireccion)
        {
            return ProviderGeo + pDireccion + "&apiKey=" + this.GetApiKey();
        }

        public string GetDistUrlProvider(string pLatMov, string pLngMov, string pLatDst, string pLngDst)
        {
            return ProviderDist + pLatMov + "," + pLngMov
                            + "&destination=" + pLatDst + "," + pLngDst
                            + "&return=summary&apiKey=" + this.GetApiKey();
        }

        public string GetDist(string url)
        {
            string GetDistByHere;
            HttpWebRequest request;
            HttpWebResponse response = null;

            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                response = request.GetResponse() as HttpWebResponse;

                WebHeaderCollection header = response.Headers;

                var encoding = ASCIIEncoding.ASCII;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    string rawresp = reader.ReadToEnd();

                    JObject outerObject = JToken.Parse(rawresp).ToObject<JObject>();
                    JToken routes = outerObject["routes"];
                    JToken sections = routes[0]["sections"];
                    JToken summary = sections[0]["summary"];
                    JToken length = summary["length"];
                    JToken duration = summary["baseDuration"]; // summary["duration"];
                    string dist = Math.Round(Convert.ToDecimal(length) / 1000, 2).ToString() + " km";
                    string time = Math.Round(Convert.ToDecimal(duration) / 60, 0).ToString() + " minutes";
                    GetDistByHere = dist + "/" + time;

                }
            }
            catch
            {
                GetDistByHere = "0/0";
            }
            return GetDistByHere;
        }
    }
}