Imports System.Runtime.InteropServices
Imports System.Text

Public Class NativeUtil
    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Public Shared Function GetCurrentPackageFullName(ByRef packageFullNameLength As Integer, ByVal packageFullName As StringBuilder) As Integer
    End Function
End Class
