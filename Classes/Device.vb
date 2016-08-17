' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Imports Windows.Security.ExchangeActiveSyncProvisioning

Public Class Device

    Private _Productname As String
    Private _ProductID As String
    Private _Product As String
    Private _OS As String
    Private _Devicetype As String

    Public Property Productname
        Get
            Return _Productname
        End Get
        Set(value)
            _Productname = value
        End Set
    End Property

    Public Property ProductID
        Get
            Return _ProductID
        End Get
        Set(value)
            _ProductID = value
        End Set
    End Property

    Public Property Product
        Get
            Return _Product
        End Get
        Set(value)
            _Product = value
        End Set
    End Property

    Public Property OS
        Get
            Return _OS
        End Get
        Set(value)
            _OS = value
        End Set
    End Property

    Public Property Devicetype
        Get
            Return _Devicetype
        End Get
        Set(value)
            _Devicetype = value
        End Set
    End Property

    Public Sub New()

        Dim DeviceInfo As New EasClientDeviceInformation

        _Productname = DeviceInfo.FriendlyName
        _Product = DeviceInfo.SystemProductName
        _ProductID = DeviceInfo.Id.ToString
        _OS = DeviceInfo.OperatingSystem
        _Devicetype = DeviceInfo.SystemSku

    End Sub
End Class
