﻿Imports System.ComponentModel
Imports System.Reflection
Imports System.Windows.Threading
Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
    Dim _computeruCounter
    Dim GPUPowerSensor
    Dim CurrentGPU
    Dim InSupress
    Dim runWorker
    Dim SupressCount

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
        For Each val As System.Configuration.SettingsProperty In My.Settings.Properties
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
            For i As Integer = 0 To CurrentGPU.Sensors.length() - 1
                If CurrentGPU.Sensors(i).Name = My.Settings.Sensor Then
                    GPUPowerSensor = CurrentGPU.Sensors(i)
                End If
            Next
            If GPUPowerSensor Is Nothing Then
                Return False
            Else
                GPUName.Text = CurrentGPU.Name
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
            If WindowState = WindowState.Minimized Then
                Title = "C:" + Fix(CPU).ToString() + "% G:" + Fix(GPU).ToString() + "W " + If(InSupress, "压制中", "未压制")
            End If
        End If
    End Sub
    Private Sub ExitSupress()
        Taskbar.ProgressState = Shell.TaskbarItemProgressState.Normal
        InSupress = False
        SupressStatus.Foreground = Brushes.Red
        SupressStatus.Content = "压制状态：无效"
        Powercfg.ChangeBoostModeAndApply(NormalMode.SelectedIndex)
    End Sub
    Private Sub EnterSupress()
        Taskbar.ProgressState = Shell.TaskbarItemProgressState.Error
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
        Else
            Taskbar.ProgressState = Shell.TaskbarItemProgressState.Normal
            runWorker = True
            RunBtn.Content = "正在压制，点击停止"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF86C166"))
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
        For Each val As System.Configuration.SettingsProperty In My.Settings.Properties
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
End Class
