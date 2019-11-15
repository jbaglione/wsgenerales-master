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
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class Service

    Inherits System.Web.Services.WebService

    Private myConn As SqlConnection
    Private myConn2 As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private myReaderMod As SqlDataReader
    Private myCmdMod As SqlCommand
    Private results As String

    Private googleProviderDist As String = WebConfigurationManager.AppSettings.Get("googleProviderDist")
    Private hereProviderDist As String = WebConfigurationManager.AppSettings.Get("hereProviderDist")
    Private googleProviderGeo As String = WebConfigurationManager.AppSettings.Get("googleProviderGeo")
    Private hereProviderGeo As String = WebConfigurationManager.AppSettings.Get("hereProviderGeo")

    <WebMethod()>
    Public Function GetDistanciaTiempo(ByVal latMov As String, ByVal lngMov As String, ByVal latDst As String, ByVal lngDst As String) As String

        GetDistanciaTiempo = Nothing

        Dim url As String = Me.GetDistUrlProvider(latMov, lngMov, latDst, lngDst)

        If (url.Contains(googleProviderDist)) Then
            GetDistanciaTiempo = GetDistByGoogle(url)
        ElseIf url.Contains(hereProviderDist) Then
            GetDistanciaTiempo = GetDistByHere(url)
        Else
            GetDistanciaTiempo = "0/0"
        End If

    End Function

    Private Function GetDistUrlProvider(ByVal pLatMov As String, ByVal pLngMov As String, ByVal pLatDst As String, ByVal pLngDst As String) As String
        GetDistUrlProvider = ""
        Try
            'Distance
            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))
            Select Case value
                Case 1
                    Dim strResultados As String = ""
                    Dim googleMapsApiKey As String = Me.getGoogleMapsApiKey()
                    GetDistUrlProvider = googleProviderDist & pLatMov & "," & pLngMov & "&destinations=" & pLatDst & "," & pLngDst & "&mode=driving&language=fr-FR&key=" & googleMapsApiKey & "&sensor=false"
                Case Else
                    Dim hereMapsAppId As String = Me.getHereMapsAppId()
                    Dim hereMapsAppCode As String = Me.getHereMapsAppCode()
                    'GetDistUrlProvider = hereProviderDist & "app_id=" & hereMapsAppId & "&app_code=" & hereMapsAppCode & "&waypoint0=geo!" & pLatMov & "," & pLngMov & "&waypoint1=geo!" & pLatDst & "," & pLngDst & "&mode=fastest;car;traffic:disabled"
                    GetDistUrlProvider = hereProviderDist & "app_id=" & hereMapsAppId & "&app_code=" & hereMapsAppCode & "&start0=" & pLatMov & "," & pLngMov & "&destination0=" & pLatDst & "," & pLngDst & "&mode=fastest;car;traffic:disabled&summaryAttributes=tt,di"
            End Select

        Catch ex As Exception

        End Try
    End Function

    Private Function GetDistByGoogle(url As String) As String
        GetDistByGoogle = Nothing
        Dim status As String = ""
        Dim tiempo As String = ""
        Dim distancia As String = ""

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

                GetDistByGoogle = distancia & "/" & tiempo

            Else
                GetDistByGoogle = "0/0"
            End If

        Catch errorVariable As Exception
            Console.Write(errorVariable.ToString())
        End Try
    End Function

    Private Function GetDistByHere(url As String) As String
        Dim request As HttpWebRequest
        Dim response As HttpWebResponse = Nothing
        Dim reader As StreamReader
        Try
            request = DirectCast(WebRequest.Create(url), HttpWebRequest)
            response = DirectCast(request.GetResponse(), HttpWebResponse)
            reader = New StreamReader(response.GetResponseStream())

            Dim rawresp As String = reader.ReadToEnd()

            Dim outer As JToken = JToken.Parse(rawresp)
            Dim inner As JObject = outer("response")
            Dim matrixEntry As List(Of JToken) = inner.Item("matrixEntry").ToList
            Dim summary As JToken = matrixEntry(0).Item("summary")
            Dim distance As JToken = summary.Item("distance")
            Dim travelTime As JToken = summary.Item("travelTime")
            Dim dist As String = Math.Round((Convert.ToInt64(distance) / 1000), 2).ToString + " km"
            Dim time As String = Math.Round((Convert.ToInt64(travelTime) / 60), 0).ToString + " minutes"
            GetDistByHere = dist + "/" + time
        Catch ex As WebException
            GetDistByHere = "0/0"
        End Try
    End Function


    <WebMethod()>
    Public Function GetLatLong(ByVal direccion As String) As String
        GetLatLong = Nothing

        Dim url As String = Me.GetGeoUrlProvider(direccion)

        If (url.Contains(googleProviderGeo)) Then
            GetLatLong = GetGeoByGoogle(url)
        ElseIf url.Contains(hereProviderGeo) Then
            GetLatLong = GetGeoByHere(url)
        Else
            GetLatLong = "0/0"
        End If

        Return GetLatLong
    End Function

    Private Function GetGeoUrlProvider(ByVal pDireccion As String) As String
        GetGeoUrlProvider = ""

        Try
            'Distance
            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))

            Select Case value
                Case 1
                    Dim strResultados As String = ""
                    Dim googleMapsApiKey As String = Me.getGoogleMapsApiKey()
                    GetGeoUrlProvider = googleProviderGeo & pDireccion & "&key=" & googleMapsApiKey & "&sensor=false"
                Case Else
                    Dim hereMapsAppId As String = Me.getHereMapsAppId()
                    Dim hereMapsAppCode As String = Me.getHereMapsAppCode()
                    GetGeoUrlProvider = hereProviderGeo & "app_id=" & hereMapsAppId & "&app_code=" & hereMapsAppCode & "&searchtext=" & pDireccion
            End Select

        Catch ex As Exception

        End Try
    End Function

    Private Function GetGeoByHere(url As String) As String
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

            GetGeoByHere = latitude.ToString + "/" + longitude.ToString
        Catch ex As WebException
            GetGeoByHere = "0/0"
        End Try
    End Function

    Private Function GetGeoByGoogle(url As String) As String
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


