Imports System.Diagnostics

Class MainWindow
    Dim cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total", True)

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        StartMonitor()
    End Sub
    Private Function StartMonitor()
        Dim dispatcherTimer = New Threading.DispatcherTimer()
        AddHandler dispatcherTimer.Tick, AddressOf dispatcherTimer_Tick
        dispatcherTimer.Interval = New TimeSpan(0, 0, 1)
        dispatcherTimer.Start()
        Return True
    End Function

    Private Sub dispatcherTimer_Tick(sender As Object, e As EventArgs)
        Dim value = cpuCounter.NextValue()
        CPUPercent.Content = value.ToString() + "%"
    End Sub
End Class
