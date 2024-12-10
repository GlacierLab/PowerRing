Imports System.ComponentModel
Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
    Dim _computeruCounter
    Dim GPUPowerSensor
    Dim CurrentGPU
    Dim InSupress
    Dim runWorker
    Dim SupressCount

    Dim Powercfg As PowercfgInterface.Instance

    Public Async Function PreInit() As Task
        StartMonitor()
        Powercfg = New PowercfgInterface.Instance()
        Await Powercfg.Init()
        My.Settings.Upgrade()
        For Each val As System.Configuration.SettingsProperty In My.Settings.Properties
            Dim Element = FindName(val.Name)
            If Element.GetType Is GetType(TextBox) Then
                Element = CType(Element, TextBox)
                Element.Text = My.Settings.Item(val.Name)
            ElseIf Element.GetType Is GetType(CheckBox) Then
                Element = CType(Element, CheckBox)
                Element.IsChecked = My.Settings.Item(val.Name)
            ElseIf Element.GetType Is GetType(ComboBox) Then
                Element = CType(Element, ComboBox)
                Element.SelectedIndex = My.Settings.Item(val.Name)

            End If
            Console.WriteLine("{0}  {1}", val.Name, My.Settings.Item(val.Name))
        Next
    End Function

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
    End Sub
    Private Function StartMonitor()
        CpuMonitorMode_SelectionChanged(Nothing, Nothing)
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
        Dim CPU = Math.Round(CDbl(_computeruCounter.NextValue()), 1)
        CPUPercent.Content = CPU.ToString() + "%"
        CurrentGPU.Update()
        Dim GPU = Math.Round(CDbl(GPUPowerSensor.Value()), 1)
        GPUPower.Content = GPU.ToString() + "W"
        Dim canSupress = False
        If runWorker Then
            If CPU > CPUHigh.Text Then
                CPUPercent.Foreground = Brushes.Red
                If InSupress And Not IgnoreCPU.IsChecked Then
                    ExitSupress()
                End If
            ElseIf CPU > CPULow.Text Then
                CPUPercent.Foreground = Brushes.Yellow
                If InSupress Then
                    canSupress = True
                End If
            Else
                CPUPercent.Foreground = Brushes.Green
                canSupress = True
            End If
            If IgnoreCPU.IsChecked Then
                canSupress = True
            End If
            Dim needSupress = False
            If GPU > GPUHigh.Text Then
                GPUPower.Foreground = Brushes.Red
                needSupress = True
            ElseIf GPU > GPULow.Text Then
                GPUPower.Foreground = Brushes.Yellow
                If InSupress Then
                    needSupress = True
                End If
            Else
                GPUPower.Foreground = Brushes.Green
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
        End If
    End Sub
    Private Sub ExitSupress()
        InSupress = False
        SupressStatus.Foreground = Brushes.Red
        SupressStatus.Content = "压制状态：无效"
        Powercfg.ChangeBoostModeAndApply(NormalMode.SelectedIndex)
    End Sub
    Private Sub EnterSupress()
        InSupress = True
        SupressStatus.Foreground = Brushes.Green
        SupressStatus.Content = "压制状态：工作"
        Powercfg.ChangeBoostModeAndApply(SupressMode.SelectedIndex)
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

    Private Sub RunBtn_Click(sender As Object, e As RoutedEventArgs) Handles RunBtn.Click
        If runWorker Then
            runWorker = False
            If InSupress Then
                ExitSupress()
            End If
            RunBtn.Content = "启动压制器"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
        Else
            runWorker = True
            RunBtn.Content = "正在压制，点击停止"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF86C166"))
        End If
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If runWorker And InSupress Then
            ExitSupress()
        End If
        For Each val As System.Configuration.SettingsProperty In My.Settings.Properties
            Dim Element = FindName(val.Name)
            If Element.GetType Is GetType(TextBox) Then
                Element = CType(Element, TextBox)
                My.Settings.Item(val.Name) = Integer.Parse(Element.Text)
            ElseIf Element.GetType Is GetType(CheckBox) Then
                Element = CType(Element, CheckBox)
                My.Settings.Item(val.Name) = Element.IsChecked
            ElseIf Element.GetType Is GetType(ComboBox) Then
                Element = CType(Element, ComboBox)
                My.Settings.Item(val.Name) = Element.SelectedIndex
            End If
            Console.WriteLine("{0}  {1}", val.Name, My.Settings.Item(val.Name))
        Next
        My.Settings.Save()
    End Sub

    Private Sub Github_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Github.MouseDown
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/PowerRing")
        startInfo.UseShellExecute = True
        Process.Start(startInfo)
    End Sub

    Private Sub CpuMonitorMode_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CpuMonitorMode.SelectionChanged
        _computeruCounter = New PerformanceCounter("Processor Information", If(CpuMonitorMode.SelectedIndex, "% Processor Utility", "% Processor Time"), "_Total", True)
    End Sub

    ''REF: https://stackoverflow.com/a/12721673
    Private Sub NumberValidationTextBox(ByVal sender As TextBox, ByVal e As TextCompositionEventArgs)
        e.Handled = Not Integer.TryParse(e.Text, Nothing)
    End Sub

    Private Sub NumberFormatTextBox(sender As Object, e As TextChangedEventArgs)
        Dim Box = CType(e.Source, TextBox)
        If Box.Text.Length = 0 Then
            Box.Text = 0
        ElseIf Box.Text.IndexOf(" ") > 0 Then
            Dim Pos = Box.SelectionStart - 1
            Box.Text = Box.Text.Replace(" ", "")
            Box.Select(Pos, 0)
        End If
    End Sub
End Class
