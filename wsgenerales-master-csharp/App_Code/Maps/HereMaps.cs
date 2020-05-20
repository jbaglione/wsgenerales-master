using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;

namespace wsgenerales_master_csharp.App_Code.Maps
{
    public class HereMaps
    {
        public string ProviderGeo = WebConfigurationManager.AppSettings.Get("hereProviderGeo");
        public string ProviderDist = WebConfigurationManager.AppSettings.Get("hereProviderDist");
        private string MapsAppId = WebConfigurationManager.AppSettings.Get("hereMapsAppId");
        private string MapsAppId2 = WebConfigurationManager.AppSettings.Get("hereMapsAppId2");
        private string MapsAppCode = WebConfigurationManager.AppSettings.Get("hereMapsAppCode");
        private string MapsAppCode2 = WebConfigurationManager.AppSettings.Get("hereMapsAppCode2");

        internal string GetGeoUrlProvider(string pDireccion)
        {
            string MapsAppId = GetAppId();
            string MapsAppCode = GetAppCode();
            return ProviderGeo + "app_id="
                        + MapsAppId + "&app_code="
                        + MapsAppCode + "&searchtext=" + pDireccion;
        }

        internal string GetDistUrlProvider(string pLatMov, string pLngMov, string pLatDst, string pLngDst)
        {
            string MapsAppId = GetAppId();
            string MapsAppCode = GetAppCode();

            return ProviderDist + "app_id="
                        + MapsAppId + "&app_code="
                        + MapsAppCode + "&start0="
                        + pLatMov + ","
                        + pLngMov + "&destination0="
                        + pLatDst + ","
                        + pLngDst + "&mode=fastest;car;traffic:disabled&summaryAttributes=tt,di";
        }

        internal string GetDist(string url)
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

                    JToken outer = JToken.Parse(rawresp);
                    JObject inner = outer["response"].ToObject<JObject>();
                    List<JToken> matrixEntry = inner["matrixEntry"].ToList();
                    JToken summary = matrixEntry[0]["summary"];
                    JToken distance = summary["distance"];
                    JToken travelTime = summary["travelTime"];
                    string dist = Math.Round(Convert.ToDecimal(distance) / 1000, 2).ToString() + " km";
                    string time = Math.Round(Convert.ToDecimal(travelTime) / 60, 0).ToString() + " minutes";
                    GetDistByHere = dist + "/" + time;
                }
            }
            catch
            {
                GetDistByHere = "0/0";
            }
            return GetDistByHere;

        }

        internal string GetGeo(string url)
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
                    JToken inner = outerObject["Response"];
                    List<JToken> view = inner["View"].ToList();
                    JToken result = view[0]["Result"];
                    JToken location = result[0]["Location"];
                    JToken navigationPosition = location["NavigationPosition"];
                    JToken latitude = navigationPosition[0]["Latitude"];
                    JToken longitude = navigationPosition[0]["Longitude"];
                    GetGeoByHere = latitude.ToString() + "/" + longitude.ToString();
                }
            }
            catch
            {
                GetGeoByHere = "0/0";
            }
            return GetGeoByHere;
        }

        private string GetAppId()
        {
            try
            {
                switch (new Random().Next(1, 3))
                {
                    case 1:
                        return MapsAppId;
                    case 2:
                        return MapsAppId2;
                }
            }
            catch
            {
            }
            return "";
        }

        private string GetAppCode()
        {
            try
            {
                switch (new Random().Next(1, 3))
                {
                    case 1:
                        return MapsAppCode;
                    case 2:
                        return MapsAppCode2;
                }
            }
            catch
            {
            }
            return "";
        }

        
    }


}