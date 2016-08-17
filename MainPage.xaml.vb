' ---------------------------------------------------------------------------------
' Copyright 2016 – Sequitur, A.A. Hondema. All rights reserved.
'
' Documentation:
' FezHat:       https://www.nuget.org/packages/GHIElectronics.UWP.Shields.FEZHAT/
'               https://bitbucket.org/ghi_elect/windows-iot/src
'               https://github.com/Microsoft-Build-2016/CodeLabs-IoTDev/blob/master/Module4-OpenHack/FEZHAT.md
'               https://www.ghielectronics.com/catalog/product/500
' Z-Wave:       https://www.nuget.org/packages/ZWave4Net
'               https://github.com/roblans/ZWave4Net
' Aeotec:       http://aeotec.com/support
' Greenwave:    http://www.greenwavesystems.com/#whatwedo
' Gauge:        https://www.nuget.org/packages/winrtxamltoolkit.controls.gauge (no longer used, replaced by tiles)
'
' References:   GHIElectronics.UWP.Shields.FEZHAT
'               Windows IoT extentions For the UWP (Windows.devices.GPIO)
'               Microsoft.NETCore.UniversalWindowsPlatform
'               Microsoft.NetCore.Runtime
'               Universal Windows
'               WinRTXamlToolkit.Controls.DataVisualization.Windows
'               ZWave4Net
'
' Weather:      api.openweathermap.org, apikey=616a73e2e99317a79a9297a6c9a9e0ff
'               http://api.openweathermap.org/data/2.5/weather?q=Leeuwarden,nl&units=metric&mode=xml&lang=nl&appid=616a73e2e99317a79a9297a6c9a9e0ff
'               <current>
'                   <city id = "2751792" name="Leeuwarden">
'                       <coord lon = "5.81" lat="53.2"/>
'                       <country> NL</country>
'                       <sun rise = "2016-06-13T03:09:15" Set="2016-06-13T20:05:04"/>
'                   </city>
'                   <temperature value = "20.28" min="18.89" max="22.22" unit="metric"/>
'                   <humidity value = "86" unit="%"/>
'                   <pressure value = "998" unit="hPa"/>
'                   <wind>
'                       <speed value = "1.07" name="Calm"/>
'                       <gusts/>
'                       <direction value = "182.003" code="S" name="South"/>
'                   </wind>
'                   <clouds value = "92" name="overcast clouds"/>
'                   <Visibility/>
'                   <precipitation value = "1.02" mode="rain" unit="1h"/>
'                   <weather number = "501" value="matige regen" icon="10d"/>
'                   <lastupdate value = "2016-06-13T12:15:19" />
'               </current>
' ----------------------------------------------------------------------------------------

Imports System.Globalization
Imports System.Net.Http
Imports System.Threading
Imports System.Xml
Imports Windows.ApplicationModel.Core
Imports Windows.ApplicationModel.Resources
Imports Windows.Globalization
Imports Windows.Storage
Imports Windows.System
Imports Windows.UI
Imports Windows.UI.Core
Imports Windows.UI.Xaml.Shapes
Imports WinRTXamlToolkit.Controls.DataVisualization.Charting
Imports ZWave
Imports ZWave.CommandClasses
Imports ZWave.Devices
Imports ZWave.Devices.Aeon

Public NotInheritable Class MainPage

#Region "Declarations"
    Inherits Page

    Dim UISyncContext = TaskScheduler.FromCurrentSynchronizationContext()

    ' Timers
    Dim ClockTimer As DispatcherTimer
    Dim NodeListTimer As DispatcherTimer
    Dim TwoMinuteTimer As DispatcherTimer
    Dim LightOnTimer As New DispatcherTimer
    Dim LightOffTimer As New DispatcherTimer

    Dim FirstRun As Boolean = True

    ' Settings
    Dim AppNodes As New AppNodes
    Dim AppSettings As AppSettings

    ' Z-Wave
    Dim Controller As ZWave.ZWaveController
    Dim ZwaveNode As ZWave.Node
    Dim ZwaveNodes As ZWave.NodeCollection

    ' Wallplug
    Dim SwitchBinary As SwitchBinary

    ' Multisensor6
    Dim MultiSensorNode As ZWave.Node
    Dim WithEvents MultisensorBatterylevel As New Measurement

    ' Measurements
    Dim WithEvents Temperature As New Measurement
    Dim WithEvents Humidity As New Measurement
    Dim WithEvents Luminance As New Measurement
    Dim WithEvents Ultraviolet As New Measurement
    Dim WithEvents Motion As New Measurement
    Dim WithEvents Vibration As New Measurement
    Dim WithEvents OutdoorTemperature As New Measurement
    Dim WithEvents OutdoorHumidity As New Measurement
    Dim WithEvents OutdoorPressure As New Measurement
    Dim WithEvents Presence As New Measurement

    ' Log
    Dim Log As Boolean = False
    Dim LogFolder As StorageFolder
    Dim LogFile As StorageFile

    ' Tiles
    Public Enum Colorscheme As Integer
        Day = 1
        Night = 2
    End Enum

    Dim ActualColorSchema As Integer = Colorscheme.Night

    Dim TileTemperatureTitle As New TextBlock
    Dim TileTemperatureValue As New TextBlock
    Dim PnlTemperature As New Grid
    Dim gvTemperature As New GridViewItem
    Dim ImgTemperature As New Image
    Dim TemperatureBinding As Binding

    Dim TileHumidityTitle As New TextBlock
    Dim TileHumidityValue As New TextBlock
    Dim PnlHumidity As New Grid
    Dim gvHumidity As New GridViewItem
    Dim ImgHumidity As New Image

    Dim TileLuminanceTitle As New TextBlock
    Dim TileLuminanceValue As New TextBlock
    Dim PnlLuminance As New Grid
    Dim gvLuminance As New GridViewItem
    Dim ImgLuminance As New Image

    Dim TileUVTitle As New TextBlock
    Dim TileUVValue As New TextBlock
    Dim PnlUltraviolet As New Grid
    Dim gvUltraviolet As New GridViewItem
    Dim ImgUltraviolet As New Image

    Dim TileMotionTitle As New TextBlock
    Dim TileMotionValue As New TextBlock
    Dim TileMotionDate As New TextBlock
    Dim PnlMotion As New Grid
    Dim gvMotion As New GridViewItem
    Dim ImgMotion As New Image

    Dim TileVibrationTitle As New TextBlock
    Dim TileVibrationValue As New TextBlock
    Dim TileVibrationDate As New TextBlock
    Dim PnlVibration As New Grid
    Dim gvVibration As New GridViewItem
    Dim ImgVibration As New Image

    Dim TileBatteryTitle As New TextBlock
    Dim TileBatteryValue As New TextBlock
    Dim PnlBattery As New Grid
    Dim gvBattery As New GridViewItem
    Dim ImgBattery As New Image

    Dim TileZwaveLightNodeIDs As New List(Of Integer)
    Dim PnlZwaveLight() As Grid = {}
    Dim TileZwaveLightTitle() As TextBlock = {}
    Dim gvZwaveLight() As GridViewItem = {}
    Dim ImgZwaveLight() As Image = {}
    Dim Powerconsumption() As TextBlock = {}
    Dim MinutesLeft() As TextBlock = {}
    Dim LightOnStartTime As New DateTime

    Dim TileWeatherTitle As New TextBlock
    Dim PnlWeather As New Grid
    Dim gvWeather As New GridViewItem
    Dim ImgWeather As New Image
    Dim WeatherText As New TextBlock

    Dim TileTemperatureOutsideTitle As New TextBlock
    Dim TileTemperatureOutsideValue As New TextBlock
    Dim PnlTemperatureOutside As New Grid
    Dim gvTemperatureOutside As New GridViewItem
    Dim ImgTemperatureOutside As New Image

    Dim TileHumidityOutsideTitle As New TextBlock
    Dim TileHumidityOutsideValue As New TextBlock
    Dim PnlHumidityOutside As New Grid
    Dim gvHumidityOutside As New GridViewItem
    Dim ImgHumidityOutside As New Image

    Dim TilePressureTitle As New TextBlock
    Dim TilePressureValue As New TextBlock
    Dim PnlPressure As New Grid
    Dim gvPressure As New GridViewItem
    Dim ImgPressure As New Image

    Dim TileClockTitle As New TextBlock
    Dim TodaysDate As New TextBlock
    Dim PnlClock As New StackPanel
    Dim gvClock As New GridViewItem

    Dim SecondHand As New RotateTransform()
    Dim MinuteHand As New RotateTransform()
    Dim HourHand As New RotateTransform()
    Dim Ellipse As New Ellipse
    Dim Seconds As New Rectangle
    Dim Minutes As New Rectangle
    Dim Hours As New Rectangle

    Dim TileDayNightTitle As New TextBlock
    Dim PnlDayNight As New Grid
    Dim gvDayNight As New GridViewItem
    Dim ImgDayNight As New Image

    Dim TilePresenceTitle As New TextBlock
    Dim ImgPresence As New Image
    Dim PnlPresence As New Grid
    Dim gvPresence As New GridViewItem

    Dim TileDateTitle As New TextBlock
    Dim TileDateValue As New TextBlock
    Dim TileDateWeekday As New TextBlock
    Dim PnlDate As New Grid
    Dim gvDate As New GridViewItem

#End Region

#Region "Ïnit"
    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)

        ' =============
        ' Program start
        ' =============
        Try
            Me.InitializeComponent()
            MyBase.OnNavigatedTo(e)

            ' If an AppSettings object is passed: fill the AppSettings object
            If Not e.Parameter.ToString = "" Then
                AppSettings = e.Parameter
            Else
                AppSettings = New AppSettings()
            End If

            ' Handler to close down the program
            AddHandler Application.Current.Suspending, AddressOf Suspending

            ' Retrieve device and netword information
            GetDeviceInformation()
            GetNetworkInformation()
            LogSysteminfo()

            ' Initialization
            InitApp(e)
            InitTemperature()
            InitHumidity()
            StartTimers()

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine OnNavigatedTo:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub InitApp(e As NavigationEventArgs)

        Try
            ' Set language
            ApplicationLanguages.PrimaryLanguageOverride = AppSettings.AppLanguage

            ' Set LightOnStartTime to low value
            LightOnStartTime = Date.MinValue

            ' Set presence to 1 (True)
            Presence.Waarde = 1

            ' Create a LightOnTimer
            LightOnTimer = New DispatcherTimer
            LightOnTimer.Stop()

            ' Create a LightOffTimer
            LightOffTimer = New DispatcherTimer
            LightOffTimer.Stop()

            LogAppSettings()

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine InitApp:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub InitTiles()

        Try
            GridTiles.Items.Clear()

            AddTemperatureTile()
            AddHumidityTile()
            AddLuminanceTile()
            AddUVTile()
            AddSensorTile()
            AddVibrationTile()
            AddBatteryTile()
            AddZwaveLightTile()
            AddWeatherTile()
            AddTemperatureOutsideTile()
            AddHumidityOutsideTile()
            AddPressureTile()
            AddDateTile()
            AddClockTile()
            AddDayNightTile()
            AddPresenceTile()

            ' Retrieve the Weather info
            GetWeather()

            ' Activate the colorscheme - day or night
            ChangeColorScheme(ActualColorSchema)

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine InitTiles:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub LogAppSettings()

        SendLog("Log", "String", "Appsettings.AppMeasurementInterval: " & AppSettings.AppMeasurementInterval.ToString)
        SendLog("Log", "String", "Appsettings.AppSendMessage: " & AppSettings.AppSendMessage.ToString)
        SendLog("Log", "String", "Appsettings.AppSendMessageInterval: " & AppSettings.AppSendMessageInterval.ToString)
        SendLog("Log", "String", "Appsettings.AppTemperatureCorrection: " & AppSettings.AppTemperatureCorrection.ToString)
        SendLog("Log", "String", "Appsettings.AppUseZwave: " & AppSettings.AppUseZwave.ToString)
        SendLog("Log", "String", "Appsettings.AppVid: " & AppSettings.AppVid.ToString)
        SendLog("Log", "String", "Appsettings.AppPid: " & AppSettings.AppPid.ToString)

    End Sub

    Protected Sub GetDeviceInformation()

        ' ========================
        ' Retrieve the device info
        ' ========================
        Try
            If AppSettings.Device Is Nothing Then
                AppSettings.Device = New Device()
            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetDeviceInformation:" & ex.Message.ToString)
        End Try
    End Sub

    Protected Sub GetNetworkInformation()

        ' ========================================================
        ' Retrieve the network info (The WiFi adapter can change!)
        ' ========================================================
        Try
            AppSettings.Network = New Network()
            GetDeviceInformation()
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetNetWorkInformation:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub LogSysteminfo()

        ' ===================================================
        ' Send the device- and network info to the webservice
        ' ===================================================
        SendLog("Log", "String", "Productname: " & AppSettings.Device.Productname.ToString)
        SendLog("Log", "String", "Product: " & AppSettings.Device.Product.ToString)
        SendLog("Log", "String", "ProductID: " & AppSettings.Device.ProductID.ToString)
        SendLog("Log", "String", "Devicetype: " & AppSettings.Device.Devicetype.ToString)
        SendLog("Log", "String", "OS: " & AppSettings.Device.OS.ToString)
        If Not AppSettings.Network.Network Is Nothing Then
            SendLog("Log", "String", "Network: " & vbCrLf & AppSettings.Network.Network.ToString)
            SendLog("Log", "String", "IP address: " & AppSettings.Network.IPaddress.ToString)
            SendLog("Log", "String", "AdapterID: " & AppSettings.Network.AdapterID.ToString)
        Else
            SendLog("Log", "String", "NO NETWORK FOUND")
        End If

    End Sub

    Protected Sub InitTemperature()

        ' ================================================
        ' Initialize high- and low values for temperature
        ' ================================================

        Temperature.LowValue = AppSettings.AppLowTemperatureLevel
        Temperature.HighValue = AppSettings.AppHighHumidityLevel

    End Sub

    Protected Sub InitHumidity()

        ' =====================================================
        ' Initialize high- and low values for humidity
        ' =====================================================

        Humidity.LowValue = AppSettings.AppLowHumidityLevel
        Humidity.HighValue = AppSettings.AppHighHumidityLevel

    End Sub
