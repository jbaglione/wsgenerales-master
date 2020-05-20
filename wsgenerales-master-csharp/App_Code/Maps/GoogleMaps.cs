using System;
using System.Web.Configuration;
using System.Xml;


namespace wsgenerales_master_csharp.App_Code.Maps
{
    public class GoogleMaps
    {
        //Google
        public string ProviderDist = WebConfigurationManager.AppSettings.Get("googleProviderDist");
        public string ProviderGeo = WebConfigurationManager.AppSettings.Get("googleProviderGeo");
        private string MapsApiKey = WebConfigurationManager.AppSettings.Get("googleMapsApiKey");
        private string MapsApiKey2 = WebConfigurationManager.AppSettings.Get("googleMapsApiKey2");

        internal string GetGeoUrlProvider(string pDireccion)
        {
            string googleMapsApiKey = this.GetGoogleMapsApiKey();
            return ProviderGeo + pDireccion + "&key="
                                + googleMapsApiKey + "&sensor=false";
        }

        private string GetGoogleMapsApiKey()
        {
            try
            {
                switch (new Random().Next(1, 3))
                {
                    case 1:
                        return MapsApiKey;
                    case 2:
                        return MapsApiKey2;
                }
            }
            catch
            {
            }
            return "";
        }

        internal string GetGeoByGoogle(string url)
        {
            string GetLatLong = "";
            try
            {
                XmlTextReader reader = new XmlTextReader(url);
                string status = "";
                XmlDocument m_xmld;
                XmlNodeList m_nodelist;
                m_xmld = new XmlDocument();
                m_xmld.Load(url);
                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status");
                foreach (XmlNode m_node in m_nodelist)
                {
                    status = m_node.ChildNodes[0].InnerText;
                }

                if (status == "OK")
                {
                    m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/geometry/location");
                    foreach (XmlNode m_node in m_nodelist)
                    {
                        object lat = m_node.ChildNodes[0].InnerText;
                        object lng = m_node.ChildNodes[1].InnerText;
                        GetLatLong = lat + "/" + lng;
                    }
                }
                else
                {
                    GetLatLong = "0/0";
                }
            }
            catch (Exception errorVariable)
            {
                Console.Write(errorVariable.ToString());
            }

            return GetLatLong;
        }

        internal string GetDistByGoogle(string url)
        {
            string GetDistByGoogle = null;
            string status = "";
            string tiempo = "";
            string distancia = "";
            try
            {
                XmlDocument m_xmld;
                XmlNodeList m_nodelist;
                m_xmld = new XmlDocument();
                m_xmld.Load(url);
                m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/status");
                foreach (XmlNode m_node in m_nodelist)
                {
                    status = m_node.ChildNodes.Item(0).InnerText;
                }

                if (status == "OK")
                {
                    m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/duration/text");
                    foreach (XmlNode m_node in m_nodelist)
                    {
                        tiempo = m_node.ChildNodes.Item(0).InnerText;
                    }

                    m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/distance/text");
                    foreach (XmlNode m_node in m_nodelist)
                    {
                        distancia = m_node.ChildNodes.Item(0).InnerText;
                    }

                    GetDistByGoogle = distancia + "/" + tiempo;
                }
                else
                {
                    GetDistByGoogle = "0/0";
                }

            }
            catch (Exception errorVariable)
            {
                Console.Write(errorVariable.ToString());
            }
            return GetDistByGoogle;
        }

        internal string GetDistUrlProvider(string pLatMov, string pLngMov, string pLatDst, string pLngDst)
        {
            string googleMapsApiKey = this.GetGoogleMapsApiKey();
            return ProviderDist
                        + pLatMov + ","
                        + pLngMov + "&destinations="
                        + pLatDst + ","
                        + pLngDst + "&mode=driving&language=fr-FR&key="
                        + googleMapsApiKey + "&sensor=false";
        }

        internal string GetDireccion(string lat, string lng)
        {
            string GetDireccion = null;
            //string strResultados = "";
            string googleMapsApiKey = this.GetGoogleMapsApiKey();
            string url = ProviderGeo + lat + "," + lng + "&sensor=false" + "&key=" + googleMapsApiKey;

            string status = "";
            try
            {
                XmlDocument m_xmld;
                XmlNodeList m_nodelist;

                m_xmld = new XmlDocument();
                m_xmld.Load(url);
                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status");
                foreach (XmlNode m_node in m_nodelist)
                {
                    status = m_node.ChildNodes[0].InnerText;
                    // MsgBox(status)
                }

                if (status == "OK")
                {
                    m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/formatted_address");
                    foreach (XmlNode m_node in m_nodelist)
                    {
                        string dire = m_node.ChildNodes[0].InnerText;
                        return dire;
                    }
                }
                else
                {
                    GetDireccion = "0";
                }

            }
            catch (Exception errorVariable)
            {
                Console.Write(errorVariable.ToString());
            }

            return GetDireccion;
        }
    }
}