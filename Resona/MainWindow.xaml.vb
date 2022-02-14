Imports System.ComponentModel
Imports System.IO
Imports LibreHardwareMonitor.Hardware

Public Class MainWindow
#Disable Warning CA2101 ' Specify marshaling for P/Invoke string arguments
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (lpApplicationName As String, lpKeyName As String, lpDefault As String, lpReturnedString As String, nSize As Integer, lpFileName As String) As Integer
#Disable Warning CA2101 ' Specify marshaling for P/Invoke string arguments
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (lpApplicationName As String, lpKeyName As String, lpString As String, lpFileName As String) As Integer
    Public Shared Function GetINI(Section As String, AppName As String, lpDefault As String, FileName As String) As String
        Dim Str As String = ""
        Str = LSet(Str, 256)
        GetPrivateProfileString(Section, AppName, lpDefault, Str, Len(Str), FileName)
        Return Microsoft.VisualBasic.Left(Str, InStr(Str, Chr(0)) - 1)
    End Function
    Public Shared Function WriteINI(Section As String, AppName As String, lpDefault As String, FileName As String) As Long
        WriteINI = WritePrivateProfileString(Section, AppName, lpDefault, FileName)
    End Function

    Dim _computeruCounter
    Dim GPUPowerSensor
    Dim CurrentGPU
    Dim InSupress
    Dim runWorker
    Dim SupressCount
    Dim ConfigFile As String = ".\ThrottleStop.ini"
    'POWERLIMITEDX是PL2,POWERLIMITEAX是PL1
    Dim PL1Prefix As String
    Dim PL2Prefix As String

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Directory.SetCurrentDirectory(Environment.CurrentDirectory + "\ThrottleStop")
        RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
        StartMonitor()
        '他娘的不研究怎么直接改PL1PL2了，直接调用ThrottleStop一把梭，最原始的思路就是最他娘的好使
        If Not File.Exists(ConfigFile) Then
            MsgBox("首次启动需要进行ThrottleStop的初始化设置" + Environment.NewLine + "请在新打开的ThrottleStop窗口中完成TPL设置和其他基础选项设置" + Environment.NewLine + "请务必勾选Start Minimized选项以保证使用体验！" + Environment.NewLine + "你也可以拷贝一份ThrottleStop配置文件到目录" + Environment.NewLine + "请在设置完成后重启本程序",, "提示")
            Process.Start("ThrottleStop.exe")
            End
        Else
            '就有铸币不看提示要完成TPL设置，逼我加个TryCatch
            Try
                PL1Prefix = GetINI("ThrottleStop", "POWERLIMITEAX", "", ConfigFile).Substring(0, 7)
                PL2Prefix = GetINI("ThrottleStop", "POWERLIMITEDX", "", ConfigFile).Substring(0, 7)
            Catch
                MsgBox("你没有在ThrottleStop内设置PL1和PL2！",, "错误")
                Process.Start("ThrottleStop.exe")
                End
            End Try
        End If
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
        KillProcess()
        InSupress = False
        SupressStatus.Foreground = Brushes.Red
        SupressStatus.Content = "压制状态：无效"
        Dim SupressHex As String = PL1Prefix + (Integer.Parse(CPUOrigin.Text) * 8).ToString("X3")
        WriteINI("ThrottleStop", "POWERLIMITEAX", SupressHex, ConfigFile)
        WriteINI("ThrottleStop", "POWERLIMITEDX", SupressHex, ConfigFile)
        Task.Run(Async Function()
                     Await Task.Delay(1000)
                     Interaction.Shell(".\ThrottleStop.exe", AppWinStyle.MinimizedNoFocus)
                 End Function)
    End Sub
    Private Sub EnterSupress()
        KillProcess()
        InSupress = True
        SupressStatus.Foreground = Brushes.Green
        SupressStatus.Content = "压制状态：工作"
        Dim SupressHex As String = PL1Prefix + (Integer.Parse(CPUSupress.Text) * 8).ToString("X3")
        WriteINI("ThrottleStop", "POWERLIMITEAX", SupressHex, ConfigFile)
        WriteINI("ThrottleStop", "POWERLIMITEDX", SupressHex, ConfigFile)
        Task.Run(Async Function()
                     Await Task.Delay(1000)
                     Interaction.Shell(".\ThrottleStop.exe", AppWinStyle.MinimizedNoFocus)
                 End Function)
    End Sub
    Private Sub KillProcess()
        'Interaction.Shell("taskkill /f /im ThrottleStop.exe")
        Dim Proc = Process.GetProcessesByName("ThrottleStop")
        If Proc.Length > 0 Then
            Proc(0).Kill()
        End If
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
            RunBtn.Content = "启动压制器"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFAD689"))
            KillProcess()
        Else
            runWorker = True
            RunBtn.Content = "正在压制，点击停止"
            RunBtn.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF86C166"))
        End If
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        KillProcess()
    End Sub

    Private Sub Github_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Github.MouseDown
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/PowerRing")
        startInfo.UseShellExecute = True
        Process.Start(startInfo)
    End Sub
End Class
