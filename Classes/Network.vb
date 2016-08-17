' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Imports Windows.Networking
Imports Windows.Networking.Connectivity

Public Class Network

    Private _Network As String
    Private _AdapterID As String
    Private _IPaddress As String
    Private _MACaddress As String

    Public Property Network
        Get
            Return _Network
        End Get
        Set(value)
            _Network = value
        End Set
    End Property

    Public Property AdapterID
        Get
            Return _AdapterID
        End Get
        Set(value)
            _AdapterID = value
        End Set
    End Property

    Public Property IPaddress
        Get
            Return _IPaddress
        End Get
        Set(value)
            _IPaddress = value
        End Set
    End Property

    Public Property MACaddress
        Get
            Return _MACaddress
        End Get
        Set(value)
            _MACaddress = value
        End Set
    End Property


    Public Sub New()
        Dim ICP = NetworkInformation.GetInternetConnectionProfile()

        If Not ICP Is Nothing Then
            _Network = ICP.ProfileName
            _AdapterID = ICP.NetworkAdapter.NetworkAdapterId.ToString

            Dim Hostnames = NetworkInformation.GetHostNames()

            For Each hn In Hostnames
                If hn.IPInformation IsNot Nothing Then
                    If hn.IPInformation.NetworkAdapter.NetworkAdapterId = ICP.NetworkAdapter.NetworkAdapterId And hn.Type = HostNameType.Ipv4 Then
                        _IPaddress = hn.CanonicalName
                    End If
                End If
            Next
        End If

    End Sub
End Class
