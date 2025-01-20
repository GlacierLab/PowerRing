Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Threading
Imports LibreHardwareMonitor.Hardware

Public Class SelectWindow

    Private GPUList = New List(Of IHardware)
    Private SensorList = New List(Of ISensor)
    Private SelectedSensor As ISensor
    Private _computer As Computer
    Public Async Function PreInit() As Task
        Await Task.Run(Sub()
                           _computer = New Computer With {
                               .IsGpuEnabled = True
                           }
                           _computer.Open()
                           For i As Integer = 0 To _computer.Hardware.Count() - 1
                               If _computer.Hardware(i).HardwareType = HardwareType.GpuNvidia Or _computer.Hardware(i).HardwareType = HardwareType.GpuAmd Or _computer.Hardware(i).HardwareType = HardwareType.GpuIntel Then
                                   GPUList.Add(_computer.Hardware(i))
                               End If
                           Next
                       End Sub)
        GPUName.Items.Clear()
        For i As Integer = 0 To GPUList.Count() - 1
            Dim label = New Label With {
                .Content = GPUList(i).Name + " (" + GPUList(i).Identifier.ToString() + ")"
            }
            GPUName.Items.Add(label)
        Next
        GPUName.SelectedIndex = 0
        Dim dispatcherTimer = New Threading.DispatcherTimer()
        AddHandler dispatcherTimer.Tick, AddressOf dispatcherTimer_Tick
        dispatcherTimer.Interval = New TimeSpan(0, 0, 1)
        dispatcherTimer.Start()
        SupressOnLaunch.IsChecked = My.Settings.SupressOnLaunch
        If IsRunningAsUwp() Then
            LaunchAsAdmin.IsEnabled = False
            LaunchAsAdmin.Content = "MSIX包不支持管理员启动"
            LaunchAsAdmin.ToolTip = "即使能够通过审核，由于MSIX包安装目录不可写，Ring0驱动仍然无法加载，手动使用管理员身份启动也不行"
        Else
            LaunchAsAdmin.IsChecked = My.Settings.LaunchAsAdmin
        End If
    End Function

    Private Sub GPUName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles GPUName.SelectionChanged
        If GPUName.SelectedIndex > -1 And GPUList.Count() > 0 Then
            SensorList = New List(Of ISensor)
            Dim GPU = GPUList(GPUName.SelectedIndex)
            SensorName.Items.Clear()
            GPU.Update()
            For i As Integer = 0 To GPU.Sensors.length() - 1
                If GPU.Sensors(i).SensorType = SensorType.Power Or GPU.Sensors(i).SensorType = SensorType.Load Then
                    SensorList.Add(GPU.Sensors(i))
                End If
            Next
            If SensorList.Count() = 0 Then
                Dim label = New Label With {
                    .Content = "未找到传感器，可能需要管理员权限"
                }
                SensorName.Items.Add(label)
                SensorName.SelectedIndex = 0
                SaveBtn.IsEnabled = False
            Else
                For i As Integer = 0 To SensorList.Count() - 1
                    Dim label = New Label With {
                        .Content = GetSensorTitle(SensorList(i))
                    }
                    SensorName.Items.Add(label)
                Next
                SensorName.SelectedIndex = 0
                SensorName.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub SensorName_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SensorName.SelectionChanged
        If SensorName.SelectedIndex > -1 And SensorList.Count() > 0 Then
            SelectedSensor = SensorList(SensorName.SelectedIndex)
            SaveBtn.IsEnabled = True
        End If
    End Sub

    Private Sub SaveBtn_Click(sender As Object, e As RoutedEventArgs) Handles SaveBtn.Click
        My.Settings.GPUDevice = GPUList(GPUName.SelectedIndex).Name
        My.Settings.Sensor = SelectedSensor.Identifier.ToString()
        My.Settings.GPUSelected = True
        My.Settings.Save()
        My.Settings.SupressOnLaunch = SupressOnLaunch.IsChecked
        My.Settings.LaunchAsAdmin = LaunchAsAdmin.IsChecked
        _computer.Close()
        Dim InitWindow = New Init()
        InitWindow.Show()
        Close()
    End Sub

    Private Sub dispatcherTimer_Tick(sender As Object, e As EventArgs)
        If SelectedSensor IsNot Nothing Then
            GPUList(GPUName.SelectedIndex).Update()
            SensorValue.Content = "传感器读数: " + Math.Round(CDbl(SelectedSensor.Value()), 3).ToString() + If(SelectedSensor.SensorType = SensorType.Load, "%", "W")
        End If
    End Sub

    Private Function GetSensorTitle(sensor As ISensor) As String
        Return sensor.SensorType.ToString() + " - " + sensor.Name + " (" + sensor.Identifier.ToString().Replace(sensor.Hardware.Identifier.ToString(), "") + ")"
    End Function

    Const APPMODEL_ERROR_NO_PACKAGE As Long = 15700L

    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function GetCurrentPackageFullName(ByRef packageFullNameLength As Integer, ByVal packageFullName As StringBuilder) As Integer
    End Function

    Public Shared Function IsRunningAsUwp() As Boolean
        Dim length As Integer = 0
        Dim sb As StringBuilder = New StringBuilder(0)
        Dim result As Integer = GetCurrentPackageFullName(length, sb)
        sb = New StringBuilder(length)
        result = GetCurrentPackageFullName(length, sb)
        Return result <> APPMODEL_ERROR_NO_PACKAGE
    End Function
End Class
