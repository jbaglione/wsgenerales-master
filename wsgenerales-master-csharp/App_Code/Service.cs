using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using System.Web.Services;
using RestSharp;
using wsgenerales_master_csharp.App_Code;
using wsgenerales_master_csharp.App_Code.Maps;

namespace wsgenerales_master_csharp
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service : WebService
    {

        private SqlConnection myConn;
        private SqlConnection myConn2;
        private SqlCommand myCmd;
        private SqlDataReader myReader;
        private SqlDataReader myReaderMod;
        private SqlCommand myCmdMod;

        private MapsController mapsController = new MapsController();

        string oneSignalUrl = WebConfigurationManager.AppSettings.Get("oneSignalUrl");
        string gestionUrl = WebConfigurationManager.AppSettings.Get("gestionUrl");


        /// <summary>
        /// Obtener latitud y longitud de una dirección.
        /// </summary>
        /// <param name="direccion">Ej: Antártida Argentina 5872, B1665HWK José C.Paz, Buenos Aires</param>
        /// <returns>            
        /// <string xmlns="http://tempuri.org/">-34.5263803/-58.7664836</string>
        ///</returns>
        [WebMethod()]
        public string GetLatLong(string direccion)
        {
            return mapsController.GetLatLong(direccion);
        }

        /// <summary>
        /// Obtener la distancia y el tiempo desde un punto del mapa a otro.
        /// </summary>
        /// <param name="latMov">Latitud origen</param>
        /// <param name="lngMov">Longitud origen</param>
        /// <param name="latDst">Latitud destino</param>
        /// <param name="lngDst">Longitud destino</param>
        /// <returns>EJ: <string xmlns="http://tempuri.org/">13,28 km/22 minutes</string></returns>
        [WebMethod()]
        public string GetDistanciaTiempo(string latMov, string lngMov, string latDst, string lngDst)
        {
            return mapsController.GetDistanciaTiempo(latMov, lngMov, latDst, lngDst);
        }

        /// <summary>
        /// Obtener la dirección de un punto en el mapa.
        /// </summary>
        /// <param name="lat">Latitud</param>
        /// <param name="lng">Longitud</param>
        /// <returns><string xmlns="http://tempuri.org/">José A.Terry 346, C1406 CHD, Buenos Aires, Argentina</string></returns>
        [WebMethod()]
        public string GetDireccion(string lat, string lng)
        {
            return mapsController.GetDireccion(lat, lng);
        }

        //[WebMethod()]
        public string getIncidente(string cod, string fec)
        {
            string result = "";
            string connectionString;
            OdbcConnection cnn;
            connectionString = "DSN=phpODBC;UID=_SYSTEM;Pwd=sys";
            cnn = new OdbcConnection(connectionString);
            try
            {
                cnn.Open();
                OdbcDataReader Reader;
                string cmdString = "Select TOP 1 inc.HorInicial, inc.HorFinal,inc.sintoma," + "incdom.Domicilio FROM Emergency.Incidentes inc INNER JOIN " + " Emergency.IncidentesDomicilios incdom On (inc.ID = incdom.IncidenteId)"
                                    + " WHERE inc.FecIncidente = \'" + fec + "\' AND inc.NroIncidente = \'" + cod + "\'";

                OdbcCommand Cmd = new OdbcCommand(cmdString, cnn);
                Reader = Cmd.ExecuteReader();
                if (Reader.Read())
                {
                    string dom = Reader["Domicilio"].ToString();
                    string sint = Reader["Sintoma"].ToString();
                    string horInicio = Reader["HorInicial"].ToString();
                    string horFinal = Reader["HorFinal"].ToString();
                    result = dom + "$" + sint + "$" + horInicio + "$" + horFinal;
                }

                cnn.Close();
            }
            catch (Exception ex)
            {
                result = "Error";
            }

            return result;
        }

        private string FormatProd(string prod)
        {
            int prodInt = Convert.ToInt32(prod);
            if (prodInt < 10)
            {
                return "00" + prodInt.ToString();
            }
            else if (prodInt < 100)
            {
                return "0" + prodInt.ToString();
            }
            else
            {
                return prodInt.ToString();
            }
        }

        // <WebMethod()>
        // Public Function GetPuntosEnPoligono(ByVal pLat As Single, ByVal pLon As Single, ByVal pTip As String) As String
        //     GetPuntosEnPoligono = ""
        //     Dim shmSession As New PanelC.Conexion
        //     Dim vNeedClose As Boolean = False
        //     Try
        //         Dim objZonas As New CompuMapC.Zonificaciones
        //         If shmSession.Iniciar("192.168.0.249", 1972, "SHAMAN", "EMERGENCIAS", "JOB", 1, True) Then
        //             vNeedClose = True
        //             Dim vDev As String = objZonas.GetPoligonosInPoint(pLat, pLon, pTip, True)
        //             GetPuntosEnPoligono = vDev
        //         Else
        //             GetPuntosEnPoligono = "Sin conexi�n"
        //         End If
        //         If vNeedClose Then
        //             shmSession.Cerrar(shmSession.PID, True)
        //         End If
        //     Catch ex As Exception
        //         GetPuntosEnPoligono = ex.Message
        //     End Try
        // End Function

        // -------------> PRUEBA DE WEBSERVICE CONTRA SQL SERVER
        [WebMethod()]
        public string getSerialSetLog(string serialNumber)
        {
            return this.getSerialSetLogLast(serialNumber, 0);
        }

        [WebMethod()]
        public string getSerialSetLogLast(string serialNumber, int pRemote)
        {
            string result = "0";
            int LicenciaId = 0;
            string ClienteIp = Context.Request.ServerVariables["remote_addr"];
            string connectionString;
            string SQL = "";
            string cnnDataSource;
            string cnnCatalog;
            string cnnUser;
            string cnnPassword;
            string conexionServidor;
            DateTime fechaDeVencimiento;
            serialNumber = serialNumber.Replace("/", "");
            connectionString = ConfigurationManager.ConnectionStrings["cnnShaman"].ToString();
            try
            {
                myConn = new SqlConnection(connectionString);
                myConn2 = new SqlConnection(connectionString);
                myCmd = myConn.CreateCommand();
                myCmd.CommandText = "SELECT id,serial FROM licencias WHERE Serial = \'" + serialNumber + "\'";
                myConn.Open();
                myConn2.Open();
                myReader = myCmd.ExecuteReader();
                if (myReader.Read())
                {
                    LicenciaId = Convert.ToInt32(myReader["ID"]);
                    myReader.Close();
                    myCmd.CommandText = "SELECT cliLic.ID as cliLicId,CnnDataSource,CnnCatalog,CnnUser," + "CnnPassword,ISNULL(sit.Url,\'\') AS ConexionServidor, FechaDeVencimiento " +
                                        "FROM ClientesLicencias cliLic LEFT JOIN Sitios sit ON (sit.Id = ConexionServidorId) " +
                                        "WHERE LicenciaID = " + LicenciaId;
                    myReader = myCmd.ExecuteReader();
                    if (myReader.Read())
                    {
                        int CliLicId = Convert.ToInt32(myReader["cliLicId"]);
                        cnnCatalog = myReader["CnnCatalog"].ToString();
                        cnnDataSource = myReader["CnnDataSource"].ToString();
                        cnnUser = myReader["CnnUser"].ToString();
                        cnnPassword = myReader["CnnPassword"].ToString();
                        conexionServidor = myReader["ConexionServidor"].ToString();
                        fechaDeVencimiento = Convert.ToDateTime(myReader["FechaDeVencimiento"]);
                        if (DateTime.Now > fechaDeVencimiento)
                            return "0";

                        SQL = "SELECT clilicprod.ID as ID, prod.Numero as NROPROD, prod.Id as ProdId " +
                                "FROM ClientesLicenciasProductos clilicprod ";
                        SQL = SQL + "INNER JOIN Productos prod ON (prod.id = clilicprod.ProductoID) ";
                        SQL = SQL + "WHERE ClientesLicenciaID = " + CliLicId;
                        myCmd.CommandText = SQL;
                        myReader.Close();
                        myReader = myCmd.ExecuteReader();
                        List<string> vMod = new List<string>();
                        List<string> vProd = new List<string>();

                        while (myReader.Read())
                        {
                            string prod = myReader["NROPROD"].ToString();
                            int prodId = Convert.ToInt32(myReader["ProdId"]);
                            vProd.Add(prod);
                            int CliLicProdId = Convert.ToInt32(myReader["ID"]);
                            SQL = "SELECT pm.codigo AS MODULOEXC FROM ProductosModulos pm ";
                            SQL = SQL + "WHERE pm.ID NOT IN ";
                            SQL = SQL + "(SELECT pmod.ID FROM ClientesLicenciasProductosModulos cpmod ";
                            SQL = SQL + "INNER JOIN ProductosModulos pmod ON (pmod.ID = cpmod.ProductosModuloID) ";
                            SQL = SQL + string.Format("WHERE cpmod.ClientesLicenciasProductoID = {0} ", CliLicProdId);
                            SQL = SQL + string.Format(") AND pm.ProductoId = {0}", prodId);
                            myCmdMod = myConn2.CreateCommand();
                            myCmdMod.CommandText = SQL;
                            myReaderMod = myCmdMod.ExecuteReader();
                            while (myReaderMod.Read())
                            {
                                string modulo = myReaderMod["MODULOEXC"].ToString();
                                prod = this.FormatProd(prod);
                                string prodMod = (prod + modulo);
                                vMod.Add(prodMod);
                            }

                            myReaderMod.Close();
                        }

                        string prods = "";
                        foreach (string prod in vProd)
                        {
                            if (prods == "")
                            {
                                prods = prod;
                            }
                            else
                            {
                                prods = prods + "/" + prod;
                            }

                        }

                        prods += "#";
                        string prodModulos = "";
                        foreach (string pmod in vMod)
                        {
                            if (prodModulos == "")
                            {
                                prodModulos = pmod;
                            }
                            else
                            {
                                prodModulos = prodModulos + "/" + pmod;
                            }

                        }

                        if (pRemote == 1)
                        {
                            if (cnnDataSource.Contains('\\'))
                            {
                                string[] vInstance = cnnDataSource.Split('\\');
                                string instance = vInstance[1];
                            }

                            // cnnDataSource = conexionServidor & "\" & instance
                            cnnDataSource = conexionServidor;
                        }

                        result = cnnDataSource + "^"
                                    + cnnCatalog + "^"
                                    + cnnUser + "^"
                                    + cnnPassword + "^"
                                    + prods
                                    + prodModulos + "^" + fechaDeVencimiento.ToString("d/MM/yyyy");
                    }
                }

                myConn.Close();
                myConn.Open();
                DateTime time = DateTime.Now;
                string format = "yyyy/MM/d HH:mm:ss";
                string sTime = time.ToString(format);
                SQL = "INSERT INTO LicenciasLogs (LicenciaId, Type, IP, Referencias, CreatedDate, UpdatedDate) ";
                SQL = SQL + "VALUES ("
                            + LicenciaId + ",1,\'"
                            + ClienteIp + "\',\'"
                            + serialNumber + "\',\'"
                            + sTime + "\',\'"
                            + sTime + "\')";
                myCmd.CommandText = SQL;
                myCmd.ExecuteNonQuery();
                myConn.Close();
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

            return result;

        }

        [WebMethod()]
        public string isInGestion(string user, string pass, string llave)
        {
            RestClient client = new RestClient(this.gestionUrl);
            RestRequest request = new RestRequest();
            request.Resource = "ExternalLogin/IsInGestion";
            request.AddParameter("user", user);
            request.AddParameter("pass", pass);
            request.AddParameter("llave", llave);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        [WebMethod()]
        public bool setPushNotification(string license, string mobile, string message)
        {
            var request = WebRequest.Create(oneSignalUrl) as HttpWebRequest;
            if (request != null)
            {
                request.KeepAlive = true;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("authorization", "Basic ZjljMmY0OTMtMTk4Zi00NWE4LWI2ODItMDllMWNmMjUxNWU5");
                byte[] byteArray = Encoding.UTF8.GetBytes(Convert.ToString(Convert.ToString(Convert.ToString("{" + "\"app_id\": \"e090d46b-2aa9-403c-8365-401dfffb77fc\"," + "\"contents\": {\"en\": \"") + message
                                        + "\", \"es\": \"") + message + "\"}," + "\"tags\" : [{ \"key\": \"mobile\", \"relation\": \"=\", \"value\": \""
                                    + mobile + "\"}," + "{\"operator\": \"AND\"}," + "{\"key\": \"license\", \"relation\": \"=\", \"value\": \"") + license + ("\"}" + "]}"));
                try
                {
                    request.ContentLength = byteArray.Length;

                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();

                    WebResponse response = request.GetResponse();
                    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    using (dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        //Console.WriteLine(responseFromServer);
                    }
                    response.Close();

                    return true;
                }
                catch (WebException ex)
                {
                    return false;
                }
            }
            return false;
        }

        [WebMethod()]
        public List<LicenseInfo> getLicenseInfo(string serialNumber)
        {
            List<LicenseInfo> getLicenseInfo = null;
            try
            {
                string connectionString;
                serialNumber = serialNumber.Replace("/", "");
                connectionString = ConfigurationManager.ConnectionStrings["cnnShaman"].ToString();
                myConn = new SqlConnection(connectionString);
                myConn.Open();
                myCmd = myConn.CreateCommand();
                myCmd.CommandText = "sp_GetLicenseInfo";
                myCmd.CommandType = CommandType.StoredProcedure;
                myCmd.Parameters.Add("@serial", SqlDbType.VarChar, 10).Value = serialNumber;
                DataTable dt = new DataTable();
                int vIdx;
                dt.Load(myCmd.ExecuteReader());
                List<LicenseInfo> lLicenseInfo = new List<LicenseInfo>();
                for (vIdx = 0; (vIdx
                            <= (dt.Rows.Count - 1)); vIdx++)
                {
                    LicenseInfo licItm = new LicenseInfo();
                    licItm.SerialNumber = serialNumber;
                    licItm.Numero = Convert.ToInt32(dt.Rows[vIdx]["Numero"]);
                    licItm.Jerarquia = dt.Rows[vIdx]["Codigo"].ToString();
                    licItm.Estado = Convert.ToInt32(dt.Rows[vIdx]["Estado"]);
                    licItm.Vencimiento = Convert.ToDateTime(dt.Rows[vIdx]["Vencimiento"]);
                    licItm.Observaciones = dt.Rows[vIdx]["Observaciones"].ToString();
                    licItm.VencimientoLicencia = Convert.ToDateTime(dt.Rows[vIdx]["FechaDeVencimiento"]);
                    licItm.VencimientoSoporte = Convert.ToDateTime(dt.Rows[vIdx]["FechaVencimientoSoporte"]);
                    lLicenseInfo.Add(licItm);
                }

                getLicenseInfo = lLicenseInfo;
                myConn.Close();
            }
            catch (Exception ex)
            {
            }
            return getLicenseInfo;
        }

    }
}
