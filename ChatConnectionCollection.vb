Imports System.Net

Public Class ChatConnectionCollection

    Private ChatConnections As New Dictionary(Of String, ChatConnection)

    Public Sub AddConnection(wiki As String, username As String, UI As ChatUI)
        Me.ChatConnections.Add(wiki, New ChatConnection(username, wiki, UI))
    End Sub

    Public Sub RemoveConnection(wiki As String)
        Me.ChatConnections.Remove(wiki)
    End Sub

    Public Sub InitiateConnection(wiki As String, cookies As CookieContainer)
        Me.ChatConnections(wiki).Initiate(cookies)
    End Sub
End Class
