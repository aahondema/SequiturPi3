' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Public Class AppNode

    Private _HomeID As String
    Private _NodeID As String
    Private _NodeName As String
    Private _NodeStatus As String
    Private _NodeType As String
    Private _Image As String
    Private _DeviceType As String
    Private _SupportedCommandClasses As String
    Private _Color As Integer
    ' Manufacturer
    Private _ManufacturerID As String
    Private _ProductID As String
    Private _ProductType As String
    ' Version
    Private _Application As String
    Private _Library As String
    Private _Protocol As String
    ' Meter
    Private _CanReset As String
    Private _MeterType As Byte
    Private _Units As String
    Private _Scale As String
    Private _PowerConsumption As String
    Private _Unit As String
    ' Alarm
    Private _AlarmType As ZWave.CommandClasses.AlarmType
    Private _AlarmLevel As Byte
    Private _AlarmDetail As ZWave.CommandClasses.AlarmDetailType

    Public Property HomeID()
        Get
            Return _HomeID
        End Get
        Set(value)
            _HomeID = value
        End Set
    End Property

    Public ReadOnly Property NodeIDTekst()
        Get
            Return "NodeID: " & _NodeID
        End Get
    End Property


    Public Property NodeID()
        Get
            Return _NodeID
        End Get
        Set(value)
            _NodeID = value
        End Set
    End Property

    Public Property NodeName()
        Get
            Return _NodeName
        End Get
        Set(value)
            _NodeName = value
        End Set
    End Property

    Public Property NodeStatus()
        Get
            If _NodeStatus = "True" Then
                Return "On"
            Else
                Return "Off"
            End If
        End Get
        Set(value)
            _NodeStatus = value
        End Set
    End Property

    Public Property NodeType()
        Get
            Return _NodeType
        End Get
        Set(value)
            _NodeType = value
        End Set
    End Property

    Public Property DeviceType()
        Get
            Return _DeviceType
        End Get
        Set(value)
            _DeviceType = value
        End Set
    End Property

    Public Property SupportedCommandClasses()
        Get
            Return _SupportedCommandClasses
        End Get
        Set(value)
            _SupportedCommandClasses = value
        End Set
    End Property

    Public Property Color
        Get
            Return _Color
        End Get
        Set(value)
            _Color = value
        End Set
    End Property

    Public Property ManufacturerID()
        Get
            If _ManufacturerID = "0089" Then
                Return "Aeon Labs"
            ElseIf _ManufacturerID = "153" Then
                Return "Greenwave"
            Else
                Return _ManufacturerID
            End If
        End Get
        Set(value)
            _ManufacturerID = value
        End Set
    End Property

    Public Property ProductID()
        Get
            Return _ProductID
        End Get
        Set(value)
            _ProductID = value
        End Set
    End Property

    Public Property ProductType()
        Get
            If _NodeID = 1 Then
                Return "Z-Wave Z-Stick"
            ElseIf _NodeID = 4 Then
                Return "Multisensor"
            ElseIf _NodeID = 3 Or _NodeID = 5 Then
                Return "Smart Powernode"
            Else
                Return _Producttype
            End If
        End Get
        Set(value)
            _Producttype = value
        End Set
    End Property

    Public Property Application()
        Get
            Return _Application
        End Get
        Set(value)
            _Application = value
        End Set
    End Property

    Public Property Library()
        Get
            Return _Library
        End Get
        Set(value)
            _Library = value
        End Set
    End Property

    Public Property Protocol()
        Get
            Return _Protocol
        End Get
        Set(value)
            _Protocol = value
        End Set
    End Property

    Public Property CanReset()
        Get
            Return _CanReset
        End Get
        Set(value)
            _CanReset = value
        End Set
    End Property

    Public Property MeterType()
        Get
            Return _MeterType
        End Get
        Set(value)
            _MeterType = value
        End Set
    End Property

    Public Property Units()
        Get
            Return _Units
        End Get
        Set(value)
            _Units = value
        End Set
    End Property

    Public Property Scale()
        Get
            Return _Scale
        End Get
        Set(value)
            _Scale = value
        End Set
    End Property

    Public Property PowerConsumption()
        Get
            Return _PowerConsumption
        End Get
        Set(value)
            _PowerConsumption = value
        End Set
    End Property

    Public ReadOnly Property PowerConsumptionTekst
        Get
            If Not _PowerConsumption = 0 Then
                Return "(" & _PowerConsumption & " " & Unit & ")"
            Else
                Return ""
            End If
        End Get
    End Property

    Public Property Unit()
        Get
            Return _Unit
        End Get
        Set(value)
            _Unit = value
        End Set
    End Property

    Public Property Image() As String
        Get
            Return _Image
        End Get
        Set(value As String)
            _Image = value
        End Set
    End Property

    Public Property AlarmType() As String
        Get
            Return _AlarmType
        End Get
        Set(value As String)
            _AlarmType = value
        End Set
    End Property

    Public Property AlarmLevel() As String
        Get
            Return _AlarmLevel
        End Get
        Set(value As String)
            _AlarmLevel = value
        End Set
    End Property

    Public Property AlarmDetail() As String
        Get
            Return _AlarmDetail
        End Get
        Set(value As String)
            _AlarmDetail = value
        End Set
    End Property

    Public Sub New()

    End Sub

End Class

Public Class AppNodes
    Inherits CollectionBase

    Public Sub Add(ByVal AppNode As AppNode)
        List.Add(AppNode)
    End Sub

End Class

