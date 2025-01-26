Imports System.Globalization
Imports System.Reflection
Imports System.Threading
Imports System.Windows.Threading

Class Application

    Public Shared _mutex As Mutex = Nothing

    Public Sub New()
        If Command() = "--runas=admin" Then
            My.Settings.LaunchAsAdmin = True
            My.Settings.Save()
        End If
    End Sub

    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException

        MessageBox.Show("聚能环遇到无法处理的错误，即将在浏览器内打开GitHub提交错误")
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/PowerRing/issues/new?title=" + e.Exception.GetType().ToString() + "&body=" + e.Exception.Message + "%0A```" + e.Exception.StackTrace.Replace(" at", "%0Aat") + "%0A```%0A" + Generate_VersionReport() + "&labels=bug") With {
            .UseShellExecute = True
        }
        Process.Start(startInfo)
        e.Handled = False
    End Sub

    Private Function Generate_VersionReport() As String
        Dim Result = ""
        Result += "版本号:" + Assembly.GetEntryAssembly().GetName().Version.ToString()
        Result += "%0A包渠道:" + If(Util.IsRunningAsUwp(), "MSIX", "Win32")
        Result += "%0A设备架构:" + Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
        Result += "%0A系统语言:" + CultureInfo.CurrentCulture.Name
        Result += "%0A请在下方描述你遇到问题时的场景，建议同时提供设备配置信息"
        Return Result
    End Function

End Class
