Imports System.IO
Imports System.Text

Public Class Recorder

    Structure Record
        Dim CPU As Double
        Dim GPU As Double
        Dim InSupress As Boolean
        Dim SupressCounter As Integer
        Dim Time As Long
    End Structure

    Private Records As New List(Of Record)



    Public Sub AddRecord(CPU As Double, GPU As Double, InSupress As Boolean, SupressCounter As Integer)
        Records.Add(New Record() With {
            .CPU = CPU,
            .GPU = GPU,
            .InSupress = InSupress,
            .SupressCounter = SupressCounter,
            .Time = Util.GetTimestamp()
                    })
    End Sub

    Public Sub WriteCSV(FileStream As FileStream)
        Dim TempString As String
        TempString = "CPU,GPU,InSupress,SupressCounter,Time" + Environment.NewLine
        FileStream.Write(New UTF8Encoding(True).GetBytes(TempString))
        For Each record In Records
            TempString = String.Join(",", {record.CPU, record.GPU, record.InSupress, record.SupressCounter, record.Time}) + Environment.NewLine
            FileStream.Write(New UTF8Encoding(True).GetBytes(TempString))
        Next
    End Sub
End Class