#End Region

#Region "Tiles"
    Protected Sub AddTemperatureTile()

        If AppSettings.TileTemperature = True Then

            ' GridViewItem
            gvTemperature.Name = "gvTemperature"
            gvTemperature.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlTemperature.Name = "PnlTemperature"
            PnlTemperature.Width = 120
            PnlTemperature.Height = 120
            PnlTemperature.HorizontalAlignment = HorizontalAlignment.Center
            PnlTemperature.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlTemperature.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock 1
            TileTemperatureTitle.Text = ResourceLoader.GetForCurrentView.GetString("Temperature")
            TileTemperatureTitle.FontSize = 12
            TileTemperatureTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileTemperatureTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileTemperatureTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileTemperatureTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgTemperature.Name = "ImgTemperature"
            ImgTemperature.Height = 30
            ImgTemperature.Width = 30
            ImgTemperature.Margin = New Thickness(80, 70, 0, 0)
            ImgTemperature.Source = New BitmapImage(New Uri("ms-appx:///images/temperature.png", UriKind.Absolute))

            ' TextBlock 2
            TileTemperatureValue.Name = "TileTemperature"
            TileTemperatureValue.Text = "-"
            TileTemperatureValue.FontSize = 30
            TileTemperatureValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileTemperatureValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileTemperatureValue.HorizontalAlignment = HorizontalAlignment.Center
            TileTemperatureValue.Margin = New Thickness(0, 30, 0, 0)

            ' Add the elements to the grid
            PnlTemperature.Children.Add(TileTemperatureTitle)
            PnlTemperature.Children.Add(ImgTemperature)
            PnlTemperature.Children.Add(TileTemperatureValue)

            ' Add the grid to the GridViewItem
            gvTemperature.Content = PnlTemperature

            ' Add the GridViewItem to the tiles
            GridTiles.Items.Add(gvTemperature)

        End If

    End Sub

    Protected Sub AddHumidityTile()

        If AppSettings.TileHumidity = True Then

            ' GridViewItem
            gvHumidity.Name = "gvHumidity"
            gvHumidity.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlHumidity.Name = "PnlHumidity"
            PnlHumidity.Width = 120
            PnlHumidity.Height = 120
            PnlHumidity.HorizontalAlignment = HorizontalAlignment.Center
            PnlHumidity.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlHumidity.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileHumidityTitle.Text = ResourceLoader.GetForCurrentView.GetString("Humidity")
            TileHumidityTitle.FontSize = 12
            TileHumidityTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileHumidityTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileHumidityTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileHumidityTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgHumidity.Name = "ImgHumidity"
            ImgHumidity.Height = 30
            ImgHumidity.Width = 30
            ImgHumidity.Margin = New Thickness(80, 70, 0, 0)
            ImgHumidity.Source = New BitmapImage(New Uri("ms-appx:///images/humidity.png", UriKind.Absolute))

            ' TextBlock2
            TileHumidityValue.Name = "TileHumidity"
            TileHumidityValue.Text = "-"
            TileHumidityValue.FontSize = 30
            TileHumidityValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileHumidityValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileHumidityValue.HorizontalAlignment = HorizontalAlignment.Center
            TileHumidityValue.Margin = New Thickness(0, 30, 0, 0)

            PnlHumidity.Children.Add(TileHumidityTitle)
            PnlHumidity.Children.Add(ImgHumidity)
            PnlHumidity.Children.Add(TileHumidityValue)

            gvHumidity.Content = PnlHumidity

            GridTiles.Items.Add(gvHumidity)
        End If

    End Sub

    Protected Sub AddLuminanceTile()

        If AppSettings.TileLuminance = True Then

            ' GridViewItem
            gvLuminance.Name = "gvLuminance"
            gvLuminance.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlLuminance.Name = "PnlLuminance"
            PnlLuminance.Width = 120
            PnlLuminance.Height = 120
            PnlLuminance.HorizontalAlignment = HorizontalAlignment.Center
            PnlLuminance.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlLuminance.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileLuminanceTitle.Text = ResourceLoader.GetForCurrentView.GetString("Luminance")
            TileLuminanceTitle.FontSize = 12
            TileLuminanceTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileLuminanceTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileLuminanceTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileLuminanceTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgLuminance.Name = "ImgLuminance"
            ImgLuminance.Height = 30
            ImgLuminance.Width = 30
            ImgLuminance.Margin = New Thickness(80, 70, 0, 0)
            ImgLuminance.Source = New BitmapImage(New Uri("ms-appx:///images/Luminance.png", UriKind.Absolute))

            ' TextBlock2
            TileLuminanceValue.Name = "TileLuminance"
            TileLuminanceValue.Text = "-"
            TileLuminanceValue.FontSize = 30
            TileLuminanceValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileLuminanceValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileLuminanceValue.HorizontalAlignment = HorizontalAlignment.Center
            TileLuminanceValue.Margin = New Thickness(0, 30, 0, 0)

            PnlLuminance.Children.Add(TileLuminanceTitle)
            PnlLuminance.Children.Add(ImgLuminance)
            PnlLuminance.Children.Add(TileLuminanceValue)

            gvLuminance.Content = PnlLuminance

            GridTiles.Items.Add(gvLuminance)
        End If

    End Sub

    Protected Sub AddUVTile()

        If AppSettings.TileUltraviolet = True Then

            ' GridViewItem
            gvUltraviolet.Name = "gvUltraviolet"
            gvUltraviolet.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlUltraviolet.Name = "PnlUltraviolet"
            PnlUltraviolet.Width = 120
            PnlUltraviolet.Height = 120
            PnlUltraviolet.HorizontalAlignment = HorizontalAlignment.Center
            PnlUltraviolet.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlUltraviolet.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileUVTitle.Text = ResourceLoader.GetForCurrentView.GetString("UV")
            TileUVTitle.FontSize = 12
            TileUVTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileUVTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileUVTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileUVTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgUltraviolet.Name = "ImgUltraviolet"
            ImgUltraviolet.Height = 30
            ImgUltraviolet.Width = 30
            ImgUltraviolet.Margin = New Thickness(80, 70, 0, 0)
            ImgUltraviolet.Source = New BitmapImage(New Uri("ms-appx:///images/uv.png", UriKind.Absolute))

            ' TextBlock2
            TileUVValue.Name = "TileUltraviolet"
            TileUVValue.Text = "-"
            TileUVValue.FontSize = 30
            TileUVValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileUVValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileUVValue.HorizontalAlignment = HorizontalAlignment.Center
            TileUVValue.Margin = New Thickness(0, 30, 0, 0)

            PnlUltraviolet.Children.Add(TileUVTitle)
            PnlUltraviolet.Children.Add(ImgUltraviolet)
            PnlUltraviolet.Children.Add(TileUVValue)

            gvUltraviolet.Content = PnlUltraviolet

            GridTiles.Items.Add(gvUltraviolet)
        End If

    End Sub

    Protected Sub AddSensorTile()

        If AppSettings.TileMotion = True Then

            ' GridViewItem
            gvMotion.Name = "gvMotion"
            gvMotion.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlMotion.Name = "PnlMotion"
            PnlMotion.Width = 120
            PnlMotion.Height = 120
            PnlMotion.HorizontalAlignment = HorizontalAlignment.Center
            PnlMotion.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlMotion.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileMotionTitle.Text = ResourceLoader.GetForCurrentView.GetString("Sensor")
            TileMotionTitle.FontSize = 12
            TileMotionTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileMotionTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileMotionTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileMotionTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgMotion.Name = "ImgMotion"
            ImgMotion.Height = 30
            ImgMotion.Width = 30
            ImgMotion.Margin = New Thickness(80, 70, 0, 0)
            ImgMotion.Source = New BitmapImage(New Uri("ms-appx:///images/sensor.png", UriKind.Absolute))

            ' TextBlock2
            TileMotionValue.Name = "TileMotion"
            TileMotionValue.Text = "-"
            TileMotionValue.FontSize = 30
            TileMotionValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileMotionValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileMotionValue.HorizontalAlignment = HorizontalAlignment.Center
            TileMotionValue.Margin = New Thickness(0, 30, 0, 0)

            ' TextBlockDate
            TileMotionDate.Name = "TileMotionDate"
            TileMotionDate.Text = ""
            TileMotionDate.FontSize = 10
            TileMotionDate.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileMotionDate.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileMotionDate.HorizontalAlignment = HorizontalAlignment.Center
            TileMotionDate.Margin = New Thickness(0, 70, 0, 0)

            PnlMotion.Children.Add(TileMotionTitle)
            PnlMotion.Children.Add(ImgMotion)
            PnlMotion.Children.Add(TileMotionValue)
            PnlMotion.Children.Add(TileMotionDate)

            gvMotion.Content = PnlMotion

            GridTiles.Items.Add(gvMotion)
        End If

    End Sub

    Protected Sub AddVibrationTile()

        If AppSettings.TileVibration = True Then

            ' GridViewItem
            gvVibration.Name = "gvVibration"
            gvVibration.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlVibration.Name = "PnlVibration"
            PnlVibration.Width = 120
            PnlVibration.Height = 120
            PnlVibration.HorizontalAlignment = HorizontalAlignment.Center
            PnlVibration.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlVibration.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileVibrationTitle.Text = ResourceLoader.GetForCurrentView.GetString("Vibration")
            TileVibrationTitle.FontSize = 12
            TileVibrationTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileVibrationTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileVibrationTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileVibrationTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgVibration.Name = "ImgVibration"
            ImgVibration.Height = 30
            ImgVibration.Width = 30
            ImgVibration.Margin = New Thickness(80, 70, 0, 0)
            ImgVibration.Source = New BitmapImage(New Uri("ms-appx:///images/Vibration.png", UriKind.Absolute))

            ' TextBlock2
            TileVibrationValue.Name = "TileVibration"
            TileVibrationValue.Text = "-"
            TileVibrationValue.FontSize = 30
            TileVibrationValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileVibrationValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileVibrationValue.HorizontalAlignment = HorizontalAlignment.Center
            TileVibrationValue.Margin = New Thickness(0, 30, 0, 0)

            ' TextBlockDate
            TileVibrationDate.Name = "TileVibrationDate"
            TileVibrationDate.Text = ""
            TileVibrationDate.FontSize = 10
            TileVibrationDate.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileVibrationDate.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileVibrationDate.HorizontalAlignment = HorizontalAlignment.Center
            TileVibrationDate.Margin = New Thickness(0, 70, 0, 0)

            PnlVibration.Children.Add(TileVibrationTitle)
            PnlVibration.Children.Add(ImgVibration)
            PnlVibration.Children.Add(TileVibrationValue)
            PnlVibration.Children.Add(TileVibrationDate)

            gvVibration.Content = PnlVibration

            GridTiles.Items.Add(gvVibration)
        End If

    End Sub

    Protected Sub AddBatteryTile()

        If AppSettings.TileBattery = True Then

            ' GridViewItem
            gvBattery.Name = "gvBattery"
            gvBattery.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlBattery.Name = "PnlBattery"
            PnlBattery.Width = 120
            PnlBattery.Height = 120
            PnlBattery.HorizontalAlignment = HorizontalAlignment.Center
            PnlBattery.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            PnlBattery.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileBatteryTitle.Text = ResourceLoader.GetForCurrentView.GetString("Battery")
            TileBatteryTitle.FontSize = 12
            TileBatteryTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileBatteryTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileBatteryTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileBatteryTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgBattery.Name = "ImgBattery"
            ImgBattery.Height = 30
            ImgBattery.Width = 30
            ImgBattery.Margin = New Thickness(80, 70, 0, 0)
            ImgBattery.Source = New BitmapImage(New Uri("ms-appx:///images/Battery0%.png", UriKind.Absolute))

            ' TextBlock2
            TileBatteryValue.Name = "TileBattery"
            TileBatteryValue.Text = "-"
            TileBatteryValue.FontSize = 30
            TileBatteryValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileBatteryValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileBatteryValue.HorizontalAlignment = HorizontalAlignment.Center
            TileBatteryValue.Margin = New Thickness(0, 30, 0, 0)

            PnlBattery.Children.Add(TileBatteryTitle)
            PnlBattery.Children.Add(ImgBattery)
            PnlBattery.Children.Add(TileBatteryValue)

            gvBattery.Content = PnlBattery

            GridTiles.Items.Add(gvBattery)
        End If

    End Sub

    Protected Sub AddZwaveLightTile()

        TileZwaveLightNodeIDs.Clear()

        If AppSettings.TileZwaveLight = True Then

            For Each AppNode As AppNode In AppNodes
                If AppNode.MeterType = MeterType.Electric Then

                    ' Zoe het NodeID op in de TileID's
                    Dim TileIndex As Integer = GetTileIndex(AppNode.NodeID)

                    ' Er is nog geen Tile aangemaakt voor deze node
                    If TileIndex = -1 Then
                        TileZwaveLightNodeIDs.Add(AppNode.NodeID)
                        TileIndex = TileZwaveLightNodeIDs.Count - 1
                    End If

                    Array.Resize(gvZwaveLight, gvZwaveLight.Length + 1)
                    gvZwaveLight(TileIndex) = New GridViewItem
                    gvZwaveLight(TileIndex).Name = "gvZwaveLight" & AppNode.NodeID.ToString
                    gvZwaveLight(TileIndex).Margin = New Thickness(5, 5, 5, 5)

                    ' Grid
                    Array.Resize(PnlZwaveLight, PnlZwaveLight.Length + 1)
                    PnlZwaveLight(TileIndex) = New Grid
                    PnlZwaveLight(TileIndex).Name = "PnlZwaveLight" & AppNode.NodeID.ToString
                    PnlZwaveLight(TileIndex).Width = 120
                    PnlZwaveLight(TileIndex).Height = 120
                    PnlZwaveLight(TileIndex).HorizontalAlignment = HorizontalAlignment.Center
                    PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Green)
                    PnlZwaveLight(TileIndex).BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

                    ' TextBlock1
                    Array.Resize(TileZwaveLightTitle, TileZwaveLightTitle.Length + 1)
                    TileZwaveLightTitle(TileIndex) = New TextBlock
                    TileZwaveLightTitle(TileIndex).Text = ResourceLoader.GetForCurrentView.GetString("Light") & " (" & AppNode.NodeID.ToString & ")"
                    TileZwaveLightTitle(TileIndex).FontSize = 12
                    TileZwaveLightTitle(TileIndex).SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
                    TileZwaveLightTitle(TileIndex).Foreground = New SolidColorBrush(Windows.UI.Colors.White)
                    TileZwaveLightTitle(TileIndex).HorizontalAlignment = HorizontalAlignment.Left
                    TileZwaveLightTitle(TileIndex).Margin = New Thickness(5, 0, 5, 0)

                    ' Image
                    Array.Resize(ImgZwaveLight, ImgZwaveLight.Length + 1)
                    ImgZwaveLight(TileIndex) = New Image
                    ImgZwaveLight(TileIndex).Name = "ImgZwaveLight" & AppNode.NodeID.ToString
                    ImgZwaveLight(TileIndex).Height = 50
                    ImgZwaveLight(TileIndex).Width = 50
                    ImgZwaveLight(TileIndex).Margin = New Thickness(0, 0, 0, 0)
                    ImgZwaveLight(TileIndex).Source = New BitmapImage(New Uri("ms-appx:///images/lightbulb-off.png", UriKind.Absolute))

                    ' Powerconsumption
                    Array.Resize(Powerconsumption, Powerconsumption.Length + 1)
                    Powerconsumption(TileIndex) = New TextBlock
                    Powerconsumption(TileIndex).Text = ""
                    Powerconsumption(TileIndex).FontSize = 11
                    Powerconsumption(TileIndex).Foreground = New SolidColorBrush(Windows.UI.Colors.White)
                    Powerconsumption(TileIndex).Margin = New Thickness(5, 12, 5, 0)

                    ' MinutesLeft
                    Array.Resize(MinutesLeft, MinutesLeft.Length + 1)
                    MinutesLeft(TileIndex) = New TextBlock
                    MinutesLeft(TileIndex).Text = ""
                    MinutesLeft(TileIndex).FontSize = 11
                    MinutesLeft(TileIndex).SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
                    MinutesLeft(TileIndex).Foreground = New SolidColorBrush(Windows.UI.Colors.White)
                    MinutesLeft(TileIndex).HorizontalAlignment = HorizontalAlignment.Right
                    MinutesLeft(TileIndex).VerticalAlignment = VerticalAlignment.Bottom
                    MinutesLeft(TileIndex).Margin = New Thickness(5, 2, 5, 0)

                    PnlZwaveLight(TileIndex).Children.Add(TileZwaveLightTitle(TileIndex))
                    PnlZwaveLight(TileIndex).Children.Add(ImgZwaveLight(TileIndex))
                    PnlZwaveLight(TileIndex).Children.Add(Powerconsumption(TileIndex))
                    PnlZwaveLight(TileIndex).Children.Add(MinutesLeft(TileIndex))

                    gvZwaveLight(TileIndex).Content = PnlZwaveLight(TileIndex)

                    GridTiles.Items.Add(gvZwaveLight(TileIndex))

                End If

            Next

        End If

    End Sub

    Protected Function GetTileIndex(ByVal NodeID As Byte)

        Dim TileIndex As Integer = -1

        If Not TileZwaveLightNodeIDs Is Nothing Then
            For i = 0 To TileZwaveLightNodeIDs.Count - 1
                If TileZwaveLightNodeIDs(i) = NodeID Then
                    TileIndex = i
                End If
            Next
        End If

        Return TileIndex

    End Function

    Protected Sub AddWeatherTile()

        If AppSettings.TileWeather = True Then

            ' GridViewItem
            gvWeather.Name = "gvWeather"
            gvWeather.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlWeather.Name = "PnlWeather"
            PnlWeather.Width = 120
            PnlWeather.Height = 120
            PnlWeather.HorizontalAlignment = HorizontalAlignment.Center
            PnlWeather.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlWeather.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileWeatherTitle.Text = ResourceLoader.GetForCurrentView.GetString("Weather")
            TileWeatherTitle.FontSize = 12
            TileWeatherTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileWeatherTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileWeatherTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileWeatherTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgWeather.Name = "ImgWeather"
            ImgWeather.Height = 60
            ImgWeather.Width = 125
            ImgWeather.Margin = New Thickness(0, 0, 0, 0)

            ' Textblock 2
            WeatherText.Text = ""
            WeatherText.FontSize = 11
            WeatherText.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            WeatherText.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            WeatherText.HorizontalAlignment = HorizontalAlignment.Right
            WeatherText.VerticalAlignment = VerticalAlignment.Bottom
            WeatherText.Margin = New Thickness(5, 2, 5, 0)

            PnlWeather.Children.Add(TileWeatherTitle)
            PnlWeather.Children.Add(ImgWeather)
            PnlWeather.Children.Add(WeatherText)

            gvWeather.Content = PnlWeather

            GridTiles.Items.Add(gvWeather)
        End If

    End Sub

    Protected Sub AddTemperatureOutsideTile()

        If AppSettings.TileWeather = True Then

            ' GridViewItem
            gvTemperatureOutside.Name = "gvTemperatureOutside"
            gvTemperatureOutside.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlTemperatureOutside.Name = "PnlTemperatureOutside"
            PnlTemperatureOutside.Width = 120
            PnlTemperatureOutside.Height = 120
            PnlTemperatureOutside.HorizontalAlignment = HorizontalAlignment.Center
            PnlTemperatureOutside.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlTemperatureOutside.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock 1
            TileTemperatureOutsideTitle.Text = ResourceLoader.GetForCurrentView.GetString("Temperature")
            TileTemperatureOutsideTitle.FontSize = 12
            TileTemperatureOutsideTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileTemperatureOutsideTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileTemperatureOutsideTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileTemperatureOutsideTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgTemperatureOutside.Name = "ImgTemperatureOutside"
            ImgTemperatureOutside.Height = 30
            ImgTemperatureOutside.Width = 30
            ImgTemperatureOutside.Margin = New Thickness(80, 70, 0, 0)
            ImgTemperatureOutside.Source = New BitmapImage(New Uri("ms-appx:///images/Temperature.png", UriKind.Absolute))

            ' TextBlock 2
            TileTemperatureOutsideValue.Name = "TileTemperatureOutside"
            TileTemperatureOutsideValue.Text = "-"
            TileTemperatureOutsideValue.FontSize = 30
            TileTemperatureOutsideValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileTemperatureOutsideValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileTemperatureOutsideValue.HorizontalAlignment = HorizontalAlignment.Center
            TileTemperatureOutsideValue.Margin = New Thickness(0, 30, 0, 0)

            PnlTemperatureOutside.Children.Add(TileTemperatureOutsideTitle)
            PnlTemperatureOutside.Children.Add(ImgTemperatureOutside)
            PnlTemperatureOutside.Children.Add(TileTemperatureOutsideValue)

            gvTemperatureOutside.Content = PnlTemperatureOutside

            GridTiles.Items.Add(gvTemperatureOutside)

        End If

    End Sub

    Protected Sub AddHumidityOutsideTile()

        If AppSettings.TileWeather = True Then

            ' GridViewItem
            gvHumidityOutside.Name = "gvHumidityOutside"
            gvHumidityOutside.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlHumidityOutside.Name = "PnlHumidityOutside"
            PnlHumidityOutside.Width = 120
            PnlHumidityOutside.Height = 120
            PnlHumidityOutside.HorizontalAlignment = HorizontalAlignment.Center
            PnlHumidityOutside.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlHumidityOutside.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock 1
            TileHumidityOutsideTitle.Text = ResourceLoader.GetForCurrentView.GetString("Humidity")
            TileHumidityOutsideTitle.FontSize = 12
            TileHumidityOutsideTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileHumidityOutsideTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileHumidityOutsideTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileHumidityOutsideTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgHumidityOutside.Name = "ImgHumidityOutside"
            ImgHumidityOutside.Height = 30
            ImgHumidityOutside.Width = 30
            ImgHumidityOutside.Margin = New Thickness(80, 70, 0, 0)
            ImgHumidityOutside.Source = New BitmapImage(New Uri("ms-appx:///images/Humidity.png", UriKind.Absolute))

            ' TextBlock 2
            TileHumidityOutsideValue.Name = "TileHumidityOutside"
            TileHumidityOutsideValue.Text = "-"
            TileHumidityOutsideValue.FontSize = 30
            TileHumidityOutsideValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileHumidityOutsideValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileHumidityOutsideValue.HorizontalAlignment = HorizontalAlignment.Center
            TileHumidityOutsideValue.Margin = New Thickness(0, 30, 0, 0)

            PnlHumidityOutside.Children.Add(TileHumidityOutsideTitle)
            PnlHumidityOutside.Children.Add(ImgHumidityOutside)
            PnlHumidityOutside.Children.Add(TileHumidityOutsideValue)

            gvHumidityOutside.Content = PnlHumidityOutside

            GridTiles.Items.Add(gvHumidityOutside)

        End If

    End Sub

    Protected Sub AddPressureTile()

        If AppSettings.TileWeather = True Then

            ' GridViewItem
            gvPressure.Name = "gvPressure"
            gvPressure.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlPressure.Name = "PnlPressure"
            PnlPressure.Width = 120
            PnlPressure.Height = 120
            PnlPressure.HorizontalAlignment = HorizontalAlignment.Center
            PnlPressure.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlPressure.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock 1
            TilePressureTitle.Text = ResourceLoader.GetForCurrentView.GetString("Pressure (hPa)")
            TilePressureTitle.FontSize = 12
            TilePressureTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TilePressureTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TilePressureTitle.HorizontalAlignment = HorizontalAlignment.Left
            TilePressureTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgPressure.Name = "ImgPressure"
            ImgPressure.Height = 30
            ImgPressure.Width = 30
            ImgPressure.Margin = New Thickness(80, 70, 0, 0)
            ImgPressure.Source = New BitmapImage(New Uri("ms-appx:///images/Pressure.png", UriKind.Absolute))

            ' TextBlock 2
            TilePressureValue.Name = "TilePressure"
            TilePressureValue.Text = "-"
            TilePressureValue.FontSize = 30
            TilePressureValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TilePressureValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TilePressureValue.HorizontalAlignment = HorizontalAlignment.Center
            TilePressureValue.Margin = New Thickness(0, 30, 0, 0)

            PnlPressure.Children.Add(TilePressureTitle)
            PnlPressure.Children.Add(ImgPressure)
            PnlPressure.Children.Add(TilePressureValue)

            gvPressure.Content = PnlPressure

            GridTiles.Items.Add(gvPressure)

        End If

    End Sub

    Protected Sub AddClockTile()

        If AppSettings.TileClock = True Then

            ' GridViewItem
            gvClock.Name = "gvClock"
            gvClock.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlClock.Name = "PnlClock"
            PnlClock.Width = 120
            PnlClock.Height = 120
            PnlClock.HorizontalAlignment = HorizontalAlignment.Center
            PnlClock.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlClock.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileClockTitle.Text = ""
            TileClockTitle.FontSize = 12
            TileClockTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileClockTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileClockTitle.HorizontalAlignment = HorizontalAlignment.Center
            TileClockTitle.Margin = New Thickness(5, 0, 5, 0)

            Dim Grid As New Grid
            Grid.Height = 100
            Grid.Margin = New Thickness(0, -8, 0, 0)

            Ellipse.Height = 80
            Ellipse.Width = 80
            Ellipse.Stroke = New SolidColorBrush(Windows.UI.Colors.White)
            Ellipse.StrokeThickness = 4
            Ellipse.StrokeDashArray = New DoubleCollection() From {1, 4}
            Ellipse.Fill = New SolidColorBrush(Windows.UI.Colors.Transparent)

            Seconds.Margin = New Thickness(60, 17, 59, 49)
            Seconds.Name = "second"
            Seconds.Stroke = New SolidColorBrush(Windows.UI.Colors.Red)
            SecondHand.CenterX = 0
            SecondHand.CenterY = 33
            SecondHand.Angle = 0
            Seconds.RenderTransform = SecondHand

            Minutes.Margin = New Thickness(60, 17, 59, 49)
            Minutes.Name = "minute"
            Minutes.Stroke = New SolidColorBrush(Windows.UI.Colors.White)
            MinuteHand.CenterX = 0
            MinuteHand.CenterY = 33
            MinuteHand.Angle = 0
            Minutes.RenderTransform = MinuteHand

            Hours.Margin = New Thickness(60, 28, 59, 49)
            Hours.Name = "hour"
            Hours.Stroke = New SolidColorBrush(Windows.UI.Colors.White)
            HourHand.CenterX = 0
            HourHand.CenterY = 23
            HourHand.Angle = 0
            Hours.RenderTransform = HourHand

            ' Date
            TodaysDate.Text = ""
            TodaysDate.FontSize = 11
            TodaysDate.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TodaysDate.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TodaysDate.HorizontalAlignment = HorizontalAlignment.Center
            TodaysDate.VerticalAlignment = VerticalAlignment.Bottom
            TodaysDate.Margin = New Thickness(5, 2, 5, -7)

            Grid.Children.Add(TodaysDate)
            Grid.Children.Add(Ellipse)
            Grid.Children.Add(Hours)
            Grid.Children.Add(Minutes)
            Grid.Children.Add(Seconds) 'Add last to get te secondhand on top of the clock

            PnlClock.Children.Add(TileClockTitle)
            PnlClock.Children.Add(Grid)

            gvClock.Content = PnlClock

            GridTiles.Items.Add(gvClock)
        End If

    End Sub

    Protected Sub AddDayNightTile()


        ' GridViewItem
        gvDayNight.Name = "gvDayNight"
        gvDayNight.Margin = New Thickness(5, 5, 5, 5)

        ' Grid
        PnlDayNight.Name = "PnlDayNight"
        PnlDayNight.Width = 120
        PnlDayNight.Height = 120
        PnlDayNight.HorizontalAlignment = HorizontalAlignment.Center
        PnlDayNight.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
        PnlDayNight.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

        ' TextBlock1
        TileDayNightTitle.Text = ResourceLoader.GetForCurrentView.GetString("Colorscheme")
        TileDayNightTitle.FontSize = 12
        TileDayNightTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
        TileDayNightTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
        TileDayNightTitle.HorizontalAlignment = HorizontalAlignment.Left
        TileDayNightTitle.Margin = New Thickness(5, 0, 5, 0)

        ' Image
        ImgDayNight.Name = "ImgDayNight"
        ImgDayNight.Height = 50
        ImgDayNight.Width = 50
        ImgDayNight.Margin = New Thickness(0, 0, 0, 0)
        ImgDayNight.Source = New BitmapImage(New Uri("ms-appx:///images/day.png", UriKind.Absolute))

        PnlDayNight.Children.Add(TileDayNightTitle)
        PnlDayNight.Children.Add(ImgDayNight)

        gvDayNight.Content = PnlDayNight

        GridTiles.Items.Add(gvDayNight)

    End Sub

    Protected Sub AddPresenceTile()

        If AppSettings.TilePresence = True Then

            ' GridViewItem
            gvPresence.Name = "gvPresence"
            gvPresence.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlPresence.Name = "PnlPresence"
            PnlPresence.Width = 120
            PnlPresence.Height = 120
            PnlPresence.HorizontalAlignment = HorizontalAlignment.Center
            PnlPresence.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlPresence.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TilePresenceTitle.Text = ResourceLoader.GetForCurrentView.GetString("Presence")
            TilePresenceTitle.FontSize = 12
            TilePresenceTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TilePresenceTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TilePresenceTitle.HorizontalAlignment = HorizontalAlignment.Left
            TilePresenceTitle.Margin = New Thickness(5, 0, 5, 0)

            ' Image
            ImgPresence.Name = "ImgPresence"
            ImgPresence.Height = 40
            ImgPresence.Width = 40
            ImgPresence.Margin = New Thickness(0, 0, 0, 0)
            ImgPresence.Source = New BitmapImage(New Uri("ms-appx:///images/in.png", UriKind.Absolute))

            PnlPresence.Children.Add(TilePresenceTitle)
            PnlPresence.Children.Add(ImgPresence)

            gvPresence.Content = PnlPresence

            GridTiles.Items.Add(gvPresence)
        End If
    End Sub


    Protected Sub AddDateTile()

        If AppSettings.TileDate = True Then

            ' GridViewItem
            gvDate.Name = "gvDate"
            gvDate.Margin = New Thickness(5, 5, 5, 5)

            ' Grid
            PnlDate.Name = "PnlDate"
            PnlDate.Width = 120
            PnlDate.Height = 120
            PnlDate.HorizontalAlignment = HorizontalAlignment.Center
            PnlDate.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            PnlDate.BorderBrush = New SolidColorBrush(Windows.UI.Colors.Black)

            ' TextBlock1
            TileDateTitle.Text = ResourceLoader.GetForCurrentView.GetString("Date")
            TileDateTitle.FontSize = 12
            TileDateTitle.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileDateTitle.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileDateTitle.HorizontalAlignment = HorizontalAlignment.Left
            TileDateTitle.Margin = New Thickness(5, 0, 5, 0)

            ' TextBlock2
            TileDateValue.Name = "TileDate"
            TileDateValue.Text = DateTime.Today.ToString("dd:MM:yyyy")
            TileDateValue.FontSize = 20
            TileDateValue.SetValue(TextBlock.FontWeightProperty, Text.FontWeights.Bold)
            TileDateValue.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileDateValue.HorizontalAlignment = HorizontalAlignment.Center
            TileDateValue.Margin = New Thickness(0, 40, 0, 0)

            ' TextBlock2
            TileDateWeekday.Name = "TileDate"
            TileDateWeekday.Text = ""
            TileDateWeekday.FontSize = 14
            TileDateWeekday.Foreground = New SolidColorBrush(Windows.UI.Colors.White)
            TileDateWeekday.HorizontalAlignment = HorizontalAlignment.Center
            TileDateWeekday.Margin = New Thickness(0, 65, 0, 0)

            PnlDate.Children.Add(TileDateTitle)
            PnlDate.Children.Add(TileDateValue)
            PnlDate.Children.Add(TileDateWeekday)

            gvDate.Content = PnlDate

            GridTiles.Items.Add(gvDate)
        End If

    End Sub

    Protected Sub ChangeColorScheme(ByVal Scheme As Byte)

        If Scheme = Colorscheme.Night Then

            PnlTemperature.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileTemperatureTitle.Opacity = 0.1
            TileTemperatureValue.Opacity = 0.1
            ImgTemperature.Opacity = 0.1

            PnlHumidity.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileHumidityTitle.Opacity = 0.1
            TileHumidityValue.Opacity = 0.1
            ImgHumidity.Opacity = 0.1

            PnlLuminance.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileLuminanceTitle.Opacity = 0.1
            TileLuminanceValue.Opacity = 0.1
            ImgLuminance.Opacity = 0.1

            PnlUltraviolet.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileUVTitle.Opacity = 0.1
            TileUVValue.Opacity = 0.1
            ImgUltraviolet.Opacity = 0.1

            PnlMotion.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileMotionTitle.Opacity = 0.1
            TileMotionValue.Opacity = 0.1
            TileMotionDate.Opacity = 0.1
            ImgMotion.Opacity = 0.1

            PnlVibration.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileVibrationTitle.Opacity = 0.1
            TileVibrationValue.Opacity = 0.1
            TileVibrationDate.Opacity = 0.1
            ImgVibration.Opacity = 0.1

            PnlBattery.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileBatteryTitle.Opacity = 0.1
            TileBatteryValue.Opacity = 0.1
            ImgBattery.Opacity = 0.1

            For Each AppNode As AppNode In AppNodes
                If AppNode.MeterType = MeterType.Electric Then

                    Dim TileIndex As Integer = GetTileIndex(AppNode.NodeID)
                    If Not TileIndex = -1 Then
                        PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
                        TileZwaveLightTitle(TileIndex).Opacity = 0.1
                        Powerconsumption(TileIndex).Opacity = 0.1
                        MinutesLeft(TileIndex).Opacity = 0.1
                        ImgZwaveLight(TileIndex).Opacity = 0.1
                    End If
                End If
            Next

            PnlWeather.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileWeatherTitle.Opacity = 0.1
            WeatherText.Opacity = 0.1
            ImgWeather.Opacity = 0.1

            PnlTemperatureOutside.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileTemperatureOutsideTitle.Opacity = 0.1
            TileTemperatureOutsideValue.Opacity = 0.1
            ImgTemperatureOutside.Opacity = 0.1

            PnlHumidityOutside.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileHumidityOutsideTitle.Opacity = 0.1
            TileHumidityOutsideValue.Opacity = 0.1
            ImgHumidityOutside.Opacity = 0.1

            PnlPressure.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TilePressureTitle.Opacity = 0.1
            TilePressureValue.Opacity = 0.1
            ImgPressure.Opacity = 0.1

            PnlClock.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileClockTitle.Opacity = 0.1
            TodaysDate.Opacity = 0.1
            Ellipse.Opacity = 0.1
            Seconds.Opacity = 0.5
            Minutes.Opacity = 0.1
            Hours.Opacity = 0.1

            PnlDayNight.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileDayNightTitle.Opacity = 0.1
            ImgDayNight.Source = New BitmapImage(New Uri("ms-appx:///images/night.png", UriKind.Absolute))

            PnlPresence.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TilePresenceTitle.Opacity = 0.1
            ImgPresence.Opacity = 0.1

            PnlDate.Background = New SolidColorBrush(Windows.UI.Color.FromArgb(255, 15, 15, 15))
            TileDateTitle.Opacity = 0.1
            TileDateValue.Opacity = 0.1
            TileDateWeekday.Opacity = 0.1
        End If

        If Scheme = Colorscheme.Day Then

            PnlTemperature.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileTemperatureTitle.Opacity = 1
            TileTemperatureValue.Opacity = 1
            ImgTemperature.Opacity = 1

            PnlHumidity.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileHumidityTitle.Opacity = 1
            TileHumidityValue.Opacity = 1
            ImgHumidity.Opacity = 1

            PnlLuminance.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileLuminanceTitle.Opacity = 1
            TileLuminanceValue.Opacity = 1
            ImgLuminance.Opacity = 1

            PnlUltraviolet.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileUVTitle.Opacity = 1
            TileUVValue.Opacity = 1
            ImgUltraviolet.Opacity = 1

            PnlMotion.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileMotionTitle.Opacity = 1
            TileMotionValue.Opacity = 1
            TileMotionDate.Opacity = 1
            ImgMotion.Opacity = 1

            PnlVibration.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileVibrationTitle.Opacity = 1
            TileVibrationValue.Opacity = 1
            TileVibrationDate.Opacity = 1
            ImgVibration.Opacity = 1

            PnlBattery.Background = New SolidColorBrush(Windows.UI.Colors.Green)
            TileBatteryTitle.Opacity = 1
            TileBatteryValue.Opacity = 1
            ImgBattery.Opacity = 1

            For Each AppNode As AppNode In AppNodes
                If AppNode.MeterType = MeterType.Electric Then

                    Dim TileIndex As Integer = GetTileIndex(AppNode.NodeID)
                    If Not TileIndex = -1 Then
                        PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Green)
                        TileZwaveLightTitle(TileIndex).Opacity = 1
                        Powerconsumption(TileIndex).Opacity = 1
                        MinutesLeft(TileIndex).Opacity = 1
                        ImgZwaveLight(TileIndex).Opacity = 1
                    End If
                End If
            Next

            PnlWeather.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileWeatherTitle.Opacity = 1
            WeatherText.Opacity = 1
            ImgWeather.Opacity = 1

            PnlTemperatureOutside.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileTemperatureOutsideTitle.Opacity = 1
            TileTemperatureOutsideValue.Opacity = 1
            ImgTemperatureOutside.Opacity = 1

            PnlHumidityOutside.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileHumidityOutsideTitle.Opacity = 1
            TileHumidityOutsideValue.Opacity = 1
            ImgHumidityOutside.Opacity = 1

            PnlPressure.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TilePressureTitle.Opacity = 1
            TilePressureValue.Opacity = 1
            ImgPressure.Opacity = 1

            PnlClock.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileClockTitle.Opacity = 1
            TodaysDate.Opacity = 1
            Ellipse.Opacity = 1
            Seconds.Opacity = 1
            Minutes.Opacity = 1
            Hours.Opacity = 1

            PnlDayNight.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileDayNightTitle.Opacity = 1
            ImgDayNight.Source = New BitmapImage(New Uri("ms-appx:///images/day.png", UriKind.Absolute))

            PnlPresence.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TilePresenceTitle.Opacity = 1
            ImgPresence.Opacity = 1

            PnlDate.Background = New SolidColorBrush(Windows.UI.Colors.DarkSlateGray)
            TileDateTitle.Opacity = 1
            TileDateValue.Opacity = 1
            TileDateWeekday.Opacity = 1
        End If

        ActualColorSchema = Scheme

    End Sub

    Protected Sub SetWallplugColor(ByVal AppNode As AppNode)

        ' Colors:
        ' 0 – Black (-128)
        ' 1 – Green (-127)
        ' 2 – Blue (-126)
        ' 3 – Red (-125)
        ' 4 – Yellow (-124)
        ' 5 – Purple (-123)
        ' 6 – Orange (-122)
        ' 7 – Light blue (-121)
        ' 8 – Pink (-120)
        ' Blocked (-119)

        If ActualColorSchema = Colorscheme.Day Then

            Dim TileIndex As Integer = GetTileIndex(AppNode.NodeID)

            If Not TileIndex = -1 Then
                If Not AppNode.Color Is Nothing Then
                    Select Case AppNode.Color
                        Case -128
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Black)
                        Case -127
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Green)
                        Case -126
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Blue)
                        Case -125
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Red)
                        Case -124
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Yellow)
                        Case -123
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Purple)
                        Case -122
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Orange)
                        Case -121
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.LightBlue)
                        Case -120
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Pink)
                        Case Else
                            PnlZwaveLight(TileIndex).Background = New SolidColorBrush(Windows.UI.Colors.Green)
                    End Select
                End If
            End If
        End If

    End Sub

