Imports Microsoft.VisualBasic

Public Class LicenseInfo
    Private clSerialNumber As String
    Private clNumero As Integer
    Private clJerarquia As String
    Private clEstado As Integer
    Private clVencimiento As Date
    Private clObservaciones As String
    Private clVencimientoLicencia As Date
    Private clVencimientoSoporte As Date
    Public Property SerialNumber() As String
        Get
            Return clSerialNumber
        End Get
        Set(ByVal value As String)
            clSerialNumber = value
        End Set
    End Property
    Public Property Numero() As Integer
        Get
            Return clNumero
        End Get
        Set(ByVal value As Integer)
            clNumero = value
        End Set
    End Property
    Public Property Jerarquia() As String
        Get
            Return clJerarquia
        End Get
        Set(ByVal value As String)
            clJerarquia = value
        End Set
    End Property
    Public Property Estado() As Integer
        Get
            Return clEstado
        End Get
        Set(ByVal value As Integer)
            clEstado = value
        End Set
    End Property
    Public Property Vencimiento() As Date
        Get
            Return clVencimiento
        End Get
        Set(ByVal value As Date)
            clVencimiento = value
        End Set
    End Property
    Public Property Observaciones() As String
        Get
            Return clObservaciones
        End Get
        Set(ByVal value As String)
            clObservaciones = value
        End Set
    End Property
    Public Property VencimientoLicencia() As Date
        Get
            Return clVencimientoLicencia
        End Get
        Set(ByVal value As Date)
            clVencimientoLicencia = value
        End Set
    End Property
    Public Property VencimientoSoporte() As Date
        Get
            Return clVencimientoSoporte
        End Get
        Set(ByVal value As Date)
            clVencimientoSoporte = value
        End Set
    End Property

End Class
