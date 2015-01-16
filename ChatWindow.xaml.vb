Imports System.Net
Imports System.Threading

Public Class ChatWindow

    Private LoginWindow As LoginWindow
    Private ChatUi As ChatUI = New ChatUI()
    Private Connections As ChatConnectionCollection = New ChatConnectionCollection

    Public Sub New(loginWindow As LoginWindow)
        InitializeComponent()
        Me.Main_Grid.Children.Add(ChatUi)
        Me.LoginWindow = loginWindow
    End Sub

    Public Sub ConnectToChats(wikis As String(), username As String, cookies As CookieContainer)
        For Each Wiki As String In wikis
            Me.Connections.AddConnection(Wiki, username, Me.ChatUi)
            Dim ConnectionThread As Thread = New Thread(Sub()
                                                            Me.Connections.InitiateConnection(Wiki, cookies)
                                                        End Sub)
            ConnectionThread.SetApartmentState(ApartmentState.STA)
            ConnectionThread.Start()
            ChatUi.CreateTab(Wiki)
        Next
        ChatUi.ViewFirstTab()
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        LoginWindow.Close()
        End
    End Sub

End Class
