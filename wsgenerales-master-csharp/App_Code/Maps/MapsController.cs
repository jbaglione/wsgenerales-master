using System;


namespace wsgenerales_master_csharp.App_Code.Maps
{
    public class MapsController
    {
        private GoogleMaps google = new GoogleMaps();
        private HereMaps here = new HereMaps();
        private HereMapsNew hereNew = new HereMapsNew();

        internal string GetLatLong(string direccion)
        {
            string GetLatLong = null;
            string url = this.GetGeoUrlProvider(direccion);
            if (url.Contains(google.ProviderGeo))
            {
                GetLatLong = google.GetGeoByGoogle(url);
            }
            else if (url.Contains(here.ProviderGeo))
            {
                GetLatLong = here.GetGeo(url);
            }
            else if (url.Contains(hereNew.ProviderGeo))
            {
                GetLatLong = hereNew.GetGeo(url);
            }
            else
            {
                GetLatLong = "0/0";
            }

            return GetLatLong;
        }
        private string GetGeoUrlProvider(string pDireccion)
        {
            try
            {
                switch (new Random().Next(1, 4))
                {
                    case 1:
                        return google.GetGeoUrlProvider(pDireccion);

                    case 2:
                        return here.GetGeoUrlProvider(pDireccion);

                    case 3:
                        return hereNew.GetGeoUrlProvider(pDireccion);
                }
            }
            catch
            {
            }
            return "";
        }

        internal string GetDistanciaTiempo(string latMov, string lngMov, string latDst, string lngDst)
        {
            string GetDistanciaTiempo = null;
            string url = this.GetDistUrlProvider(latMov, lngMov, latDst, lngDst);
            if (url.Contains(google.ProviderDist))
            {
                GetDistanciaTiempo = google.GetDistByGoogle(url);
            }
            else if (url.Contains(here.ProviderDist))
            {
                GetDistanciaTiempo = here.GetDist(url);
            }
            else if (url.Contains(hereNew.ProviderDist))
            {
                GetDistanciaTiempo = hereNew.GetDist(url);
            }
            else
            {
                GetDistanciaTiempo = "0/0";
            }

            return GetDistanciaTiempo;
        }

        internal string GetDireccion(string lat, string lng)
        {
            return google.GetDireccion(lat, lng);
        }

        private string GetDistUrlProvider(string pLatMov, string pLngMov, string pLatDst, string pLngDst)
        {
            try
            {
                // Distance
                switch (new Random().Next(1, 4))
                {
                    case 1:
                        return google.GetDistUrlProvider(pLatMov, pLngMov, pLatDst, pLngDst);
                    case 2:
                        return here.GetDistUrlProvider(pLatMov, pLngMov, pLatDst, pLngDst);
                    case 3:
                        return hereNew.GetDistUrlProvider(pLatMov, pLngMov, pLatDst, pLngDst);
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        }
    }
}