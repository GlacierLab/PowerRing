Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
    Dim _computeruCounter
    Dim GPUPowerSensor
    Dim CurrentGPU
    Dim InSupress
    Dim SupressCount

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
        Dim canSupress = False
        If CPU > CPUHigh.Text Then
            CPUPercent.Foreground = Brushes.Red
            ExitSupress()
        ElseIf CPU > CPULow.Text Then
            CPUPercent.Foreground = Brushes.Yellow
            If InSupress Then
                canSupress = True
            End If
        Else
            CPUPercent.Foreground = Brushes.Green
            canSupress = True
        End If
        Dim needSupress = False
        If GPU > GPUHigh.Text Then
            needSupress = True
        ElseIf GPU > GPULow.Text Then
            If InSupress Then
                needSupress = True
            End If
        End If
        Dim CounterAction = False
        If Not InSupress And canSupress And needSupress Then
            CountSupress()
            CounterAction = True
        End If
        If Not needSupress And InSupress Then
            CountExit()
            CounterAction = True
        End If
        If Not CounterAction Then
            ClearCount()
        End If
    End Sub
    Private Sub ExitSupress()
        InSupress = False
        SupressStatus.Foreground = Brushes.Red
        SupressStatus.Content = "压制状态：禁用"
    End Sub
    Private Sub EnterSupress()
        InSupress = True
        SupressStatus.Foreground = Brushes.Green
        SupressStatus.Content = "压制状态：启用"
    End Sub
    Private Sub CountExit()
        Counter.Foreground = Brushes.Red
        If SupressCount = BeforeTime.Text Then
            ExitSupress()
            SupressCount = 0
            Counter.Content = ""
        Else
            SupressCount += 1
            Counter.Content = SupressCount
        End If
    End Sub
    Private Sub CountSupress()
        Counter.Foreground = Brushes.Green
        If SupressCount = BeforeTime.Text Then
            EnterSupress()
            SupressCount = 0
            Counter.Content = ""
        Else
            SupressCount += 1
            Counter.Content = SupressCount
        End If
    End Sub
    Private Sub ClearCount()
        SupressCount = 0
        Counter.Content = ""
    End Sub

End Class