#End Region

#Region "Logic"
    Protected Sub StartTimers()

        ' Nodelist interval
        NodeListTimer = New DispatcherTimer()
        NodeListTimer.Interval = TimeSpan.FromSeconds(2)
        AddHandler NodeListTimer.Tick, AddressOf InitAppNodeList
        NodeListTimer.Start()

        ' Clock interval
        ClockTimer = New DispatcherTimer()
        ClockTimer.Interval = TimeSpan.FromSeconds(1)
        AddHandler ClockTimer.Tick, AddressOf ClockTimer_Tick
        ClockTimer.Start()

        ' TwoMinute interval
        TwoMinuteTimer = New DispatcherTimer()
        TwoMinuteTimer.Interval = TimeSpan.FromSeconds(120)
        AddHandler TwoMinuteTimer.Tick, AddressOf TwoMinuteTimer_Tick
        TwoMinuteTimer.Start()

    End Sub

    Protected Sub ClockTimer_Tick()

        ' ================
        ' Update the clock
        ' ================

        SecondHand.Angle = DateTime.Now.Second * 6
        MinuteHand.Angle = DateTime.Now.Minute * 6
        HourHand.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5)

        TileDateValue.Text = DateTime.Today.ToString("dd-MM-yyyy")

        Dim Weekday As DayOfWeek = DateTime.Today.DayOfWeek
        Select Case Weekday
            Case DayOfWeek.Monday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Monday")
            Case DayOfWeek.Tuesday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Tuesday")
            Case DayOfWeek.Wednesday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Wednesday")
            Case DayOfWeek.Thursday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Thursday")
            Case DayOfWeek.Friday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Friday")
            Case DayOfWeek.Saturday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Saturday")
            Case DayOfWeek.Sunday
                TileDateWeekday.Text = ResourceLoader.GetForCurrentView.GetString("Sunday")
        End Select

    End Sub

    Protected Async Sub TwoMinuteTimer_Tick()

        ' ======================================
        ' Update several tiles (every 2 minutes)
        ' ======================================

        ' Update the weather
        GetWeather()

        ' Update the battery status
        ShowBattery()

        ' Update the switch state
        For Each AppNode As AppNode In AppNodes
            If AppNode.MeterType = MeterType.Electric Then
                Await GetConfiguration(AppNode)
                SetWallplugColor(AppNode)
                ShowLightStatus()
            End If
        Next

    End Sub

    Protected Sub ShowTemperature()

        ' ======================
        ' Update the temperature
        ' ======================

        Try

            TileTemperatureValue.Text = Math.Round(Temperature.Waarde, 1).ToString & "°C"

            ' Set background color if tile in case of low- or high temperature
            If ActualColorSchema = Colorscheme.Day Then
                Select Case Temperature.Waarde
                    Case < Temperature.LowValue
                        PnlTemperature.Background = New SolidColorBrush(Colors.Blue)
                    Case Temperature.LowValue To Temperature.HighValue
                        PnlTemperature.Background = New SolidColorBrush(Colors.Green)
                    Case > Temperature.HighValue
                        PnlTemperature.Background = New SolidColorBrush(Colors.Red)
                End Select
            End If

        Catch ex As Exception
            SendService("Log", 0, "String", "Error in routine ShowTemperature: " & ex.ToString)
        End Try

    End Sub

    Protected Sub ShowHumidity()

        ' ===================
        ' Update the humidity
        ' ===================

        Try

            TileHumidityValue.Text = Math.Round(Humidity.Waarde, 1).ToString & "%"

            If ActualColorSchema = Colorscheme.Day Then
                Select Case Humidity.Waarde
                    Case <= Humidity.LowValue
                        PnlHumidity.Background = New SolidColorBrush(Colors.Red)
                    Case Humidity.LowValue To Humidity.HighValue
                        PnlHumidity.Background = New SolidColorBrush(Colors.Green)
                    Case >= Humidity.HighValue
                        PnlHumidity.Background = New SolidColorBrush(Colors.Red)
                End Select
            End If

        Catch ex As Exception
            SendService("Log", 0, "String", "Error in routine ShowHumidity: " & ex.ToString)
        End Try

    End Sub

    Protected Sub ShowLuminance()

        ' ====================
        ' Update the luminance
        ' ====================

        Try

            TileLuminanceValue.Text = Math.Round(Luminance.Waarde, 1).ToString & "Lux"

            Select Case Luminance.Waarde
                Case <= AppSettings.AppLowLuxLevel
                    ' Luminance below low-level?: save the date and tine
                    If Luminance.TimeLowValue = Date.MinValue Then
                        Luminance.TimeLowValue = DateTime.Now
                    End If
                Case Else
                    ' Luminance above low-level?: reset date and time
                    Luminance.TimeLowValue = Date.MinValue
            End Select

            ' Check if the lights will have to be switched on or off
            LightOnOff()

        Catch ex As Exception
            SendService("Log", 0, "String", "Error in routine ShowLuminance: " & ex.ToString)
        End Try

        ' Does the colorscheme have to be changed?
        If Luminance.Waarde > AppSettings.AppLowLuxLevel Then
            If ActualColorSchema = Colorscheme.Night Then
                ChangeColorScheme(Colorscheme.Day)
            End If
        Else
            If ActualColorSchema = Colorscheme.Day Then
                ChangeColorScheme(Colorscheme.Night)
            End If
        End If

    End Sub

    Protected Sub ShowLightStatus()

        Try

            ' Retrieve wallplug status
            For Each AppNode As AppNode In AppNodes
                If AppNode.MeterType = MeterType.Electric Then

                    Dim TileIndex As Integer = GetTileIndex(AppNode.NodeID)

                    If Not TileIndex = -1 Then
                        If AppNode.NodeStatus = "On" Then

                            ImgZwaveLight(TileIndex).Source = New BitmapImage(New Uri("ms-appx:///Images/lightbulb-on.png", UriKind.Absolute))
                            Powerconsumption(TileIndex).Text = AppNode.PowerConsumptionTekst

                            ' Light on but no timer active?
                            If LightOnTimer.IsEnabled = False Then
                                StartLightOnTimer(5)
                            End If

                            ' Timer active?, show the remaining time
                            If Not LightOnStartTime = Date.MinValue Then
                                Dim tijd As TimeSpan = DateTime.Now.Subtract(LightOnStartTime)
                                If LightOnTimer.Interval.TotalMinutes - tijd.Minutes = 1 Then
                                    MinutesLeft(TileIndex).Text = (LightOnTimer.Interval.TotalMinutes - tijd.Minutes).ToString & " " & ResourceLoader.GetForCurrentView.GetString("minute left")
                                Else
                                    MinutesLeft(TileIndex).Text = (LightOnTimer.Interval.TotalMinutes - tijd.Minutes).ToString & " " & ResourceLoader.GetForCurrentView.GetString("minutes left")
                                End If
                            End If
                        Else
                            ImgZwaveLight(TileIndex).Source = New BitmapImage(New Uri("ms-appx:///Images/lightbulb-off.png", UriKind.Absolute))
                            Powerconsumption(TileIndex).Text = ""
                            MinutesLeft(TileIndex).Text = ""
                        End If
                    End If
                End If
            Next

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ShowLightStatus:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub ShowUltraviolet()

        ' ==================
        ' Update ultravialet
        ' ==================

        Try
            TileUVValue.Text = Math.Round(Ultraviolet.Waarde, 1).ToString & ""
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ShowUltraviolet:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub ShowMotion()

        ' ============================
        ' Update time of last motion
        ' ============================

        Try
            If Motion.Waarde = 1 Then

                Dim MinuteString As String = Motion.TimeMeasured.minute.ToString
                If MinuteString.Length = 1 Then MinuteString = "0" & MinuteString

                TileMotionValue.Text = Motion.TimeMeasured.hour.ToString & ":" & MinuteString
                TileMotionDate.Text = Motion.TimeMeasured.day.ToString & "-" & Motion.TimeMeasured.month.ToString & "-" & Motion.TimeMeasured.year.ToString

                ' Check if the lights will have to be switched on or off
                LightOnOff()

            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ShowMotion:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub ShowVibration()

        ' =============================
        ' Update time of last vibration
        ' =============================

        Try
            If Vibration.Waarde = 1 Then

                Dim MinuteString As String = Motion.TimeMeasured.minute.ToString
                If MinuteString.Length = 1 Then MinuteString = "0" & MinuteString

                TileVibrationValue.Text = Vibration.TimeMeasured.hour.ToString & ":" & MinuteString
                TileVibrationDate.Text = Vibration.TimeMeasured.day.ToString & "-" & Vibration.TimeMeasured.month.ToString & "-" & Vibration.TimeMeasured.year.ToString
            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ShowVibration:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub ShowBattery()

        If Not MultisensorBatterylevel Is Nothing Then
            TileBatteryValue.Text = MultisensorBatterylevel.Waarde & "%"

            Select Case MultisensorBatterylevel.Waarde
                Case 71 To 101
                    ImgBattery.Source = New BitmapImage(New Uri("ms-appx:///Images/battery100%.png", UriKind.Absolute))
                Case 31 To 70
                    ImgBattery.Source = New BitmapImage(New Uri("ms-appx:///Images/battery70%.png", UriKind.Absolute))
                Case 11 To 30
                    ImgBattery.Source = New BitmapImage(New Uri("ms-appx:///Images/battery30%.png", UriKind.Absolute))
                Case Else
                    ImgBattery.Source = New BitmapImage(New Uri("ms-appx:///Images/battery0%.png", UriKind.Absolute))
            End Select

        End If

    End Sub

    Protected Sub LightOnOff()

        Try
            ' Luminance below low-light level and measurement-received time not low?
            If Luminance.Waarde <= AppSettings.AppLowLuxLevel And Not Luminance.TimeMeasured = Date.MinValue Then

                ' Is the light off?
                If LightOnTimer.IsEnabled = False Then

                    ' Has the light not been on before?
                    If LightOffTimer.IsEnabled = False Then
                        Try
                            For Each AppNode As AppNode In AppNodes
                                If AppNode.MeterType = MeterType.Electric Then
                                    If AppNode.NodeStatus = "Off" Then
                                        SendService("Log", AppNode.NodeID, "String", "Switching lights on")
                                        Task.Run(Sub() SwitchLights(AppNode, True))

                                        ' Start the light-on timer (has to be done here because of threadsave reasons)
                                        StartLightOnTimer(5)
                                    End If
                                End If
                            Next
                        Catch ex As Exception
                            SendService("LightOnOff", 0, "String", "Error: " & ex.ToString)
                        End Try
                    End If
                End If
            End If

            ' Light on because motion is triggered?
            If Motion.Waarde = 1 Then
                ' Luminance below low-light level?
                If Luminance.Waarde < AppSettings.AppLowLuxLevel Then
                    Try
                        For Each AppNode As AppNode In AppNodes
                            If AppNode.MeterType = MeterType.Electric Then
                                If AppNode.NodeStatus = "Off" Then
                                    SendService("Log", AppNode.NodeID, "String", "Switching lights on")
                                    Task.Run(Sub() SwitchLights(AppNode, True))

                                    ' Start the light-on timer (has to be done here because of threadsave reasons)
                                    StartLightOnTimer(5)
                                End If
                            End If
                        Next
                    Catch ex As Exception
                        SendService("Switch", 0, "String", "Error: " & ex.ToString)
                    End Try
                End If
            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine LightOnOff:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub StartLightOnTimer(ByVal Minutes As Integer)

        Try
            ' Start the timer
            If LightOnTimer.IsEnabled Then
                LightOnTimer.Stop()
                RemoveHandler LightOnTimer.Tick, AddressOf LightOnTimer_Tick
            End If

            LightOnTimer.Interval = TimeSpan.FromMinutes(Minutes)
            AddHandler LightOnTimer.Tick, AddressOf LightOnTimer_Tick
            LightOnTimer.Start()

            LightOnStartTime = DateTime.Now()
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine StartLightOnTimer:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub LightOnTimer_Tick()

        ' ===============================
        ' Endtime of lightontimer reached
        ' ===============================

        Try
            SendLog("Log", "String", "Endtime of LightOnTimer reached")

            ' Luminance still low and motion triggered?, leave the light on and start the timer again
            If Luminance.Waarde <= AppSettings.AppLowLuxLevel And Motion.Waarde = 1 Then
                StartLightOnTimer(5)
            End If

            ' No motion triggered or luminance above low-light level?, turn of the lights
            If Luminance.Waarde > AppSettings.AppLowLuxLevel Or Motion.Waarde = 0 Then

                For Each AppNode As AppNode In AppNodes
                    If AppNode.MeterType = MeterType.Electric Then
                        If AppNode.NodeStatus = "On" Then
                            Task.Run(Sub() SwitchLights(AppNode, False, True))
                        End If
                    End If
                Next

                ' Stop the LightOnTimer
                LightOnTimer.Stop()
                LightOnStartTime = Date.MinValue

                ' Start the LightOffTimer
                LightOffTimer.Interval = TimeSpan.FromHours(10)
                AddHandler LightOffTimer.Tick, AddressOf LightOffTimer_Tick
                LightOffTimer.Start()

            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine LightOnTimer_Tick:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub LightOffTimer_Tick()

        ' ================================
        ' Endtime of lightofftimer reached
        ' ================================

        SendLog("Log", "String", "Endtime of LightOffTimer reached")

        LightOffTimer.Stop()
        RemoveHandler LightOffTimer.Tick, AddressOf LightOffTimer_Tick

    End Sub

    Protected Async Sub SwitchLights(ByVal AppNode As AppNode, ByVal OnOff As Boolean, Optional ByVal ManualOverride As Boolean = False)

        ' ====================================================
        ' Switch the light on or off
        ' OnOff = 0 : off
        ' OnOff = 1 : on
        ' ManualOverride = 1: Manual switch via touchscreen
        ' ====================================================

        ZwaveNode = ZwaveNodes(AppNode.NodeID)
        SwitchBinary = ZwaveNode.GetCommandClass(Of SwitchBinary)

        Try
            If AppSettings.AppUseZwave = 1 Then
                If Not AppNode Is Nothing Then
                    If ManualOverride = True Or (ManualOverride = False And Luminance.Waarde < AppSettings.AppLowLuxLevel) Then

                        Try

                            If OnOff = False Then
                                ' Light is on and will be switched off
                                If AppNode.NodeStatus = "On" Then
                                    Try
                                        Await SwitchBinary.Set(False)
                                    Catch AggregateEx As AggregateException
                                        SendLog("Log", "String", "Error while switching off light: " & AggregateEx.Message)
                                    Catch ex As Exception
                                        SendLog("Log", "String", "Error while switching off light: " & ex.Message)
                                    End Try
                                End If
                            Else
                                ' Light is off and will be switched on
                                If AppNode.NodeStatus = "Off" Then
                                    Try
                                        Await SwitchBinary.Set(True)
                                    Catch AggregateEx As AggregateException
                                        SendLog("Log", "String", "Error while switching off light: " & AggregateEx.Message)
                                    Catch ex As Exception
                                        SendLog("Log", "String", "Error while switching on light: " & ex.Message)
                                    End Try
                                End If
                            End If

                        Catch ex As Exception
                            SendLog("Log", "String", "Error while switching the light on/off: " & ex.Message.ToString)
                        End Try

                    End If

                    ' Retrieve the new state of the switch
                    Await GetSwitchBinary(AppNode)

                End If
            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine SwitchLight for node " * AppNode.NodeID.ToString & ":" & ex.Message.ToString)
        End Try

    End Sub
