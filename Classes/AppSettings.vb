' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Public Class AppSettings

    ' Tiles
    Private _TileTemperature As Boolean
    Private _TileHumidity As Boolean
    Private _TileLuminance As Boolean
    Private _TileUltraviolet As Boolean
    Private _TileMotion As Boolean
    Private _TileVibration As Boolean
    Private _TileBattery As Boolean
    Private _TileZwaveLight As Boolean
    Private _TileWeather As Boolean
    Private _TileClock As Boolean
    Private _TilePresence As Boolean
    Private _TileDate As Boolean

    Private _AppMeasurementInterval As String
    Private _AppSendMessage As String
    Private _AppSendMessageInterval As String
    Private _AppTemperatureCorrection As String
    Private _AppUseZwave As Integer = 0
    Private _AppVid As UInt32
    Private _AppPid As UInt32
    Private _AppLowTemperatureLevel As Integer = 15
    Private _AppHighTemperatureLevel As Integer = 26
    Private _AppLowHumidityLevel As Integer = 40
    Private _AppHighHumidityLevel As Integer = 70
    Private _AppLowLuxLevel As Integer = 2
    Private _Nodes As AppNodes
    Private _AppLanguage As String

    Private _AppDevice As Device
    Private _AppNetwork As Network

    Public Property TileTemperature
        Get
            Return _TileTemperature
        End Get
        Set(value)
            _TileTemperature = value
        End Set
    End Property

    Public Property TileHumidity
        Get
            Return _TileHumidity
        End Get
        Set(value)
            _TileHumidity = value
        End Set
    End Property

    Public Property TileLuminance
        Get
            Return _TileLuminance
        End Get
        Set(value)
            _TileLuminance = value
        End Set
    End Property

    Public Property TileUltraviolet
        Get
            Return _TileUltraviolet
        End Get
        Set(value)
            _TileUltraviolet = value
        End Set
    End Property

    Public Property TileMotion
        Get
            Return _TileMotion
        End Get
        Set(value)
            _TileMotion = value
        End Set
    End Property

    Public Property TileVibration
        Get
            Return _TileVibration
        End Get
        Set(value)
            _TileVibration = value
        End Set
    End Property

    Public Property TileBattery
        Get
            Return _TileBattery
        End Get
        Set(value)
            _TileBattery = value
        End Set
    End Property

    Public Property TileZwaveLight
        Get
            Return _TileZwaveLight
        End Get
        Set(value)
            _TileZwaveLight = value
        End Set
    End Property

    Public Property TileWeather
        Get
            Return _TileWeather
        End Get
        Set(value)
            _TileWeather = value
        End Set
    End Property

    Public Property TileClock
        Get
            Return _TileClock
        End Get
        Set(value)
            _TileClock = value
        End Set
    End Property

    Public Property TilePresence
        Get
            Return _TilePresence
        End Get
        Set(value)
            _TilePresence = value
        End Set
    End Property

    Public Property TileDate
        Get
            Return _TileDate
        End Get
        Set(value)
            _TileDate = value
        End Set
    End Property

    Public Property AppMeasurementInterval()
        Get
            Return _AppMeasurementInterval
        End Get
        Set(value)
            _AppMeasurementInterval = value
        End Set
    End Property

    Public Property AppSendMessage()
        Get
            Return _AppSendMessage
        End Get
        Set(value)
            _AppSendMessage = value
        End Set
    End Property

    Public Property AppSendMessageInterval()
        Get
            Return _AppSendMessageInterval
        End Get
        Set(value)
            _AppSendMessageInterval = value
        End Set
    End Property

    Public Property AppTemperatureCorrection()
        Get
            Return _AppTemperatureCorrection
        End Get
        Set(value)
            _AppTemperatureCorrection = value
        End Set
    End Property

    Public Property AppUseZwave()
        Get
            Return _AppUseZwave
        End Get
        Set(value)
            _AppUseZwave = value
        End Set
    End Property

    Public Property AppVid()
        Get
            If _AppVid = Nothing Then
                Return &H658 'Default
            Else
                Return _AppVid
            End If
        End Get
        Set(value)
            _AppVid = value
        End Set
    End Property

    Public Property AppPid()
        Get
            If _AppPid = Nothing Then
                Return &H200 'Default
            Else
                Return _AppPid
            End If
            Return _AppPid
        End Get
        Set(value)
            _AppPid = value
        End Set
    End Property

    Public Property AppLowTemperatureLevel
        Get
            Return _AppLowTemperatureLevel
        End Get
        Set(value)
            _AppLowTemperatureLevel = value
        End Set
    End Property

    Public Property AppHighTemperatureLevel
        Get
            Return _AppHighTemperatureLevel
        End Get
        Set(value)
            _AppHighTemperatureLevel = value
        End Set
    End Property

    Public Property AppLowHumidityLevel
        Get
            Return _AppLowHumidityLevel
        End Get
        Set(value)
            _AppLowHumidityLevel = value
        End Set
    End Property

    Public Property AppHighHumidityLevel
        Get
            Return _AppHighHumidityLevel
        End Get
        Set(value)
            _AppHighHumidityLevel = value
        End Set
    End Property

    Public Property AppLowLuxLevel
        Get
            Return _AppLowLuxLevel
        End Get
        Set(value)
            _AppLowLuxLevel = value
        End Set
    End Property

    Public Property Nodes()
        Get
            Return _Nodes
        End Get
        Set(value)
            _Nodes = value
        End Set
    End Property

    Public Property AppLanguage
        Get
            Return _AppLanguage
        End Get
        Set(value)
            _AppLanguage = value
        End Set
    End Property

    Public Property Device
        Get
            Return _AppDevice
        End Get
        Set(value)
            _AppDevice = value
        End Set
    End Property

    Public Property Network
        Get
            Return _AppNetwork
        End Get
        Set(value)
            _AppNetwork = value
        End Set
    End Property

    Public Sub New()

        ' Default settings (Op termijn via de webservice ophalen)
        TileTemperature = True
        TileHumidity = True
        TileLuminance = True
        TileUltraviolet = False
        TileMotion = True
        TileVibration = True
        TileBattery = True
        TileZwaveLight = True
        TileWeather = True
        TileClock = True
        TilePresence = True
        TileDate = True

        AppMeasurementInterval = "2"
        AppSendMessage = "1"
        AppSendMessageInterval = "300"
        AppTemperatureCorrection = "0"
        AppUseZwave = 1
        AppVid = &H658
        AppPid = &H200
        AppLowLuxLevel = 1
        AppLanguage = "nl-NL"

    End Sub
End Class
