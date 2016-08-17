' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Imports Windows.Devices.I2C
Imports GHIElectronics.UWP.Shields
Imports Windows.Globalization
Imports Windows.Devices.WiFi
Imports Windows.Security.Credentials

Public NotInheritable Class Settings
    Inherits Page

#Region "Declaraties"

    ' Device info
    Dim Device As Device

    ' Network info
    Dim Network As Network

    ' Settings
    Dim AppSettings As AppSettings

    'WiFi
    Dim WifiAdapter As WiFiAdapter
    Dim WifiNetwork As WiFiAvailableNetwork
#End Region

#Region "Init"
    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)

        ' =============
        ' Program start
        ' =============
        MyBase.OnNavigatedTo(e)
        Me.InitializeComponent()

        Init(e)

    End Sub

    Private Sub Init(e As NavigationEventArgs)

        AppSettings = New AppSettings
        AppSettings = e.Parameter

        ApplicationLanguages.PrimaryLanguageOverride = AppSettings.AppLanguage

        For Each Item As ComboBoxItem In ddl_interval.Items
            If Item.Tag = AppSettings.AppMeasurementInterval.ToString Then
                ddl_interval.SelectedItem = Item
            End If
        Next

        If AppSettings.AppSendMessage = 1 Then
            SendDataToggle.IsOn = True
        Else
            SendDataToggle.IsOn = False
        End If

        For Each Item As ComboBoxItem In ddl_sendmessage_interval.Items
            If Item.Tag = AppSettings.AppSendMessageInterval.ToString Then
                ddl_sendmessage_interval.SelectedItem = Item
            End If
        Next

        For Each Item As ComboBoxItem In ddl_temperature_correction.Items
            If Item.Tag = AppSettings.AppTemperatureCorrection.ToString Then
                ddl_temperature_correction.SelectedItem = Item
            End If
        Next

        If AppSettings.AppUseZwave = 1 Then
            ZwaveToggle.IsOn = True
        Else
            ZwaveToggle.IsOn = False
        End If

        For Each Item As ComboBoxItem In ddl_language.Items
            If Item.Tag = AppSettings.AppLanguage.ToString Then
                ddl_language.SelectedItem = Item
            End If
        Next

        txt_vid.Text = AppSettings.AppVid
        txt_pid.Text = AppSettings.AppPid

        For Each Item As ComboBoxItem In ddl_LowLuxLevel.Items
            If Item.Tag = AppSettings.AppLowLuxLevel.ToString Then
                ddl_LowLuxLevel.SelectedItem = Item
            End If
        Next

        If AppSettings.TileTemperature = True Then
            TileTemperature.IsOn = True
        Else
            TileTemperature.IsOn = False
        End If

        If AppSettings.TileHumidity = True Then
            TileHumidity.IsOn = True
        Else
            TileHumidity.IsOn = False
        End If

        If AppSettings.TileLuminance = True Then
            TileLuminance.IsOn = True
        Else
            TileLuminance.IsOn = False
        End If

        If AppSettings.TileUltraviolet = True Then
            TileUV.IsOn = True
        Else
            TileUV.IsOn = False
        End If

        If AppSettings.TileMotion = True Then
            TileMotion.IsOn = True
        Else
            TileMotion.IsOn = False
        End If

        If AppSettings.TileVibration = True Then
            TileVibration.IsOn = True
        Else
            TileVibration.IsOn = False
        End If

        If AppSettings.TileBattery = True Then
            TileBattery.IsOn = True
        Else
            TileBattery.IsOn = False
        End If

        If AppSettings.TileZwaveLight = True Then
            TileZwaveLight.IsOn = True
        Else
            TileZwaveLight.IsOn = False
        End If

        If AppSettings.TileWeather = True Then
            TileWeather.IsOn = True
        Else
            TileWeather.IsOn = False
        End If

        If AppSettings.TileClock = True Then
            TileClock.IsOn = True
        Else
            TileClock.IsOn = False
        End If

        If AppSettings.TilePresence = True Then
            TilePresence.IsOn = True
        Else
            TilePresence.IsOn = False
        End If

        InitWifi()

    End Sub

    Private Async Sub InitWifi()

        ' Controleer of er autorisatie is
        Dim WiFiAccess = Await WiFiAdapter.RequestAccessAsync()
        If WiFiAccess = WiFiAccessStatus.Allowed Then

            ' Haal de beschikbare WiFi adapters op
            Dim WifiList = Await WiFiAdapter.FindAllAdaptersAsync()

            If WifiList.Count > 0 Then

                ' Selecteer de eerste adapter
                WifiAdapter = WifiList(0)

                ' Zoek de WiFi verbindingen op deze adapter
                Await WifiAdapter.ScanAsync()

                ' Voeg de WiFi netwerken toe aan de selectielijst
                For Each WifiNetwork In WifiAdapter.NetworkReport.AvailableNetworks
                    ddl_wifi.Items.Add(WifiNetwork.Ssid.ToString)
                Next
            End If
        End If

        ' Maak het huidige netwerk selected
        For Each Wifi_Item In ddl_wifi.Items
            If Wifi_Item = AppSettings.Network.network Then
                ddl_wifi.SelectedItem = Wifi_Item
            End If
        Next
    End Sub


