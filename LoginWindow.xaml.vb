Class LoginWindow

    Private WikiLogin As Login

    Private ChatWindow As ChatWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Me.Main_Grid.Children.Add(New LoginUI(Me))
    End Sub

    Public Sub PerformLogin(username As String, password As String, wiki As String, loginUI As LoginUI)
        wiki = wiki.Replace(" ", String.Empty)
        Dim Wikis() As String = wiki.Split(",")
        Me.WikiLogin = New Login(username, password, Wikis(0))
        If Me.WikiLogin.GetResult = "Success" Then
            Me.ChatWindow = New ChatWindow(Me)
            Me.ChatWindow.Show()
            Me.Hide()
            Me.ChatWindow.ConnectToChats(Wikis, username, Me.WikiLogin.GetCookies)
        Else
            loginUI.ResetForLogin()
        End If
    End Sub

    Sub SaveCredentials(p1 As String, p2 As String, p3 As String, p4 As Boolean)
        My.Settings.Username = p1
        Dim Cipher As New Simple3Des(p1)
        Dim EncryptedPassword As String = Cipher.EncryptData(p2)
        My.Settings.Password = EncryptedPassword
        My.Settings.Wikis = p3
        My.Settings.CredentialsSaved = p4
        My.Settings.Save()
    End Sub

End Class