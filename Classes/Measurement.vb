' -------------------------------------------------------------
' Copyright 2016 - Sequitur, A.A. Hondema. All rights reserved.
' -------------------------------------------------------------

Public Class Measurement
    Implements INotifyPropertyChanged

    Private _Sensor As String
    Private _Datatype As String
    Private _Waarde As Double
    Private _Counter As Integer
    Private _SensorStatus As Boolean
    Private _TimeMeasured As DateTime
    Private _LowValue As Double
    Private _TimeLowValue As DateTime
    Private _HighValue As Double
    Private _TimeHighValue As DateTime
    Private _MeasurementPerHour(23) As Decimal

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub NotifyPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Property Sensor
        Get
            Return _Sensor
        End Get
        Set(value)
            _Sensor = value
        End Set
    End Property

    Public Property Datatype
        Get
            Return _Datatype
        End Get
        Set(value)
            _Datatype = value
        End Set
    End Property

    Public Property Waarde
        Get
            Return _Waarde
        End Get
        Set(value)
            _Waarde = value
            NotifyPropertyChanged("Waarde")
        End Set
    End Property

    Public Property Counter
        Get
            Return _Counter
        End Get
        Set(value)
            _Counter = value
        End Set
    End Property

    Public Property SensorStatus
        Get
            Return _SensorStatus
        End Get
        Set(value)
            _SensorStatus = value
        End Set
    End Property

    Public Property TimeMeasured
        Get
            Return _TimeMeasured
        End Get
        Set(value)
            _TimeMeasured = value
        End Set
    End Property

    Public Property LowValue
        Get
            Return _LowValue
        End Get
        Set(value)
            _LowValue = value
        End Set
    End Property

    Public Property TimeLowValue
        Get
            If _TimeLowValue = Nothing Then
                Return Date.MinValue
            Else
                Return _TimeLowValue
            End If
        End Get
        Set(value)
            _TimeLowValue = value
        End Set
    End Property

    Public Property HighValue
        Get
            Return _HighValue
        End Get
        Set(value)
            _HighValue = value
        End Set
    End Property

    Public Property TimeHighValue
        Get
            If _TimeHighValue = Nothing Then
                Return Date.MinValue
            Else
                Return _TimeHighValue
            End If
        End Get
        Set(value)
            _TimeHighValue = value
        End Set
    End Property

    Public Property MeasurementPerHour(ByVal Hour As Integer)
        Get
            Return _MeasurementPerHour(Hour)
        End Get
        Set(value)
            _MeasurementPerHour(Hour) = CInt(value)
        End Set
    End Property
End Class