#End Region

#Region "Navigatie"
    Private Sub HamburgerButton_Click(sender As Object, e As RoutedEventArgs)

        MySplitView.IsPaneOpen = Not MySplitView.IsPaneOpen

    End Sub

    Private Sub Menu_Sensors_Click(sender As Object, e As RoutedEventArgs) Handles Menu_Sensors.Click

        UpdateAppsetting()

        Frame.Navigate(GetType(MainPage), AppSettings)

    End Sub

    Private Sub Menu_DeviceInfo_Click(sender As Object, e As RoutedEventArgs) Handles Menu_DeviceInfo.Click

        UpdateAppsetting()

        Frame.Navigate(GetType(DeviceInfo), AppSettings)

    End Sub

#End Region

#Region "CloseDown"
    Private Sub UpdateAppsetting()

        AppSettings.AppMeasurementInterval = ddl_interval.SelectedItem.Tag

        If SendDataToggle.IsOn = True Then
            AppSettings.AppSendMessage = 1
        Else
            AppSettings.AppSendMessage = 0
        End If

        AppSettings.AppSendMessageInterval = ddl_sendmessage_interval.SelectedItem.Tag
        AppSettings.AppTemperatureCorrection = ddl_temperature_correction.SelectedItem.Tag

        If ZwaveToggle.IsOn = True Then
            AppSettings.AppUseZwave = 1
        Else
            AppSettings.AppUseZwave = 0
        End If

        AppSettings.AppVid = txt_vid.Text
        AppSettings.AppPid = txt_pid.Text
        AppSettings.AppLowLuxLevel = ddl_LowLuxLevel.SelectedItem.tag

        AppSettings.AppLanguage = ddl_language.SelectedItem.tag

        If TileTemperature.IsOn = True Then
            AppSettings.TileTemperature = 1
        Else
            AppSettings.TileTemperature = 0
        End If

        If TileHumidity.IsOn = True Then
            AppSettings.TileHumidity = 1
        Else
            AppSettings.TileHumidity = 0
        End If

        If TileLuminance.IsOn = True Then
            AppSettings.TileLuminance = 1
        Else
            AppSettings.TileLuminance = 0
        End If

        If TileUV.IsOn = True Then
            AppSettings.TileUltraviolet = 1
        Else
            AppSettings.TileUltraviolet = 0
        End If

        If TileMotion.IsOn = True Then
            AppSettings.TileMotion = 1
        Else
            AppSettings.TileMotion = 0
        End If

        If TileVibration.IsOn = True Then
            AppSettings.TileVibration = 1
        Else
            AppSettings.TileVibration = 0
        End If

        If TileBattery.IsOn = True Then
            AppSettings.TileBattery = 1
        Else
            AppSettings.TileBattery = 0
        End If

        If TileZwaveLight.IsOn = True Then
            AppSettings.TileZwaveLight = 1
        Else
            AppSettings.TileZwaveLight = 0
        End If

        If TileWeather.IsOn = True Then
            AppSettings.TileWeather = 1
        Else
            AppSettings.TileWeather = 0
        End If

        If TileClock.IsOn = True Then
            AppSettings.TileClock = 1
        Else
            AppSettings.TileClock = 0
        End If

        If TilePresence.IsOn = True Then
            AppSettings.TilePresence = 1
        Else
            AppSettings.TilePresence = 0
        End If

        WiFiLogon()

    End Sub

    Private Async Sub WiFiLogon()

        For Each WifiNetwork In WifiAdapter.NetworkReport.AvailableNetworks

            If WifiNetwork.Ssid.ToString = ddl_wifi.SelectedItem Then

                'WifiAdapter.Disconnect()

                Dim WifiConnResult As WiFiConnectionResult

                If txt_password.Text = "" Then
                        WifiConnResult = Await WifiAdapter.ConnectAsync(WifiNetwork, WiFiReconnectionKind.Automatic)
                    Else
                        Dim credential = New PasswordCredential()
                        credential.Password = txt_password.Text
                        WifiConnResult = Await WifiAdapter.ConnectAsync(WifiNetwork, WiFiReconnectionKind.Automatic, credential)
                    End If

                    If (WifiConnResult.ConnectionStatus = WiFiConnectionStatus.Success) Then
                        Dim Bingo As String = "Bingo"
                    End If
                End If

        Next

    End Sub

#End Region

End Class