#Region "Get_Keys"
    Private Function getGoogleMapsApiKey() As String

        getGoogleMapsApiKey = ""

        Try

            Dim googleMapsApiKey As String = WebConfigurationManager.AppSettings.Get("googleMapsApiKey")
            Dim googleMapsApiKey2 As String = WebConfigurationManager.AppSettings.Get("googleMapsApiKey2")

            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))

            Select Case value
                Case 1 : getGoogleMapsApiKey = googleMapsApiKey
                Case 2 : getGoogleMapsApiKey = googleMapsApiKey2
            End Select

        Catch ex As Exception

        End Try

    End Function

    Private Function getHereMapsAppId() As String

        getHereMapsAppId = ""

        Try

            Dim hereMapsAppId As String = WebConfigurationManager.AppSettings.Get("hereMapsAppId")
            Dim hereMapsAppId2 As String = WebConfigurationManager.AppSettings.Get("hereMapsAppId2")

            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))

            Select Case value
                Case 1 : getHereMapsAppId = hereMapsAppId
                Case 2 : getHereMapsAppId = hereMapsAppId2
            End Select

        Catch ex As Exception

        End Try

    End Function

    Private Function getHereMapsAppCode() As String

        getHereMapsAppCode = ""

        Try

            Dim hereMapsAppCode As String = WebConfigurationManager.AppSettings.Get("hereMapsAppCode")
            Dim hereMapsAppCode2 As String = WebConfigurationManager.AppSettings.Get("hereMapsAppCode2")

            Randomize()
            Dim value As Integer = CInt(Int((2 * Rnd()) + 1))

            Select Case value
                Case 1 : getHereMapsAppCode = hereMapsAppCode
                Case 2 : getHereMapsAppCode = hereMapsAppCode2
            End Select

        Catch ex As Exception

        End Try

    End Function
#End Region


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

    <WebMethod()>
    Public Function getIncidente(ByVal cod As String, ByVal fec As String) As String
        Dim result As String = ""
        Dim connectionString As String
        Dim cnn As OdbcConnection
        connectionString = "DSN=phpODBC;UID=_SYSTEM;Pwd=sys"
        cnn = New OdbcConnection(connectionString)
        Try
            cnn.Open()
            Dim Reader As OdbcDataReader
            Dim cmdString = "Select TOP 1 inc.HorInicial, inc.HorFinal,inc.sintoma," &
            "incdom.Domicilio FROM Emergency.Incidentes inc INNER JOIN " &
            " Emergency.IncidentesDomicilios incdom On (inc.ID = incdom.IncidenteId)" &
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
    <WebMethod()>
    Public Function getSerialSetLog(ByVal serialNumber As String) As String
        Return getSerialSetLogLast(serialNumber, 0)
    End Function

    <WebMethod()>
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

                myCmd.CommandText = "SELECT cliLic.ID as cliLicId,CnnDataSource,CnnCatalog,CnnUser," &
                                    "CnnPassword,ISNULL(sit.Url,'') AS ConexionServidor,FechaDeVencimiento FROM ClientesLicencias cliLic LEFT JOIN Sitios sit ON (sit.Id = ConexionServidorId)  WHERE LicenciaID = " &
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

    <WebMethod()>
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

    <WebMethod()>
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

    <WebMethod()>
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
