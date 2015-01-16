Public Class LoginUI

    Private MainUI As LoginWindow

    Sub New(mainWindow As LoginWindow)
        InitializeComponent()
        MainUI = mainWindow
        If My.Settings.CredentialsSaved Then
            Me.Username_TextBox.Text = My.Settings.Username
            Dim Cipher As New Simple3Des(My.Settings.Username)
            Try
                Dim plainText As String = Cipher.DecryptData(My.Settings.Password)
                Me.Password_PasswordBox.Password = plainText
            Catch ex As System.Security.Cryptography.CryptographicException
            End Try
            Me.Wiki_TextBox.Text = My.Settings.Wikis
            Me.SaveCredentials_CheckBox.IsChecked = True
        End If
    End Sub

    Public Sub ResetForLogin()
        Me.Login_Button.Content = My.Resources.LoginUI_LoginText
    End Sub

    Private Sub Username_TextBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Username_TextBox.GotFocus
        If Me.Username_TextBox.Text = My.Resources.LoginUI_DefaultUsername Then
            Me.Username_TextBox.Text = ""
        End If
    End Sub

    Private Sub Password_PasswordBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Password_PasswordBox.GotFocus
        If Me.Password_PasswordBox.Password = My.Resources.LoginUI_DefaultPassword Then
            Me.Password_PasswordBox.Password = ""
        End If
    End Sub

    Private Sub Wiki_TextBox_GotFocus(sender As Object, e As RoutedEventArgs) Handles Wiki_TextBox.GotFocus
        If Me.Wiki_TextBox.Text = My.Resources.LoginUI_DefaultWiki Then
            Me.Wiki_TextBox.Text = ""
        End If
    End Sub

    Private Sub Username_TextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Username_TextBox.LostFocus
        If Me.Username_TextBox.Text = "" Then
            Me.Username_TextBox.Text = My.Resources.LoginUI_DefaultUsername
        End If
    End Sub

    Private Sub Password_PasswordBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Password_PasswordBox.LostFocus
        If Me.Password_PasswordBox.Password = "" Then
            Me.Password_PasswordBox.Password = My.Resources.LoginUI_DefaultPassword
        End If
    End Sub

    Private Sub Wiki_TextBox_LostFocus(sender As Object, e As RoutedEventArgs) Handles Wiki_TextBox.LostFocus
        If Me.Wiki_TextBox.Text = "" Then
            Me.Wiki_TextBox.Text = My.Resources.LoginUI_DefaultWiki
        End If
    End Sub

    Private Sub Login_Button_Click(sender As Object, e As RoutedEventArgs) Handles Login_Button.Click
        If Me.SaveCredentials_CheckBox.IsChecked Then
            Me.MainUI.SaveCredentials(Me.Username_TextBox.Text, Me.Password_PasswordBox.Password, Me.Wiki_TextBox.Text, True)
        Else
            Me.MainUI.SaveCredentials(Nothing, Nothing, Nothing, False)
        End If
        Me.Login_Button.Content = My.Resources.LoginUI_LoggingInText
        Me.MainUI.PerformLogin(Me.Username_TextBox.Text, Me.Password_PasswordBox.Password, Me.Wiki_TextBox.Text, Me)
    End Sub

End Class
