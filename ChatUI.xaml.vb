Public Class ChatUI

    Public Sub CreateTab(wiki As String)
        Dim NewTab As TabItem = New TabItem()
        NewTab.Name = wiki + "_TabItem"
        NewTab.Header = wiki
        Dim LogBox As New RichTextBox
        LogBox.Name = wiki + "_RichTextBox"
        LogBox.IsReadOnly = True
        LogBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        NewTab.Content = LogBox
        Me.Chat_TabControl.Items.Add(NewTab)
    End Sub

    Public Sub ViewFirstTab()
        Me.Chat_TabControl.SelectedIndex = 0
    End Sub

    Public Sub AddToLog(wiki As String, content As String)
        Dim Name = wiki & "_RichTextBox"
        Dim LogBox As RichTextBox = New RichTextBox()
        For Each LogTab As TabItem In Me.Chat_TabControl.Items
            LogBox = DirectCast(LogTab.Content, RichTextBox)
            If LogBox.Name = Name Then
                LogBox.AppendText(content & vbCrLf)
                Exit For
            End If
        Next
    End Sub

End Class
