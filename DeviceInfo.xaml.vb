' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Imports Windows.Globalization

Public NotInheritable Class DeviceInfo
    Inherits Page

#Region "Declaraties"

    ' AppSettings
    Dim AppSettings As AppSettings
#End Region

#Region "Init"

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)

        ' =============
        ' Program start
        ' =============
        MyBase.OnNavigatedTo(e)

        Init(e)

    End Sub

    Protected Sub Init(e As NavigationEventArgs)

        ' =======================================
        ' Haal de device en netwerk informatie op
        ' =======================================

        AppSettings = New AppSettings
        AppSettings = e.Parameter

        ApplicationLanguages.PrimaryLanguageOverride = AppSettings.AppLanguage

        ShowDeviceInformation()

        ShowNetworkInformation()

    End Sub

    Protected Sub ShowDeviceInformation()

        ' =========================
        ' Toon de device informatie
        ' =========================

        If Not AppSettings.Device Is Nothing Then
            txt_name.Text = AppSettings.Device.Productname
            txt_product.Text = AppSettings.Device.Product
            txt_productID.Text = AppSettings.Device.ProductID
            txt_OS.Text = AppSettings.Device.OS
            txt_device.Text = AppSettings.Device.Devicetype
        End If

    End Sub

    Protected Sub ShowNetworkInformation()

        ' =============================
        ' Toon de netwerk informatie op
        ' =============================

        If Not AppSettings.Network.Network Is Nothing Then
            txt_network.Text = AppSettings.Network.Network
            txt_adapterID.Text = AppSettings.Network.AdapterID
            txt_IPaddress.Text = AppSettings.Network.IPaddress
        Else
            ' Geen IP adres gevonden, zorg ervoor dat er geen connectie met de webservice kan worden gemaakt
            txt_IPaddress.Text = "Unknown"
        End If

    End Sub

#End Region

#Region "Navigatie"
    Private Sub HamburgerButton_Click(sender As Object, e As RoutedEventArgs)

        MySplitView.IsPaneOpen = Not MySplitView.IsPaneOpen

    End Sub

    Private Sub Menu_Sensors_Click(sender As Object, e As RoutedEventArgs) Handles Menu_Sensors.Click

        Frame.Navigate(GetType(MainPage), AppSettings)

    End Sub

    Private Sub Menu_Settings_Click(sender As Object, e As RoutedEventArgs) Handles Menu_Settings.Click

        Frame.Navigate(GetType(Settings), AppSettings)

    End Sub

#End Region

End Class