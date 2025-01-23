Imports System.Configuration
Imports System.IO
Imports System.Security.Principal
Imports System.Threading

Public Class Init
    Dim RainbowBrush As Brush
    Public Sub New()
        If My.Settings.LaunchAsAdmin And Not IsAdministrator() Then
            Dim info As New ProcessStartInfo(System.Environment.ProcessPath, "--runas=admin") With {
                    .UseShellExecute = True,
                    .Verb = "runas"
                }
            Process.Start(info)
            Environment.Exit(0)
        Else
            InitializeComponent()
        End If
    End Sub

    Private Async Sub Init_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Application._mutex?.Dispose()
        Dim createdNew As Boolean
        Application._mutex = New Mutex(True, "ProjectResona_PowerRing", createdNew)
        If Not createdNew Then
            RainbowBrush = BorderBrush
            Status.Content = "无法同时运行多个实例" + Environment.NewLine + "点击继续并关闭其他实例"
            Status.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF0000"))
            BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF0000"))
            ShowInTaskbar = True
            Title = "初始化失败"
            AddHandler MouseLeftButtonDown, AddressOf KillAndReload
        Else
            Await LoadMain()
        End If
    End Sub

    Private Async Sub KillAndReload(sender As Object, e As MouseButtonEventArgs)
        ShowInTaskbar = False
        Title = "Resona is loading..."
        Status.Content = "启动中..."
        Status.Foreground = Brushes.Black
        BorderBrush = RainbowBrush
        RemoveHandler MouseLeftButtonDown, AddressOf KillAndReload
        Await Task.Run(Sub()
                           Dim Proc = Process.GetProcessesByName("Resona")
                           If Proc.Length > 0 Then
                               For Each Pro In Proc
                                   If Not Pro.Id Like Environment.ProcessId Then
                                       Pro.Kill()
                                   End If
                               Next
                           End If
                       End Sub)
        Init_ContentRendered(Nothing, Nothing)
    End Sub

    Private Async Function LoadMain() As Task
        If Not ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile Then
            Dim CurrentConf = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath)
            Dim ConfPath = Directory.GetParent(CurrentConf)
            If ConfPath.Exists Then
                Status.Content = "正在导入旧版配置"
                Await Task.Run(Sub()
                                   My.Settings.Upgrade()
                                   For Each Dir As DirectoryInfo In ConfPath.GetDirectories()
                                       If Not Dir.ToString() = CurrentConf Then
                                           Dir.Delete(True)
                                       End If
                                   Next
                               End Sub)
            End If
        End If
        If Not My.Settings.GPUSelected Then
            Status.Content = "正在扫描显卡"
            Dim SelectWindow As New SelectWindow()
            Await SelectWindow.PreInit()
            SelectWindow.Show()
        Else
            Dim Main As New MainWindow()
            Dim ReturnValue = Await Main.PreInit()
            If ReturnValue Then
                Main.Show()
            Else
                Status.Content = "正在扫描显卡"
                Dim SelectWindow As New SelectWindow()
                Await SelectWindow.PreInit()
                SelectWindow.Show()
            End If
        End If
        Await Task.Delay(50)
        Close()
    End Function

    Public Shared Function IsAdministrator() As Boolean
        Return (New WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator)
    End Function

End Class
