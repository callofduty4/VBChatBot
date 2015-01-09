Imports System.Net
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class Login

    Private UserAgent As String = My.Resources.Login_UserAgent
    Private Cookies As CookieContainer
    Private Result As String

    Sub New(username As String, password As String, wiki As String)
        Me.Login(username, password, wiki)
    End Sub

    Sub Login(username As String, password As String, wiki As String)
        Dim LoginToken As String = ""
        Dim URL As String = "http://" + wiki + ".wikia.com/api.php?action=login&lgname=" + username + "&lgpassword=" + password + "&format=json"
        Dim LoginViaAPI As HttpWebRequest
        Try
            LoginViaAPI = DirectCast(WebRequest.Create(URL), HttpWebRequest)
            LoginViaAPI.UserAgent = Me.UserAgent
            LoginViaAPI.Method = "POST"
            Me.Cookies = New CookieContainer()
            LoginViaAPI.CookieContainer = Me.Cookies
            Dim APIResponse As HttpWebResponse = DirectCast(LoginViaAPI.GetResponse(), HttpWebResponse)
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As New StreamReader(Response, Encoding.UTF8)
            Dim JSONResponseString As String = ResponseReader.ReadToEnd()
            Dim DecodedResponse As JObject = JObject.Parse(JSONResponseString)
            LoginToken = DecodedResponse("login")("token").ToString()
        Catch e As WebException
            MessageBox.Show(My.Resources.Login_ConnectionError)
            Me.Result = "Failure"
            Return
        Catch e As System.UriFormatException
            MessageBox.Show(My.Resources.Login_InvalidWikiEntry)
            Me.Result = "Failure"
            Return
        End Try
        URL = "http://" + wiki + ".wikia.com/api.php?action=login&lgname=" + username + "&lgpassword=" + password + "&lgtoken=" + LoginToken + "&format=json"
        Try
            LoginViaAPI = DirectCast(WebRequest.Create(URL), HttpWebRequest)
            LoginViaAPI.UserAgent = Me.UserAgent
            LoginViaAPI.Method = "POST"
            LoginViaAPI.CookieContainer = Me.Cookies
            Dim APIResponse As HttpWebResponse = DirectCast(LoginViaAPI.GetResponse(), HttpWebResponse)
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As New StreamReader(Response, Encoding.UTF8)
            Dim JSONResponseString As String = ResponseReader.ReadToEnd()
            Dim DecodedResponse As JObject = JObject.Parse(JSONResponseString)
            Dim Result = DecodedResponse("login")("result").ToString()
            If Result <> "Success" Then
                Dim ErrorMsg As String = Me.ParseLoginError(Result)
                MessageBox.Show(ErrorMsg)
                Me.Result = "Failure"
            Else
                Me.Result = "Success"
            End If
            Debug.WriteLine(Result)
        Catch e As WebException
            MessageBox.Show(My.Resources.Login_ConnectionError)
            Me.Result = "Failure"
            Return
        End Try
    End Sub

    Private Function ParseLoginError(result As String) As String
        Select Case result
            Case "NotExists"
                Return My.Resources.Login_NotExists
            Case "EmptyPass"
                Return My.Resources.Login_EmptyPass
            Case "WrongPass"
                Return My.Resources.Login_WrongPass
            Case "Throttled"
                Return My.Resources.Login_Throttled
            Case "Blocked"
                Return My.Resources.Login_Blocked
            Case "NeedToken"
                Return My.Resources.Login_NeedToken
            Case Else
                Return My.Resources.Login_UnknownError
        End Select
    End Function

    Public Function GetCookies() As CookieContainer
        Return Me.Cookies
    End Function

    Public Function GetResult() As String
        Return Me.Result
    End Function

End Class
