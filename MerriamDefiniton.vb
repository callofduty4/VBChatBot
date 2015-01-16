Imports System.Net
Imports System.Xml
Imports System.IO
Imports System.Text.RegularExpressions

Public Class MerriamDefiniton

    'Merriam-Webster's Elementary Dictionary with Audio (Grades 3-5)

    Private Word As String

    Sub New(word As String)
        Me.Word = word
    End Sub

    Function GetDefinition(defIndex As Integer) As String
        Dim URL As String = "http://www.dictionaryapi.com/api/v1/references/sd2/xml/" & Word.ToLower() & "?key=" & My.Resources.MerriamDefinition_APIKey
        Dim GetMerriamResult As New WebClient()
        GetMerriamResult.Headers.Add("User-Agent", My.Resources.MerriamDefinition_UserAgent)
        Dim ReturnData As String = Nothing
        Try
            Dim RawXML As String = GetMerriamResult.DownloadString(URL)
            Dim Reader As XmlDocument = New XmlDocument()
            Reader.LoadXml(RawXML)
            Dim NodeList As XmlNodeList = Reader.GetElementsByTagName("entry")
            Dim TargetNode As XmlNode = Nothing
            For Each node As XmlNode In NodeList
                If Regex.Replace(node.Attributes("id").Value, "\[.*\]", "") = Word.ToLower() Then
                    TargetNode = node
                    Exit For
                End If
            Next
            If TargetNode IsNot Nothing Then
                For Each node As XmlNode In TargetNode.ChildNodes
                    If node.Name = "def" Then
                        For Each nnode As XmlNode In node.ChildNodes
                            If nnode.Name = "dt" Then
                                If defIndex = 0 Then
                                    ReturnData = Word & nnode.InnerText
                                    Exit For
                                Else
                                    defIndex = defIndex - 1
                                    Continue For
                                End If
                            End If
                        Next
                    End If
                Next
            End If
        Catch
        End Try
        Return ReturnData
    End Function

End Class