#End Region

#Region "Z-Wave"

    Protected Sub OpenController()

        ' =================================
        ' Open the Aeotec Z-Wave controller
        ' =================================
        ' Attention: This function must run in a seperate thread and has to be called by using task.run: Await Task.Run(Sub() OpenController())

        Try
            Controller = New ZWaveController(AppSettings.AppVid, AppSettings.AppPid)
            Controller.Open()
            SendLog("Log", "String", "Controller is open")
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine OpenController:" & ex.Message.ToString)
        End Try

    End Sub

    Protected Async Function GetNodes() As Task

        ' =============================================
        ' Retrieve all Z-wave nodes from the controller
        ' =============================================

        If Controller Is Nothing Then
            Await Task.Run(Sub() OpenController())
        End If

        If Not Controller Is Nothing Then

            Await Writelog("GetNodes()")

            Try
                ZwaveNodes = Await Controller.GetNodes()
                SendLog("Log", "String", ZwaveNodes.Count.ToString & " Nodes retrieved")
            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetNodes:" & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Sub InitAppNodeList()

        ' ==========================================
        ' Initialize the AppSettings.Nodes nodelist
        ' ==========================================

        SendLog("Log", "String", "InitNodeList()")

        ' Reset the timer
        NodeListTimer.Stop()
        NodeListTimer.Interval = TimeSpan.FromMinutes(180)
        NodeListTimer.Start()

        AppNodes = New AppNodes

        If ZwaveNodes Is Nothing Then
            Await GetNodes()
        End If

        If Not ZwaveNodes Is Nothing Then

            Try

                ' Initialize the nodelist
                AppSettings.Nodes = New AppNodes()

                ' Add every Z-wave node to the appnodes list
                For Each ZwaveNode As ZWave.Node In ZwaveNodes

                    Dim AppNode As New AppNode()
                    AppNode.NodeID = ZwaveNode.NodeID

                    Await GetHomeID(AppNode)

                    ' Z-Wave Z-Stick controller
                    If AppNode.NodeID = 1 Then
                        AppNode.ManufacturerID = "0089"
                        AppNode.ProductID = "0001"
                        AppNode.ProductType = "0002"
                        AppNode.NodeType = "Static Controller"
                        AppNode.Image = "images/random.png"
                    End If

                    If AppNode.NodeID > 1 Then

                        Try
                            Dim SupportedCommandClasses = Await ZwaveNode.GetSupportedCommandClasses

                            For i = 1 To SupportedCommandClasses.Count - 1

                                Select Case True

                                ' Switch (Powernode)
                                    Case SupportedCommandClasses(i).Class.ToString.Contains("SwitchBinary")
                                        Await InitSwitchBinary(AppNode)

                                ' Meter (Powernode)
                                    Case SupportedCommandClasses(i).Class.ToString.Contains("Meter")
                                        Await InitMeter(AppNode)

                                ' Multisensor
                                    Case SupportedCommandClasses(i).Class.ToString.Contains("SensorMultiLevel")
                                        Await InitMultiSensor(AppNode)

                                ' Alarm 
                                    Case SupportedCommandClasses(i).Class.ToString.Contains("Alarm")
                                        Await InitAlarm(AppNode)

                                ' Retrieve additional information of the nodes
                                    Case SupportedCommandClasses(i).Class.ToString.Contains("ManufacturerSpecific")
                                        Await GetManufacturer(AppNode)

                                    Case SupportedCommandClasses(i).Class.ToString.Contains("Version")
                                        Await GetVersion(AppNode)

                                    Case SupportedCommandClasses(i).Class.ToString.Contains("Battery")
                                        Await GetBattery(AppNode)
                                End Select
                            Next
                        Catch ex As Exception
                            ' Node is not responding!
                            SendLog("Log", "String", "Error in routine InitAppNodeList for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
                        End Try
                    End If

                    AppNodes.Add(AppNode)

                Next

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine InitAppNodeList:" & ex.Message.ToString)
            End Try

            AppSettings.Nodes = AppNodes

            ' Create the tiles now: we now now the number of switches we have to add
            InitTiles()

            ShowLightStatus()

        End If

    End Sub

    Protected Async Function GetHomeID(ByVal AppNode As AppNode) As Task

        ' =====================================
        ' Retrieve the HomeID of the controller
        ' =====================================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        Try
            AppNode.HomeID = Await ZwaveNode.Controller.GetHomeID()
            SendService("HomeID", AppNode.NodeID.ToString, "String", AppNode.HomeID.ToString)
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetHomeID for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Function

    Protected Async Function GetCommandClasses(ByVal AppNode As AppNode) As Task

        ' ===========================
        ' Retrieve the commandclasses
        ' ===========================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        Try
            Dim SupportedCommandClasses = Await ZwaveNode.GetSupportedCommandClasses
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetCommandClasses for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Function

    Protected Sub GetDeviceInfo(ByVal AppNode As AppNode)

        ' =======================
        ' Retrieve the DeviceInfo
        ' =======================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                Dim DeviceInfo = ZwaveNode.GetType
                SendService("DeviceInfo", AppNode.NodeID, "String", DeviceInfo.ToString)
            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetDeviceInfo for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Sub

    Protected Async Function GetManufacturer(ByVal AppNode As AppNode) As Task

        ' ==============================
        ' Retrieve the manufacturer info
        ' ==============================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                Dim manufacturerSpecific = ZwaveNode.GetCommandClass(Of ManufacturerSpecific)()
                Dim manufacturerSpecificReport = Await manufacturerSpecific.Get()

                SendService("Manufacturer", AppNode.NodeID, "String", manufacturerSpecificReport.ToString)

                AppNode.ManufacturerID = manufacturerSpecificReport.ManufacturerID.ToString
                AppNode.ProductID = manufacturerSpecificReport.ProductID.ToString
                AppNode.ProductType = manufacturerSpecificReport.ProductType.ToString
            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetManufacturer for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Function GetVersion(ByVal AppNode As AppNode) As Task

        ' =========================
        ' Retrieve the version info
        ' =========================

        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                Dim Version = ZwaveNode.GetCommandClass(Of Version)
                Dim VersionReport = Await Version.Get()

                SendService("Version", AppNode.NodeID, "String", VersionReport.ToString)

                AppNode.Application = VersionReport.Application.ToString
                AppNode.Library = VersionReport.Library.ToString
                AppNode.Protocol = VersionReport.Protocol.ToString
            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetVersion for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Function GetProtocolInfo(ByVal AppNode As AppNode, ByVal ZwaveNode As Node) As Task

        ' =========================
        ' Retrieve the ProtocolInfo
        ' =========================

        If Not ZwaveNode Is Nothing Then
            Try
                Dim ProtocolInfo = Await ZwaveNode.GetProtocolInfo
                SendService("ProtocolInfo", AppNode.NodeID, "String", ProtocolInfo.ToString)
            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetProtocolInfo for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Function GetSwitchBinary(ByVal AppNode As AppNode) As Task

        ' ==============================
        ' Retrieve the SwitchBinary info
        ' ==============================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                SwitchBinary = ZwaveNode.GetCommandClass(Of SwitchBinary)
                Dim Result As SwitchBinaryReport = Await SwitchBinary.Get()
                AppNode.NodeStatus = Result.Value.ToString

                Await GetConfiguration(AppNode)

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetSwitchBinary for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Function GetConfiguration(ByVal AppNode As AppNode) As Task

        ' ============================================
        ' Retrieve the SwitchBinary configuration info
        ' ============================================
        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try

                Dim configuration = ZwaveNode.GetCommandClass(Of Configuration)()
                Dim Result As ConfigurationReport = Await configuration.Get(2)
                AppNode.Color = Result.Value

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine GetConfiguration for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Function GetBattery(ByVal Appnode As AppNode) As Task

        Try
            ZwaveNode = ZwaveNodes(Appnode.NodeID)

            If Not ZwaveNode Is Nothing Then
                Dim battery = ZwaveNode.GetCommandClass(Of Battery)()
                Dim batteryReport = Await battery.Get()
                SendService("Battery", Appnode.NodeID, "decimal", batteryReport.Value.ToString)
                TileBatteryValue.Text = batteryReport.Value.ToString & "%"
            End If

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetBattery for node " & Appnode.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Function
#End Region

#Region "switchBinary"

    Protected Async Function InitSwitchBinary(ByVal AppNode As AppNode) As Task

        ' ==========================================
        ' Configure the GreenWave Smartplug NS310-F
        ' ==========================================

        AppNode.NodeType = "switchBinary"

        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                ' Set the 'keep alive' LED value to max (255 minutes)
                ' PowerNode uses LED to indicate the connection status with associated node. 
                ' The LED will flash (ref to topic “indictor” in user guide) if there has being no any frame transmission for a specific time which can be configured. 
                ' Default value is 2 minutes.
                Dim configuration = ZwaveNode.GetCommandClass(Of Configuration)()
                Await configuration.Set(1, CByte(255))

                ' Associate the switch with the Z-Wave controller(Node 1) for the groups 1, 2, 3 and 4
                ' This is necessary to receive events!
                Dim association = ZwaveNode.GetCommandClass(Of Association)()
                Await association.Add(1, 1) ' Wheel selection
                Await association.Add(2, 1) ' Relay health
                Await association.Add(3, 1) ' Power level
                Await association.Add(4, 1) ' Overcurrent protection

                ' Create an event handler to receive events
                Dim switchBinary = ZwaveNode.GetCommandClass(Of SwitchBinary)()
                AddHandler switchBinary.Changed, AddressOf CatchSwitchBinaryEvent

                ' Retrieve the current status
                Await GetSwitchBinary(AppNode)
                Await GetConfiguration(AppNode)

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine InitSwitchBinary for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Async Sub CatchSwitchBinaryEvent(ByVal O As Object, ByVal e As ReportEventArgs(Of SwitchBinaryReport))

        Dim Device As SwitchBinary = CType(O, SwitchBinary)

        SendService("SwitchBinary", Device.Node.NodeID.ToString, "boolean", e.Report.Value.ToString)

        Try
            For Each AppNode As AppNode In AppNodes
                If AppNode.NodeID = e.Report.Node.NodeID Then
                    AppNode.NodeStatus = e.Report.Value.ToString
                    If AppNode.NodeStatus = "On" Then
                        AppNode.Image = "images/lightbulb-on.png"
                    Else
                        AppNode.Image = "images/lightbulb-off.png"
                    End If

                    ' Get the configuration info to determine the color settings of the switch
                    Await GetConfiguration(AppNode)

                End If
            Next
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine CatchSwitchBinaryEvent for node " & e.Report.Node.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Sub

    Protected Async Function InitMeter(ByVal AppNode As AppNode) As Task

        ' =============================
        ' Configure the GreenWave Meter
        ' =============================

        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                ' Create an event handler to receive events
                Dim Meter = ZwaveNode.GetCommandClass(Of Meter)()
                AddHandler Meter.Changed, AddressOf CatchMeterEvent

                Dim MeterSupportedReport = Await Meter.GetSupported()

                AppNode.CanReset = MeterSupportedReport.CanReset.ToString
                Select Case MeterSupportedReport.Type.ToString
                    Case "Electric"
                        AppNode.MeterType = MeterType.Electric
                    Case "Gas"
                        AppNode.MeterType = MeterType.Gas
                    Case "Water"
                        AppNode.MeterType = MeterType.Water
                    Case Else
                        AppNode.MeterType = MeterType.Unknown
                End Select

                AppNode.NodeName = MeterSupportedReport.Type.ToString
                AppNode.Units = MeterSupportedReport.Units.ToString

                SendService("InitMeter", AppNode.NodeID.ToString, "String", MeterSupportedReport.ToString)

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine InitMeter for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Sub CatchMeterEvent(ByVal O As Object, ByVal e As ReportEventArgs(Of MeterReport))

        Dim Device As Meter = CType(O, Meter)

        SendService("Meter", Device.Node.NodeID.ToString, "boolean", e.Report.Value.ToString)

        For Each AppNode As AppNode In AppNodes
            If AppNode.NodeID = Device.Node.NodeID Then

                AppNode.Scale = e.Report.Scale.ToString
                AppNode.PowerConsumption = e.Report.Value.ToString
                AppNode.Unit = e.Report.Unit.ToString

                Exit For
            End If
        Next

    End Sub

    Protected Async Function InitAlarm(ByVal AppNode As AppNode) As Task

        ' ======================================================
        ' Configure the alarm of the GreenWave Smartplug NS310-F
        ' ======================================================

        ZwaveNode = ZwaveNodes(AppNode.NodeID)

        If Not ZwaveNode Is Nothing Then
            Try
                ' Create an event handler to receive events
                Dim Alarm = ZwaveNode.GetCommandClass(Of Alarm)()
                AddHandler Alarm.Changed, AddressOf CatchAlarmEvent

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine InitAlarm for node " & AppNode.NodeID.ToString & ": " & ex.Message.ToString)
            End Try
        End If

    End Function

    Protected Sub CatchAlarmEvent(ByVal O As Object, ByVal e As ReportEventArgs(Of AlarmReport))

        Dim Device As Alarm = CType(O, Alarm)

        SendService("Alarm", Device.Node.NodeID.ToString, "string", e.Report.ToString)

        For Each AppNode As AppNode In AppNodes

            If AppNode.NodeID = Device.Node.NodeID Then
                AppNode.AlarmType = e.Report.Type
                AppNode.AlarmLevel = e.Report.Level
                AppNode.AlarmDetail = e.Report.Detail

                Exit For
            End If
        Next

    End Sub
#End Region

#Region "Multisensor"

    Private Async Function InitMultiSensor(ByVal AppNode As AppNode) As Task

        Try

            AppNode.Image = "images/sensor.png"
            AppNode.NodeType = "MultiSensor"

            ZwaveNode = ZwaveNodes(AppNode.NodeID)

            If Not ZwaveNode Is Nothing Then
                ' Associate the multisensor with the Z-Wave controller(Node 1) for groups 1, 2 en 3
                Dim association = ZwaveNode.GetCommandClass(Of Association)()
                Await association.Add(1, 1)
                Await association.Add(2, 1)
                Await association.Add(3, 1)

                Dim configuration = ZwaveNode.GetCommandClass(Of Configuration)()

                ' Sensors report group (Group 1)
                Await configuration.Set(101, 241)

                ' Interval for reports (3 minutes)
                Await configuration.Set(111, 180)

                ' Interval for PIR (4 minutes)
                Await configuration.Set(3, 2)

                ' Sensitivity for PIR is (maximum)
                Await configuration.Set(4, 5)

                ' Luminance correction (3)
                Await configuration.Set(203, 0)

                MultiSensorEvents(AppNode)

                SendService("Multisensor", AppNode.NodeID, "String", "Multisensor initialised")
            End If

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine InitMultisensor for node " & ZwaveNode.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Function

    Public Async Sub MultiSensorEvents(ByVal Appnode As AppNode)

        ' ============================================================
        ' ' Create an event handlers for all sensors to receive events
        ' ============================================================
        Try
            ZwaveNode = ZwaveNodes(Appnode.NodeID)

            If Not ZwaveNode Is Nothing Then

                Dim device = New Aeon.MultiSensor6(ZwaveNode)

                AddHandler device.TemperatureMeasured, AddressOf CatchTemperatureEvent
                AddHandler device.HumidityMeasured, AddressOf CatchHumidityEvent
                AddHandler device.LuminanceMeasured, AddressOf CatchLuminanceEvent
                AddHandler device.UltravioletMeasured, AddressOf CatchUltravioletEvent
                AddHandler device.MotionDetected, AddressOf CatchMotionDetectedEvent
                AddHandler device.MotionCancelled, AddressOf CatchtMotionCancelledEvent
                AddHandler device.VibrationDetected, AddressOf CatchVibrationEvent

                Dim wakeUp = ZwaveNode.GetCommandClass(Of WakeUp)()
                Await wakeUp.SetInterval(TimeSpan.FromMinutes(3), &H1)

                AddHandler wakeUp.Changed, AddressOf CatchWakeUpEvent

                Dim Batterylevel As Measure = Await device.GetBatteryLevel()
                MultisensorBatterylevel.Waarde = Batterylevel.Value.ToString

            End If

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine SubscribeToMultisensor for node " & ZwaveNode.NodeID.ToString & ": " & ex.Message.ToString)
        End Try

    End Sub

    Protected Sub CatchTemperatureEvent(ByVal O As Object, ByVal e As MeasureEventArgs)

        ' ===========================
        ' Catch the temperature event
        ' ===========================
        Temperature.Waarde = e.Meassure.Value + AppSettings.AppTemperatureCorrection
        Temperature.TimeMeasured = DateTime.Now

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Temperature", Device.Node.NodeID.ToString, "decimal", Math.Round(Temperature.Waarde, 1).ToString)

        ' Store the highest value of each hour to populate the diagram
        If Temperature.Waarde > Temperature.MeasurementPerHour(Date.Now.Hour) Then
            Temperature.MeasurementPerHour(Date.Now.Hour) = Temperature.Waarde
        End If

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowTemperature(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchHumidityEvent(ByVal O As Object, ByVal e As MeasureEventArgs)

        ' ===========================
        ' Catch the humidity event
        ' ===========================

        Humidity.Waarde = e.Meassure.Value
        Humidity.TimeMeasured = DateTime.Now

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Humidity", Device.Node.NodeID.ToString, "decimal", CInt(Humidity.Waarde))

        ' Store the highest value of each hour to populate the diagram
        If Humidity.Waarde > Humidity.MeasurementPerHour(Date.Now.Hour) Then
            Humidity.MeasurementPerHour(Date.Now.Hour) = Humidity.Waarde
        End If

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowHumidity(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchLuminanceEvent(ByVal O As Object, ByVal e As MeasureEventArgs)

        ' ===========================
        ' Catch the luminance event
        ' ===========================

        Luminance.Waarde = e.Meassure.Value
        Luminance.TimeMeasured = DateTime.Now

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Luminance", Device.Node.NodeID.ToString, "decimal", CInt(Luminance.Waarde))

        ' Store the highest value of each hour to populate the diagram
        If Luminance.Waarde > Luminance.MeasurementPerHour(Date.Now.Hour) Then
            Luminance.MeasurementPerHour(Date.Now.Hour) = Luminance.Waarde
        End If

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowLuminance(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchUltravioletEvent(ByVal O As Object, ByVal e As MeasureEventArgs)

        ' ===========================
        ' Catch the uv event
        ' ===========================

        Ultraviolet.Waarde = e.Meassure.Value

        If e.Meassure.Value <> 0 Then
            Ultraviolet.TimeMeasured = DateTime.Now

            Dim Device As MultiSensor6 = CType(O, MultiSensor6)
            SendService("UV", Device.Node.NodeID.ToString, "decimal", CInt(Ultraviolet.Waarde))

            ' Store the highest value of each hour to populate the diagram
            If Ultraviolet.Waarde > Ultraviolet.MeasurementPerHour(Date.Now.Hour) Then
                Ultraviolet.MeasurementPerHour(Date.Now.Hour) = Ultraviolet.Waarde
            End If
        End If

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowUltraviolet(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchMotionDetectedEvent(ByVal O As Object, ByVal e As EventArgs)

        ' =============================
        ' Catch the motion detect event
        ' =============================

        Motion.Waarde = 1
        Motion.TimeMeasured = DateTime.Now

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Motion detect", Device.Node.NodeID.ToString, "datetime", Motion.TimeMeasured)

        ' Store the motion for each hour to populate the diagram
        Motion.MeasurementPerHour(Date.Now.Hour) = Motion.Waarde

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowMotion(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchtMotionCancelledEvent(ByVal O As Object, ByVal e As EventArgs)

        ' ================================
        ' Catch the motion cancelled event
        ' ================================

        Motion.Waarde = 0
        Motion.TimeMeasured = DateTime.MinValue

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Motion cancelled", Device.Node.NodeID.ToString, "datetime", DateTime.Now)

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowMotion(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchVibrationEvent(ByVal O As Object, ByVal e As EventArgs)

        ' ===========================
        ' Catch the vibration event
        ' ===========================

        Vibration.Waarde = 1
        Vibration.TimeMeasured = DateTime.Now

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("Vibration", Device.Node.NodeID.ToString, "decimal", Vibration.TimeMeasured)

        ' Store the highest value of each hour to populate the diagram
        Vibration.MeasurementPerHour(Date.Now.Hour) = Vibration.Waarde

        ' Show the measured value on the screen on the UI thread
        Task.Factory.StartNew(Sub() ShowVibration(), New System.Threading.CancellationToken(), TaskCreationOptions.PreferFairness, UISyncContext)

    End Sub

    Protected Sub CatchWakeUpEvent(ByVal O As Object, ByVal e As ReportEventArgs(Of WakeUpReport))

        ' ===========================
        ' Catch the wakeup event
        ' ===========================

        Dim Device As MultiSensor6 = CType(O, MultiSensor6)
        SendService("WakeUp", Device.Node.NodeID.ToString, "boolean", DateTime.Now)

    End Sub

    Protected Sub CatchBatteryEvent(ByVal O As Object, ByVal e As MeasureEventArgs)

        ' ===========================
        ' Catch the battery event
        ' ===========================

        'ToDo

    End Sub
#End Region

#Region "Weather"
    Protected Async Sub GetWeather()

        Try
            Dim httpClient = New HttpClient()
            ' FILL YOUR OWN APPID IN THE CONNECTION STRING
            Dim httpResponse = Await httpClient.GetAsync("http://api.openweathermap.org/data/2.5/weather?q=Harlingen,nl&units=metric&mode=xml&lang=nl&appid=fill-your-own-appid-here")
            httpResponse.EnsureSuccessStatusCode()
            Dim httpResponseBody = Await httpResponse.Content.ReadAsStringAsync()

            Dim Icon As String = ""
            Dim Weather As String = ""
            Dim Temperature As String = ""
            Dim Humidity As String = ""
            Dim Pressure As String = ""

            Using reader As XmlReader = XmlReader.Create(New StringReader(httpResponseBody))

                ' Search the XML for the needed values
                While reader.Read()
                    Select Case reader.Name
                        Case "temperature"
                            If Temperature = "" Then Temperature = reader.GetAttribute("value")
                        Case "humidity"
                            If Humidity = "" Then Humidity = reader.GetAttribute("value")
                        Case "pressure"
                            If Pressure = "" Then Pressure = reader.GetAttribute("value")
                        Case "weather"
                            If Icon = "" Then Icon = reader.GetAttribute("icon") & ".png"
                            If Weather = "" Then Weather = reader.GetAttribute("value")
                    End Select
                End While
            End Using

            ' Round to 1 position behind the comma
            Dim TempDec As Decimal = CDec(Temperature.Replace(".", ","))
            TempDec = Math.Round(TempDec, 1)
            If TempDec = CInt(TempDec) Then
                TileTemperatureOutsideValue.Text = CInt(TempDec).ToString & "°C"
            Else
                TileTemperatureOutsideValue.Text = TempDec.ToString & "°C"
            End If

            TileHumidityOutsideValue.Text = Humidity & "%"
            TilePressureValue.Text = Pressure
            ImgWeather.Source = New BitmapImage(New Uri("ms-appx:///images/" & Icon, UriKind.Absolute))
            If Weather.Length > 18 Then
                Weather = Weather.Substring(1, 15) & "..."
            End If
            WeatherText.Text = Weather

            ' Store the highest value of each hour to populate the diagram
            If TempDec > OutdoorTemperature.MeasurementPerHour(Date.Now.Hour) Then
                OutdoorTemperature.MeasurementPerHour(Date.Now.Hour) = TempDec
            End If

            ' Store the highest value of each hour to populate the diagram
            If Humidity > OutdoorHumidity.MeasurementPerHour(Date.Now.Hour) Then
                OutdoorHumidity.MeasurementPerHour(Date.Now.Hour) = Humidity
            End If

            ' Store the highest value of each hour to populate the diagram
            If Pressure > OutdoorPressure.MeasurementPerHour(Date.Now.Hour) Then
                OutdoorPressure.MeasurementPerHour(Date.Now.Hour) = Pressure
            End If

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine GetWeather: " & ex.Message.ToString)
        End Try

    End Sub

#End Region

#Region "Webservice"
    Protected Sub SendService(Sensor As String, NodeID As Integer, Datatype As String, Waarde As String)

        ' If you need to send the measurement-data to a webservice, do it here....

    End Sub

    Protected Async Sub SendLog(Sensor As String, Datatype As String, Waarde As String)

        Await Writelog(Waarde)

    End Sub

#End Region

#Region "Logging"
    Protected Async Sub OpenLogFile()

        ' ===================
        ' Open the logfile
        ' ===================

        Try
            LogFolder = Windows.Storage.ApplicationData.Current.LocalFolder
            LogFile = Await LogFolder.CreateFileAsync("log-" & Date.Now.ToString("yyyyMMdd") & ".txt", CreationCollisionOption.OpenIfExists)
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine OpenLogFile: " & ex.Message.ToString)
        End Try

    End Sub

    Protected Async Function Writelog(ByVal Message As String) As Task

        ' ======================
        ' Write the logfile
        ' ======================

        If Log = True Then

            Try
                If LogFolder Is Nothing Then
                    OpenLogFile()
                End If

                Message = Message.Replace("""", "")
                Await Windows.Storage.FileIO.AppendTextAsync(LogFile, DateTime.Now.ToString & " " & Message & vbCrLf)

            Catch ex As Exception
                SendLog("Log", "String", "Error in routine WriteLog: " & ex.Message.ToString)
            End Try

        End If

    End Function
#End Region

#Region "Navigation"
    Private Sub HamburgerButton_Click(sender As Object, e As RoutedEventArgs)

        ' =============
        ' Open the menu
        ' =============

        MySplitView.IsPaneOpen = Not MySplitView.IsPaneOpen

    End Sub

    Private Sub Menu_Settings_Click(sender As Object, e As RoutedEventArgs) Handles Menu_Settings.Click

        ' ======================
        ' Navigate to Settings
        ' ======================

        Try
            ControllerDispose()
            Frame.Navigate(GetType(Settings), AppSettings)
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Menu_Settings_Click: " & ex.Message.ToString)
        End Try

    End Sub

    Private Sub Menu_DeviceInfo_Click(sender As Object, e As RoutedEventArgs) Handles Menu_DeviceInfo.Click

        ' ========================
        ' Navigate to DeviceInfo
        ' ========================

        Try
            ControllerDispose()
            Frame.Navigate(GetType(DeviceInfo), AppSettings)
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Menu_DeviceInfo_Click: " & ex.Message.ToString)
        End Try

    End Sub

    Private Sub Menu_Restart_Click(sender As Object, e As RoutedEventArgs) Handles Menu_Restart.Click

        ' ===================
        ' Restart the program
        ' ===================

        Try
            ControllerDispose()

            Task.Run(Sub() ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(2))).Start()
            Application.Current.Exit()

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Menu_Restart_Click: " & ex.Message.ToString)
        End Try

    End Sub

    Private Sub Menu_CloseDown_Click(sender As Object, e As RoutedEventArgs) Handles Menu_CloseDown.Click

        ' ===================
        ' Stop the program
        ' ===================

        Try
            ControllerDispose()

            Task.Run(Sub() ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(2))).Start()
            Application.Current.Exit()

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Menu_CloseDown_Click: " & ex.Message.ToString)
        End Try

    End Sub
#End Region

#Region "CloseDown"

    Private Sub ControllerDispose()

        ' ====================
        ' Close the controller
        ' ====================
        Try
            If Not Controller Is Nothing Then
                Controller.Close()
                SendLog("Log", "String", "Controller is closed")
            End If
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ControllerDispose: " & ex.Message.ToString)
        End Try

    End Sub

    Private Sub Suspending(sender As Object, e As SuspendingEventArgs)

        ' ==================
        ' Program is closing
        ' ==================

        Try
            SendLog("Log", "String", "Suspending()")

            Dim deferral = e.SuspendingOperation.GetDeferral()
            ControllerDispose()
            deferral.Complete()
        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Suspending: " & ex.Message.ToString)
        End Try

    End Sub

    Private Async Sub Item_Click(sender As Object, e As ItemClickEventArgs)

        ' ========================
        ' Item click event handler
        ' ========================

        Try
            If Not e.ClickedItem Is Nothing Then

                If e.ClickedItem.name = "PnlTemperature" Then
                    ShowGraph("Temperature")
                End If

                If e.ClickedItem.name = "PnlHumidity" Then
                    ShowGraph("Humidity")
                End If

                If e.ClickedItem.name = "PnlLuminance" Then
                    ShowGraph("Luminance")
                End If

                If e.ClickedItem.name = "PnlUltraviolet" Then
                    ShowGraph("Ultraviolet")
                End If

                If e.ClickedItem.name = "PnlMotion" Then
                    ShowGraph("Motion")
                End If

                If e.ClickedItem.name = "PnlVibration" Then
                    ShowGraph("Vibration")
                End If

                If e.ClickedItem.name = "PnlTemperatureOutside" Then
                    ShowGraph("OutdoorTemperature")
                End If

                If e.ClickedItem.name = "PnlHumidityOutside" Then
                    ShowGraph("OutdoorHumidity")
                End If

                If e.ClickedItem.name = "PnlPressure" Then
                    ShowGraph("Pressure")
                End If

                If e.ClickedItem.name.ToString.Length > 13 Then
                    If e.ClickedItem.name.ToString.Substring(0, 13) = "PnlZwaveLight" Then
                        Dim ClickedNodeID = e.ClickedItem.name.ToString.Substring(13, e.ClickedItem.name.ToString.Length - 13)

                        For Each AppNode As AppNode In AppNodes
                            If AppNode.NodeID = ClickedNodeID Then
                                If AppNode.NodeStatus = "Off" Then
                                    AppNode.NodeStatus = "On"
                                    Await Task.Run(Sub() SwitchLights(AppNode, True, True))
                                    StartLightOnTimer(5)
                                Else
                                    Await Task.Run(Sub() SwitchLights(AppNode, False, True))
                                    AppNode.NodeStatus = "Off"
                                    LightOnStartTime = Date.MinValue
                                    LightOnTimer.Stop()
                                End If
                            End If
                        Next
                        ShowLightStatus()
                    End If
                End If

                ' Get weather info
                If e.ClickedItem.name = "PnlWeather" Then
                    TwoMinuteTimer_Tick()
                End If

                ' Toggle presence
                If e.ClickedItem.name = "PnlPresence" Then
                    TogglePresence()
                End If

            End If

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine Item_Click: " & ex.Message.ToString)
        End Try

    End Sub
#End Region

#Region "Graph"
    Private Sub ShowGraph(ByVal Sensor As String)

        ' ==========
        ' Show chart
        ' ==========

        Try
            TxtGraph.Text = ResourceLoader.GetForCurrentView.GetString(Sensor)

            Dim valueList As New List(Of KeyValuePair(Of String, Integer))()
            For i = 0 To 23
                Select Case Sensor
                    Case "Temperature"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Temperature.MeasurementPerHour(i)))
                    Case "Humidity"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Humidity.MeasurementPerHour(i)))
                    Case "Luminance"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Luminance.MeasurementPerHour(i)))
                    Case "Ultraviolet"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Ultraviolet.MeasurementPerHour(i)))
                    Case "Motion"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Motion.MeasurementPerHour(i)))
                    Case "Vibration"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, Vibration.MeasurementPerHour(i)))
                    Case "OutdoorTemperature"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, OutdoorTemperature.MeasurementPerHour(i)))
                    Case "OutdoorHumidity"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, OutdoorHumidity.MeasurementPerHour(i)))
                    Case "Pressure"
                        valueList.Add(New KeyValuePair(Of String, Integer)(i, OutdoorPressure.MeasurementPerHour(i)))
                End Select
            Next

            LineGraph.Series.Clear()

            Dim LineSeries1 As AreaSeries = New AreaSeries()

            Select Case Sensor
                Case "Temperature", "OutdoorTemperature"
                    LineSeries1.Title = "°C"
                Case "Humidity", "OutdoorHumidity"
                    LineSeries1.Title = "%"
                Case "Luminance"
                    LineSeries1.Title = "Lux"
                Case "Ultraviolet"
                    LineSeries1.Title = ""
                Case "Pressure"
                    LineSeries1.Title = "hPa"
            End Select

            LineSeries1.DependentValuePath = "Value"
            LineSeries1.IndependentValuePath = "Key"
            LineSeries1.ItemsSource = valueList
            LineGraph.Series.Add(LineSeries1)
            PopupGraph.IsOpen = True

        Catch ex As Exception
            SendLog("Log", "String", "Error in routine ShowGraph: " & ex.Message.ToString)
        End Try

    End Sub

    Private Sub HideGraph()

        ' ==========
        ' Hide graph
        ' ==========

        PopupGraph.IsOpen = False
    End Sub

    Private Sub TogglePresence()

        ' ========================
        ' Toggle and show presence
        ' ========================

        If Presence.Waarde = 1 Then
            ImgPresence.Source = New BitmapImage(New Uri("ms-appx:///Images/out.png", UriKind.Absolute))
            Presence.Waarde = 0
        Else
            ImgPresence.Source = New BitmapImage(New Uri("ms-appx:///Images/in.png", UriKind.Absolute))
            Presence.Waarde = 1
        End If
        Presence.TimeMeasured = DateTime.Now

        SendService("Presence", 0, "Boolean", Presence.Waarde.ToString)

    End Sub

#End Region
End Class
