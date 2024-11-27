Public Class Init

    Private Async Sub Init_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Dim Main As MainWindow = New MainWindow()
        Await Main.PreInit()
        Main.Show()
        Await Task.Delay(250)
        Close()
    End Sub

End Class
