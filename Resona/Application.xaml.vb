Imports System.Windows.Threading

Class Application
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException

        MessageBox.Show("聚能环遇到无法处理的错误，即将在浏览器内打开GitHub提交错误")
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/PowerRing/issues/new?title=" + e.Exception.GetType().ToString() + "&body=" + e.Exception.Message + e.Exception.StackTrace.Replace(" at", "%0Aat") + "&labels=bug") With {
            .UseShellExecute = True
        }
        Process.Start(startInfo)
        e.Handled = True
    End Sub

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

End Class
