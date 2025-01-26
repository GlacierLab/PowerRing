Imports System.Text

Public Class Util

    Const APPMODEL_ERROR_NO_PACKAGE As Long = 15700L

    Public Shared Function IsRunningAsUwp() As Boolean
        Dim length As Integer = 0
        Dim sb As New StringBuilder(0)
        Dim result As Integer = NativeUtil.GetCurrentPackageFullName(length, sb)
        sb = New StringBuilder(length)
        result = NativeUtil.GetCurrentPackageFullName(length, sb)
        Return result <> APPMODEL_ERROR_NO_PACKAGE
    End Function
End Class
