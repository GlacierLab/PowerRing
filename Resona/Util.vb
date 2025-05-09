﻿Imports System.Text

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

    Public Shared Function GetTimestamp() As Long
        Return Date.UtcNow.Subtract(New DateTime(1970, 1, 1)).TotalSeconds
    End Function

    Public Shared Function SaveFileDialog(Type As String, DefaultName As String) As String
        Dim dlg As New Microsoft.Win32.SaveFileDialog()
        dlg.FileName = DefaultName
        dlg.DefaultExt = "." + Type
        dlg.Filter = Type + " files (*." + Type + ")|*." + Type

        Dim result? As Boolean = dlg.ShowDialog()
        While Not result
            dlg.ShowDialog()
        End While
        Return dlg.FileName
    End Function
End Class
