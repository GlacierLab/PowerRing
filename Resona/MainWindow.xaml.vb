Imports System.ComponentModel
Imports System.Configuration
Imports System.Reflection
Imports System.Windows.Threading
Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
    Dim _computeruCounter As PerformanceCounter
    Dim GPUPowerSensor As ISensor
    Dim CurrentGPU As IHardware
    Dim InSupress As Boolean
    Dim runWorker As Boolean
    Dim SupressCount As Int16
    Dim InStudy As Boolean
    Dim MaxPower As Double

    Dim Powercfg As PowercfgInterface.Instance
    Dim dispatcherTimer As DispatcherTimer

    Dim _computer As Computer

    Public Async Function PreInit() As Task(Of Boolean)
        Title = "聚能环 PowerRing " + Assembly.GetEntryAssembly().GetName().Version.ToString()
        If Not StartMonitor() Then
            Close()
            Return False
        End If
        Powercfg = New PowercfgInterface.Instance()
        Await Powercfg.Init()
        LoadConf()
        If My.Settings.SupressOnLaunch Then
            RunBtn_Click(Nothing, Nothing)
        End If
        '''只有功率传感器值得自学习
        If GPUPowerSensor.SensorType = SensorType.Power Then
            SelfStudy.Visibility = Visibility.Visible
        End If
        Return True
    End Function

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
    End Sub
    Private Function StartMonitor() As Boolean
        CpuMonitorMode_SelectionChanged(Nothing, Nothing)
        If Not GPUFinder() Then
            Return False
        End If
        dispatcherTimer = New Threading.DispatcherTimer()
        AddHandler dispatcherTimer.Tick, AddressOf dispatcherTimer_Tick
        dispatcherTimer.Interval = New TimeSpan(0, 0, My.Settings.TickInterval)
        dispatcherTimer.Start()
        Return True
    End Function
    Private Function GPUFinder() As Boolean
        _computer = New Computer With {
            .IsGpuEnabled = True
        }
        _computer.Open()
        For i As Integer = 0 To _computer.Hardware.Count() - 1
            If _computer.Hardware(i).Name = My.Settings.GPUDevice Then
                CurrentGPU = _computer.Hardware(i)
            End If
        Next
        If CurrentGPU IsNot Nothing Then
            CurrentGPU.Update()
            For i As Integer = 0 To CurrentGPU.Sensors.length() - 1
                If CurrentGPU.Sensors(i).Identifier.ToString() = My.Settings.Sensor Then
                    GPUPowerSensor = CurrentGPU.Sensors(i)
                End If
            Next
            If GPUPowerSensor Is Nothing Then
                Return False
            Else
                GPUName.Text = CurrentGPU.Name + " - " + GPUPowerSensor.Name
                Return True
            End If
        Else
            Return False
        End If
    End Function

    Private Sub ReselectGPU()
        My.Settings.GPUSelected = False
        My.Settings.Save()
        Dim NewInit As New Init()
        NewInit.Show()
        Close()
    End Sub

    Private Sub dispatcherTimer_Tick(sender As Object, e As EventArgs)
        Dim CPU = Math.Round(CDbl(_computeruCounter.NextValue()), 1)
        CPUPercent.Content = CPU.ToString() + "%"
        CurrentGPU.Update()
        Dim GPU = Math.Round(CDbl(GPUPowerSensor.Value()), 1)
        If InStudy AndAlso GPU > MaxPower Then
            MaxPower = GPU
            GPUHigh.Text = Convert.ToInt32(0.75 * MaxPower)
            GPULow.Text = Convert.ToInt32(0.6 * MaxPower)
        End If
        GPUPower.Content = GPU.ToString() + If(GPUPowerSensor.SensorType = SensorType.Load, "%", "W")
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
            If WindowState = WindowState.Minimized Then
                Title = "C:" + Fix(CPU).ToString() + "% G:" + Fix(GPU).ToString() + If(GPUPowerSensor.SensorType = SensorType.Load, "% ", "W ") + If(InSupress, "压制中", "未压制")
            End If
        End If
    End Sub
    Private Async Sub ExitSupress()
        Taskbar.ProgressState = Shell.TaskbarItemProgressState.Normal
        InSupress = False
        SupressStatus.Foreground = Brushes.Red
        SupressStatus.Content = "压制状态：无效"
        If DetectPlanChange.IsChecked Then
            If Await Powercfg.IsProfileChanged() And runWorker Then
                DetectPlanChange.Foreground = Brushes.Red
            End If
        End If
        Powercfg.ChangeBoostModeAndApply(NormalMode.SelectedIndex)
    End Sub
    Private Async Sub EnterSupress()
        Taskbar.ProgressState = Shell.TaskbarItemProgressState.Error
        InSupress = True
        SupressStatus.Foreground = Brushes.Green
        SupressStatus.Content = "压制状态：工作"
        If DetectPlanChange.IsChecked Then
            If Await Powercfg.IsProfileChanged() And runWorker Then
                DetectPlanChange.Foreground = Brushes.Red
            End If
        End If
        Powercfg.ChangeBoostModeAndApply(SupressMode.SelectedIndex)
    End Sub
    Private Sub CountExit()
        Counter.Foreground = Brushes.Red
        If SupressCount = BeforeTime.Text Then
            ExitSupress()
            SupressCount = 0
            Counter.Content = "0"
        Else
            Taskbar.ProgressState = Shell.TaskbarItemProgressState.Paused
            SupressCount += 1
            Counter.Content = SupressCount
        End If
    End Sub
    Private Sub CountSupress()
        Counter.Foreground = Brushes.Green
        If SupressCount = BeforeTime.Text Then
            EnterSupress()
            SupressCount = 0
            Counter.Content = "0"
        Else
            Taskbar.ProgressState = Shell.TaskbarItemProgressState.Paused
            SupressCount += 1
            Counter.Content = SupressCount
        End If
    End Sub
    Private Sub ClearCount()
        Taskbar.ProgressState = If(InSupress, Shell.TaskbarItemProgressState.Error, Shell.TaskbarItemProgressState.Normal)
        SupressCount = 0
        Counter.Content = "0"
    End Sub

    Private Sub RunBtn_Click(sender As Object, e As RoutedEventArgs) Handles RunBtn.Click
        If runWorker Then
            runWorker = False
            If InSupress Then
                ExitSupress()
            End If
            Taskbar.ProgressState = Shell.TaskbarItemProgressState.None
            RunBtn.Content = "启动压制器"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
            GPUPower.Foreground = Brushes.Black
            CPUPercent.Foreground = Brushes.Black
            Counter.Content = "0"
            Counter.Foreground = Brushes.Black
            DetectPlanChange.Foreground = Brushes.Black
            If InStudy Then
                SelfStudy.Content = "已学习"
                SelfStudy.IsEnabled = True
                SelfStudy.Foreground = Brushes.Green
                InStudy = False
                GPULow.IsEnabled = True
                GPUHigh.IsEnabled = True
                SaveConf()
            End If
        Else
            Taskbar.ProgressState = Shell.TaskbarItemProgressState.Normal
            runWorker = True
            RunBtn.Content = "正在压制，点击停止"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF86C166"))
            If DetectPlanChange.IsChecked Then
                DetectPlanChange.Foreground = Brushes.Green
            End If
        End If
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If GPUPowerSensor Is Nothing Then
            Return
        End If
        If runWorker And InSupress Then
            ExitSupress()
        End If
        _computer.Close()
        SaveConf()
    End Sub

    Private Sub SaveConf()
        For Each val As SettingsProperty In My.Settings.Properties
            Dim Element = FindName(val.Name)
            If Element Is Nothing Then
                Continue For
            ElseIf Element.GetType Is GetType(TextBox) Then
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

    Private Sub LoadConf()
        For Each val As SettingsProperty In My.Settings.Properties
            Dim Element = FindName(val.Name)
            If Element Is Nothing Then
                Continue For
            ElseIf Element.GetType Is GetType(TextBox) Then
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
    End Sub

    Private Sub Github_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Github.MouseDown
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/PowerRing") With {
            .UseShellExecute = True
        }
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
        If (Box Is GPUHigh Or Box Is GPULow) And GPUPowerSensor.SensorType = SensorType.Load Then
            If Box.Text > 99 Then
                Box.Text = 99
            End If
        End If
    End Sub

    Private Sub TickInterval_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TickInterval.TextChanged
        Dim Interval As Integer
        Dim Success = Integer.TryParse(e.Source.Text, Interval)
        If Success And Interval > 0 Then
            dispatcherTimer.Interval = New TimeSpan(0, 0, Interval)
        End If
    End Sub

    Private Sub GPUName_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If MessageBox.Show("是否重新选择显卡和传感器？", "设备选择", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
            ReselectGPU()
        End If
    End Sub

    Private Sub MainWindow_StateChanged(sender As Object, e As EventArgs) Handles Me.StateChanged
        If WindowState = WindowState.Normal Then
            Title = "聚能环 PowerRing " + Assembly.GetEntryAssembly().GetName().Version.ToString()
        End If
    End Sub

    Private Sub ApplyMode_Click(sender As Object, e As RoutedEventArgs) Handles ApplyMode.Click
        Powercfg.ChangeBoostModeAndApply(NormalMode.SelectedIndex)
    End Sub

    Private Sub ConfDir_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles ConfDir.MouseDown
        Process.Start(New System.Diagnostics.ProcessStartInfo() With {
            .FileName = IO.Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath),
            .UseShellExecute = True,
            .Verb = "open"
        })
    End Sub

    Private Sub SelfStudy_Click(sender As Object, e As RoutedEventArgs) Handles SelfStudy.Click
        MessageBox.Show("===请认真阅读自学习说明===" + Environment.NewLine + "自学习功能适用于你无法合理配置显卡功耗阈值的情况，启动自学习之后会立即启动压制器，请在此期间运行显卡负荷较大的游戏，并且不要使用帧率限制和垂直同步，确保显卡性能完全利用，聚能环会自动学习显卡最大功耗并配置合理数值，游戏退出后请手动停止压制器，即可保存学习结果。自学习得到的数值并非绝对最优，只保证相对有效。", "自学习说明")
        SelfStudy.Content = "学习中"
        SelfStudy.IsEnabled = False
        GPULow.IsEnabled = False
        GPULow.Text = 0
        GPUHigh.IsEnabled = False
        GPUHigh.Text = 0
        MaxPower = 0.0
        InStudy = True
        If Not runWorker Then
            RunBtn_Click(Nothing, Nothing)
        End If
    End Sub
End Class
