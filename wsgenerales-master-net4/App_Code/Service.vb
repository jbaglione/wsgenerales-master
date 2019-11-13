Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Data.Odbc
Imports System.Xml
Imports System.Data.SqlClient
Imports RestSharp
Imports System.Net
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Web.Configuration
Imports System.Data


' Para permitir que se llame a este servicio Web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la siguiente línea.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class Service

    Inherits System.Web.Services.WebService

    Private myConn As SqlConnection
    Private myConn2 As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private myReaderMod As SqlDataReader
    Private myCmdMod As SqlCommand
    Private results As String

    <WebMethod()> _
    Public Function GetDistanciaTiempo(ByVal latMov As String, ByVal lngMov As String, ByVal latDst As String, ByVal lngDst As String) As String

        GetDistanciaTiempo = Nothing

        Dim tiempo As String = ""
        Dim distancia As String = ""
        Dim googleMapsApiKey As String = Me.getGoogleMapsApiKey()

        Dim url As String = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" & latMov & "," & lngMov & "&destinations=" & latDst & "," & lngDst & "&mode=driving&language=fr-FR&key=" & googleMapsApiKey & "&sensor=false"

        Dim status As String = ""

        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText

            Next

            If status = "OK" Then

                m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/duration/text")

                For Each m_node In m_nodelist

                    tiempo = m_node.ChildNodes.Item(0).InnerText

                Next

                m_nodelist = m_xmld.SelectNodes("/DistanceMatrixResponse/row/element/distance/text")

                For Each m_node In m_nodelist

                    distancia = m_node.ChildNodes.Item(0).InnerText

                Next

                GetDistanciaTiempo = distancia & "/" & tiempo

            Else

                GetDistanciaTiempo = "0/0"
            End If

        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try

    End Function

    Private Function getGoogleMapsApiKey() As String

        getGoogleMapsApiKey = ""

        Try

            Dim googleMapsApiKey As String = WebConfigurationManager.AppSettings.Get("googleMapsApiKey")
            Dim googleMapsApiKey2 As String = WebConfigurationManager.AppSettings.Get("googleMapsApiKey2")
            Dim googleMapsApiKey3 As String = WebConfigurationManager.AppSettings.Get("googleMapsApiKey3")

            Randomize()
            Dim value As Integer = CInt(Int((3 * Rnd()) + 1))

            Select Case value
                Case 2 : getGoogleMapsApiKey = googleMapsApiKey2
                Case 3 : getGoogleMapsApiKey = googleMapsApiKey3
                Case Else : getGoogleMapsApiKey = googleMapsApiKey
            End Select

        Catch ex As Exception

        End Try

    End Function

    <WebMethod()>
    Public Function GetLatLong(ByVal direccion As String) As String
        GetLatLong = Nothing
        Dim googleProvider As String = WebConfigurationManager.AppSettings.Get("googleProvider")
        Dim hereProvider As String = WebConfigurationManager.AppSettings.Get("hereProvider")

        Dim url As String = Me.GetUrlProvider(direccion)
        Dim res As Boolean = url.Contains(hereProvider)

        If (url.Contains(googleProvider)) Then
            GetLatLong = GetByGoogle(url)
        ElseIf url.Contains(hereProvider) Then
            GetLatLong = GetByHere(url)
        Else
            GetLatLong = "0/0"
        End If

        Return GetLatLong
    End Function

    <WebMethod()>
    Public Function GetDireccion(ByVal lat As String, ByVal lng As String) As String

        GetDireccion = Nothing
        Dim strResultados As String = ""
        Dim url As String = "http://maps.googleapis.com/maps/api/geocode/xml?address=" & lat & "," & lng & "&sensor=false"
        Dim status As String = ""
        Try
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText
                MsgBox(status)

            Next

            If status = "OK" Then

                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/formatted_address")

                For Each m_node In m_nodelist

                    Dim dire = m_node.ChildNodes.Item(0).InnerText

                    GetDireccion = dire

                Next

            Else
                GetDireccion = "0"

            End If
        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try


    End Function

    Private Function GetByHere(url As String) As String
        Dim request As HttpWebRequest
        Dim response As HttpWebResponse = Nothing
        Dim reader As StreamReader
        Try
            request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            response = DirectCast(request.GetResponse(), HttpWebResponse)
            reader = New StreamReader(response.GetResponseStream())

            Dim rawresp As String = reader.ReadToEnd()

            Dim outer As JToken = JToken.Parse(rawresp)
            Dim inner As JObject = outer("Response")
            Dim view As List(Of JToken) = inner.Item("View").ToList
            Dim result As JToken = view(0).Item("Result")
            Dim location As JToken = result(0).Item("Location")
            Dim navigationPosition As JToken = location.Item("NavigationPosition")
            Dim latitude As JToken = navigationPosition(0)("Latitude")
            Dim longitude As JToken = navigationPosition(0)("Longitude")

            GetByHere = latitude.ToString + "/" + longitude.ToString
        Catch ex As WebException
            GetByHere = "0/0"
        End Try
    End Function

    Private Function GetByGoogle(url As String) As String
        Dim GetLatLong As String = ""

        Try
            Dim strResultados As String = ""
            Dim reader As XmlTextReader = New XmlTextReader(url)
            Dim status As String = ""

            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode

            m_xmld = New XmlDocument()

            m_xmld.Load(url)

            m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/status")

            For Each m_node In m_nodelist

                status = m_node.ChildNodes.Item(0).InnerText

            Next

            If status = "OK" Then

                m_nodelist = m_xmld.SelectNodes("/GeocodeResponse/result/geometry/location")

                For Each m_node In m_nodelist

                    Dim lat = m_node.ChildNodes.Item(0).InnerText

                    Dim lng = m_node.ChildNodes.Item(1).InnerText

                    GetLatLong = lat & "/" & lng

                Next

            Else
                GetLatLong = "0/0"

            End If

        Catch errorVariable As Exception

            Console.Write(errorVariable.ToString())

        End Try

        Return GetLatLong
    End Function

    Private Function GetUrlProvider(ByVal pDireccion As String) As String
        GetUrlProvider = ""
        Dim googleProvider As String = WebConfigurationManager.AppSettings.Get("googleProvider")
        Dim hereProvider As String = WebConfigurationManager.AppSettings.Get("hereProvider")
        Try
            'Distance
            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))

            Select Case value
                Case 1
                    Dim strResultados As String = ""
                    Dim googleMapsApiKey As String = Me.getGoogleMapsApiKey()
                    GetUrlProvider = googleProvider & pDireccion & "&key=" & googleMapsApiKey & "&sensor=false"
                Case Else
                    GetUrlProvider = hereProvider & "app_id=kzobkWkoXdvt3kwYJ2c2&app_code=RZKiLMM1kl3pHhyTI-3AXA&searchtext=" & pDireccion
            End Select

        Catch ex As Exception

        End Try
    End Function

    '<WebMethod()>
    'Public Function GetPuntosEnPoligono(ByVal pLat As Single, ByVal pLon As Single, ByVal pTip As String) As String

    '    GetPuntosEnPoligono = ""
    '    Dim shmSession As New PanelC.Conexion
    '    Dim vNeedClose As Boolean = False

    '    Try

    '        Dim objZonas As New CompuMapC.Zonificaciones

    '        If shmSession.Iniciar("192.168.0.249", 1972, "SHAMAN", "EMERGENCIAS", "JOB", 1, True) Then

    '            vNeedClose = True

    '            Dim vDev As String = objZonas.GetPoligonosInPoint(pLat, pLon, pTip, True)

    '            GetPuntosEnPoligono = vDev

    '        Else

    '            GetPuntosEnPoligono = "Sin conexión"

    '        End If

    '        If vNeedClose Then
    '            shmSession.Cerrar(shmSession.PID, True)
    '        End If

    '    Catch ex As Exception

    '        GetPuntosEnPoligono = ex.Message

    '    End Try

    'End Function

    <WebMethod()> _
    Public Function getIncidente(ByVal cod As String, ByVal fec As String) As String
        Dim result As String = ""
        Dim connectionString As String
        Dim cnn As OdbcConnection
        connectionString = "DSN=phpODBC;UID=_SYSTEM;Pwd=sys"
        cnn = New OdbcConnection(connectionString)
        Try
            cnn.Open()
            Dim Reader As OdbcDataReader
            Dim cmdString = "SELECT TOP 1 inc.HorInicial, inc.HorFinal,inc.sintoma," & _
            "incdom.Domicilio FROM Emergency.Incidentes inc INNER JOIN " & _
            " Emergency.IncidentesDomicilios incdom ON (inc.ID = incdom.IncidenteId)" & _
            " WHERE inc.FecIncidente = '" & fec & "' AND inc.NroIncidente = '" & cod & "'"
            Dim Cmd As New OdbcCommand(cmdString, cnn)
            Reader = Cmd.ExecuteReader()
            If (Reader.Read()) Then
                Dim dom As String = Reader("Domicilio")
                Dim sint As String = Reader("Sintoma")
                Dim horInicio As String = Reader("HorInicial")
                Dim horFinal As String = Reader("HorFinal")
                result = dom & "$" & sint & "$" & horInicio & "$" & horFinal
            End If
            cnn.Close()
        Catch ex As Exception

            result = "Error"
        End Try

        Return result
    End Function
    Public Function formatProd(prod As String) As String

        prod = CType(prod, Integer)
        If prod < 10 Then
            prod = CType(prod, String)
            Return "00" & prod
        ElseIf prod < 100 Then
            prod = CType(prod, String)
            Return "0" & prod
        Else
            prod = CType(prod, String)
            Return prod
        End If
    End Function

    '-------------> PRUEBA DE WEBSERVICE CONTRA SQL SERVER
    <WebMethod()> _
    Public Function getSerialSetLog(ByVal serialNumber As String) As String
        Return getSerialSetLogLast(serialNumber, 0)
    End Function

    <WebMethod()> _
    Public Function getSerialSetLogLast(ByVal serialNumber As String, ByVal pRemote As Integer) As String
        Dim result As String = "0"
        Dim LicenciaId As Integer = 0
        Dim ClienteIp As String = Context.Request.ServerVariables("remote_addr")
        Dim connectionString As String
        Dim SQL As String = ""
        Dim cnnDataSource As String
        Dim cnnCatalog As String
        Dim cnnUser As String
        Dim cnnPassword As String
        Dim conexionServidor As String
        Dim fechaDeVencimiento As DateTime

        serialNumber = serialNumber.Replace("/", "")
        connectionString = ConfigurationManager.ConnectionStrings("cnnShaman").ToString


        Try

            myConn = New SqlConnection(connectionString)
            myConn2 = New SqlConnection(connectionString)
            myCmd = myConn.CreateCommand
            myCmd.CommandText = "SELECT id,serial FROM licencias WHERE Serial = '" & serialNumber & "'"
            myConn.Open()
            myConn2.Open()
            myReader = myCmd.ExecuteReader()

            If myReader.Read() Then

                LicenciaId = myReader("ID")
                myReader.Close()

                myCmd.CommandText = "SELECT cliLic.ID as cliLicId,CnnDataSource,CnnCatalog,CnnUser," & _
                                    "CnnPassword,ISNULL(sit.Url,'') AS ConexionServidor,FechaDeVencimiento FROM ClientesLicencias cliLic LEFT JOIN Sitios sit ON (sit.Id = ConexionServidorId)  WHERE LicenciaID = " & _
                                    LicenciaId

                myReader = myCmd.ExecuteReader()
                If (myReader.Read()) Then

                    Dim CliLicId As Integer = myReader("cliLicId")
                    cnnCatalog = myReader("CnnCatalog")
                    cnnDataSource = myReader("CnnDataSource")
                    cnnUser = myReader("CnnUser")
                    cnnPassword = myReader("CnnPassword")
                    conexionServidor = myReader("ConexionServidor")
                    fechaDeVencimiento = myReader("FechaDeVencimiento")
                    If (Date.Today > fechaDeVencimiento) Then
                        Return 0
                    End If
                    SQL = "SELECT clilicprod.ID as ID, prod.Numero as NROPROD, prod.Id as ProdId FROM ClientesLicenciasProductos clilicprod "
                    SQL = SQL & "INNER JOIN Productos prod ON (prod.id = clilicprod.ProductoID) "
                    SQL = SQL & "WHERE ClientesLicenciaID = " & CliLicId
                    myCmd.CommandText = SQL
                    myReader.Close()
                    myReader = myCmd.ExecuteReader()
                    Dim vMod As New Collection
                    Dim vProd As New Collection

                    While (myReader.Read())

                        Dim prod As String = myReader("NROPROD")
                        Dim prodId As Integer = myReader("ProdId")
                        vProd.Add(prod)
                        Dim CliLicProdId As Integer = myReader("ID")

                        SQL = "SELECT pm.codigo AS MODULOEXC FROM ProductosModulos pm "
                        SQL = SQL & "WHERE pm.ID NOT IN "
                        SQL = SQL & "(SELECT pmod.ID FROM ClientesLicenciasProductosModulos cpmod "
                        SQL = SQL & "INNER JOIN ProductosModulos pmod ON (pmod.ID = cpmod.ProductosModuloID) "
                        SQL = SQL & String.Format("WHERE cpmod.ClientesLicenciasProductoID = {0} ", CliLicProdId)
                        SQL = SQL & String.Format(") AND pm.ProductoId = {0}", prodId)

                        myCmdMod = myConn2.CreateCommand
                        myCmdMod.CommandText = SQL
                        myReaderMod = myCmdMod.ExecuteReader
                        While (myReaderMod.Read())
                            Dim modulo As String = myReaderMod("MODULOEXC")
                            prod = formatProd(prod)
                            Dim prodMod As String = prod & modulo
                            vMod.Add(prodMod)
                        End While
                        myReaderMod.Close()
                    End While

                    Dim prods As String = ""
                    For Each prod As String In vProd
                        If prods = "" Then
                            prods = prod
                        Else
                            prods = prods & "/" & prod
                        End If
                    Next

                    prods = prods & "#"

                    Dim prodModulos As String = ""

                    For Each pmod As String In vMod
                        If prodModulos = "" Then
                            prodModulos = pmod
                        Else
                            prodModulos = prodModulos & "/" & pmod
                        End If
                    Next

                    If (pRemote = 1) Then
                        If (cnnDataSource.Contains("\")) Then
                            Dim vInstance As String() = cnnDataSource.Split("\")
                            Dim instance As String = vInstance(1)
                        End If

                        'cnnDataSource = conexionServidor & "\" & instance
                        cnnDataSource = conexionServidor
                    End If

                    result = cnnDataSource & "^" & cnnCatalog & "^" & cnnUser & "^" & cnnPassword & "^" & prods & prodModulos & "^" & fechaDeVencimiento

                End If

            End If

            myConn.Close()
            myConn.Open()

            Dim time As DateTime = DateTime.Now
            Dim format As String = "yyyy/MM/d HH:mm:ss"
            Dim sTime As String = time.ToString(format)
            SQL = "INSERT INTO LicenciasLogs (LicenciaId, Type, IP, Referencias, CreatedDate, UpdatedDate) "
            SQL = SQL & "VALUES (" & LicenciaId & ",1,'" & ClienteIp & "','" & serialNumber & "','" & sTime & "','" & sTime & "')"
            myCmd.CommandText = SQL
            myCmd.ExecuteNonQuery()
            myConn.Close()

        Catch ex As Exception

            Return ex.Message.ToString

        End Try

        Return result

    End Function

    <WebMethod()> _
    Public Function isInGestion(ByVal user As String, ByVal pass As String, ByVal llave As String) As String
        Dim client = New RestClient()
        client.BaseUrl = "http://localhost:57771/"

        Dim request = New RestRequest()
        request.Resource = "ExternalLogin/IsInGestion"
        request.AddParameter("user", user)
        request.AddParameter("pass", pass)
        request.AddParameter("llave", llave)

        Dim response As IRestResponse = client.Execute(request)

        Return response.Content

    End Function

    <WebMethod()> _
    Public Function setPushNotification(license As String, mobile As String, message As String) As Boolean

        Dim oneSignalUrl As String = WebConfigurationManager.AppSettings.Get("oneSignalUrl")

        Dim request = TryCast(WebRequest.Create(oneSignalUrl), HttpWebRequest)

        request.KeepAlive = True
        request.Method = "POST"
        request.ContentType = "application/json"

        request.Headers.Add("authorization", "Basic ZjljMmY0OTMtMTk4Zi00NWE4LWI2ODItMDllMWNmMjUxNWU5")

        Dim byteArray As Byte() = Encoding.UTF8.GetBytes((Convert.ToString((Convert.ToString((Convert.ToString("{" + """app_id"": ""e090d46b-2aa9-403c-8365-401dfffb77fc""," + """contents"": {""en"": """) & message) + """, ""es"": """) & message) + """}," + """tags"" : [{ ""key"": ""mobile"", ""relation"": ""="", ""value"": """ + mobile + """}," + "{""operator"": ""AND""}," + "{""key"": ""license"", ""relation"": ""="", ""value"": """) & license) + """}" + "]}")

        Dim responseContent As String = Nothing

        Try
            Using writer = request.GetRequestStream()
                writer.Write(byteArray, 0, byteArray.Length)
            End Using

            Using response = TryCast(request.GetResponse(), HttpWebResponse)
                Using reader = New StreamReader(response.GetResponseStream())
                    responseContent = reader.ReadToEnd()
                End Using
            End Using
            Return True
        Catch ex As WebException
            Return False
        End Try

    End Function

    <WebMethod()> _
    Public Function getLicenseInfo(ByVal serialNumber As String) As List(Of LicenseInfo)

        getLicenseInfo = Nothing

        Try

            Dim connectionString As String

            serialNumber = serialNumber.Replace("/", "")
            connectionString = ConfigurationManager.ConnectionStrings("cnnShaman").ToString

            myConn = New SqlConnection(connectionString)

            myConn.Open()

            myCmd = myConn.CreateCommand
            myCmd.CommandText = "sp_GetLicenseInfo"
            myCmd.CommandType = Data.CommandType.StoredProcedure
            myCmd.Parameters.Add("@serial", Data.SqlDbType.VarChar, 10).Value = serialNumber

            Dim dt As New DataTable
            Dim vIdx As Integer
            dt.Load(myCmd.ExecuteReader)

            Dim lLicenseInfo As List(Of LicenseInfo) = New List(Of LicenseInfo)()

            For vIdx = 0 To dt.Rows.Count - 1

                Dim licItm As New LicenseInfo

                licItm.SerialNumber = serialNumber
                licItm.Numero = dt.Rows(vIdx)("Numero")
                licItm.Jerarquia = dt.Rows(vIdx)("Codigo")
                licItm.Estado = dt.Rows(vIdx)("Estado")
                licItm.Vencimiento = dt.Rows(vIdx)("Vencimiento")
                licItm.Observaciones = dt.Rows(vIdx)("Observaciones")
                licItm.VencimientoLicencia = dt.Rows(vIdx)("FechaDeVencimiento")
                licItm.VencimientoSoporte = dt.Rows(vIdx)("FechaVencimientoSoporte")

                lLicenseInfo.Add(licItm)

            Next

            getLicenseInfo = lLicenseInfo

            myConn.Close()

        Catch ex As Exception

        End Try

    End Function


End Class
