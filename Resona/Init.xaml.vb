Imports System.Configuration
Imports System.Threading

Public Class Init

    Private Shared _mutex As Mutex = Nothing

    Private Async Sub Init_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Dim createdNew As Boolean
        _mutex = New Mutex(True, "ProjectResona_PowerRing", createdNew)
        If Not createdNew Then
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

    Private Sub KillAndReload(sender As Object, e As MouseButtonEventArgs)
        Dim Proc = Process.GetProcessesByName("Resona")
        If Proc.Length > 0 Then
            For Each Pro In Proc
                If Not Pro.Id Like Process.GetCurrentProcess().Id Then
                    Pro.Kill()
                End If
            Next
        End If
        _mutex.Dispose()
        Dim NewInit As New Init()
        NewInit.Show()
        Close()
    End Sub

    Private Async Function LoadMain() As Task
        If Not ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).HasFile Then
            My.Settings.Upgrade()
        End If
        If Not My.Settings.GPUSelected Then
            Status.Content = "正在扫描显卡"
            Dim SelectWindow As New SelectWindow()
            Await SelectWindow.PreInit()
            SelectWindow.Show()
        Else
            Dim Main As MainWindow = New MainWindow()
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
        _mutex.Dispose()
        Close()
    End Function

End Class
