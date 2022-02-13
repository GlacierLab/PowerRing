Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
    Dim _computeruCounter
    Dim GPUPowerSensor
    Dim CurrentGPU

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        StartMonitor()
    End Sub
    Private Function StartMonitor()
        _computeruCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total", True)
        GPUFinder()
        Dim dispatcherTimer = New Threading.DispatcherTimer()
        AddHandler dispatcherTimer.Tick, AddressOf dispatcherTimer_Tick
        dispatcherTimer.Interval = New TimeSpan(0, 0, 1)
        dispatcherTimer.Start()
        Return True
    End Function
    Private Sub GPUFinder()
        Dim _computer = New Computer()
        _computer.IsGpuEnabled = True
        _computer.Open()
        For i As Integer = 0 To _computer.Hardware.Count() - 1
            If _computer.Hardware(i).HardwareType = HardwareType.GpuNvidia Then
                CurrentGPU = _computer.Hardware(i)
            End If
        Next
        If CurrentGPU IsNot Nothing Then
            For i As Integer = 0 To CurrentGPU.Sensors.length() - 1
                If CurrentGPU.Sensors(i).SensorType = SensorType.Power Then
                    GPUPowerSensor = CurrentGPU.Sensors(i)
                End If
            Next
            If GPUPowerSensor Is Nothing Then
                MsgBox("不支持当前显卡，可能是驱动版本过低",, "错误")
            End If
        Else
            MsgBox("未找到可检测的显卡",, "错误")
        End If

    End Sub

    Private Sub dispatcherTimer_Tick(sender As Object, e As EventArgs)
        Dim CPU = _computeruCounter.NextValue()
        CPUPercent.Content = CPU.ToString() + "%"
        CurrentGPU.Update()
        Dim GPU = GPUPowerSensor.Value()
        GPUPower.Content = GPU.ToString() + "W"
    End Sub
End Class
