﻿Imports System.Net
Imports System.IO
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.ComponentModel
Imports System.Threading
Imports System.Text.RegularExpressions

Public Class ChatConnection

    Private ChatUI As ChatUI
    Private Username, Wiki As String
    Private UserAgent As String = My.Resources.ChatConnection_UserAgent
    Private ChatKey, ChatServer, ChatServerID, ChatPort, ChatRoomID, ChatSession, ChatURL As String
    Private PingInterval, CommandsUsed As UInteger
    Private ConstantConnection As BackgroundWorker = New BackgroundWorker()
    Private Cookies As CookieContainer
    Private ChatProperties As ChatProperties = New ChatProperties()

    Sub New(username, wiki, UI)
        Me.ChatUI = UI
        Me.Username = username
        Me.Wiki = wiki
        Me.ConstantConnection.WorkerSupportsCancellation = True
        AddHandler Me.ConstantConnection.DoWork, AddressOf Me.ConstantConnection_DoWork
        AddHandler Me.ConstantConnection.RunWorkerCompleted, AddressOf Me.ConstantConnection_RunWorkerCompleted
    End Sub

    Private Function GetUNIXTime() As String
        Dim ChatTimespan As TimeSpan = (DateTime.UtcNow - New DateTime(1970, 1, 1))
        Return CStr(ChatTimespan.TotalMilliseconds)
    End Function

    Public Sub Initiate(cookies As CookieContainer)
        ChatProperties.SetChatQuitting(False)
        ChatProperties.SetChatExiting(False)
        Me.Cookies = cookies
        Debug.WriteLine("Getting chat info")
        GetChatInfo()
        Debug.WriteLine("Chat info obtained")
        If Not ChatProperties.GetChatQuitting() Then
            Debug.WriteLine("Get chat key")
            MakeConnection()
            Debug.WriteLine("Chat key obtained")
            If Not ChatProperties.GetChatQuitting() Then
                Debug.WriteLine("Starting connection")
            Else
                OnFailedConnection()
            End If
        Else
            OnFailedConnection()
        End If
    End Sub

    Private Sub GetChatInfo()
        Dim URL As String = "http://" + Me.Wiki + ".wikia.com/wikia.php?controller=Chat&format=json"
        Dim GetChatInfo As HttpWebRequest = DirectCast(WebRequest.Create(URL), HttpWebRequest)
        GetChatInfo.UserAgent = Me.UserAgent
        GetChatInfo.Method = "POST"
        GetChatInfo.CookieContainer = Me.Cookies
        Try
            Dim APIResponse As HttpWebResponse = DirectCast(GetChatInfo.GetResponse(), HttpWebResponse)
            Dim Response As Stream = APIResponse.GetResponseStream()
            Dim ResponseReader As New StreamReader(Response, Encoding.UTF8)
            Dim ChatInfo As String = ResponseReader.ReadToEnd()
            ParseChatInfo(ChatInfo)
        Catch e As Exception
            'TODO deal with error in getting chat info
            ChatProperties.SetChatQuitting(True)
        End Try
    End Sub

    Private Sub ParseChatInfo(UnparsedInfo As String)
        Dim ChatInfo As JObject = JObject.Parse(UnparsedInfo)
        Me.ChatRoomID = ChatInfo("roomId").ToString()
        Me.ChatKey = ChatInfo("chatkey").ToString()
        Me.ChatServer = ChatInfo("nodeHostname").ToString()
        Me.ChatServerID = ChatInfo("nodeInstance").ToString()
        Me.ChatPort = ChatInfo("nodePort").ToString()
        'TODO take care of notes and logging setup
    End Sub

    Private Sub MakeConnection()
        Me.ChatURL = "http://" + Me.ChatServer + ":" + Me.ChatPort + "/socket.io/1/?name=" + Me.Username + "&key=" + Me.ChatKey + "&roomId=" + Me.ChatRoomID + "&serverId=" + Me.ChatServerID + "&EIO=2&transport=polling"
        Me.GetChatSession()
        Me.ChatURL = "http://" + Me.ChatServer + ":" + Me.ChatPort + "/socket.io/1/?name=" + Me.Username + "&key=" + Me.ChatKey + "&roomId=" + Me.ChatRoomID + "&serverId=" + Me.ChatServerID + "&EIO=2&transport=polling&sid=" + Me.ChatSession
        Me.ConstantConnection.RunWorkerAsync()
    End Sub

    Private Sub GetChatSession()
        Dim GetChatInfo As HttpWebRequest = DirectCast(WebRequest.Create(ChatURL + "&t=" + GetUNIXTime()), HttpWebRequest)
        GetChatInfo.UserAgent = Me.UserAgent
        GetChatInfo.Method = "GET"
        GetChatInfo.CookieContainer = Me.Cookies
        Try
            Dim APIResponse As HttpWebResponse = DirectCast(GetChatInfo.GetResponse(), HttpWebResponse)
            Dim APIResponseStream As Stream = APIResponse.GetResponseStream()
            Dim APIReadStream As New StreamReader(APIResponseStream, Encoding.ASCII)
            Dim APIResponseString = APIReadStream.ReadToEnd()
            APIResponseString = Regex.Replace(APIResponseString, ".*0{", "{")
            Dim APIResponseData As JObject = JObject.Parse(APIResponseString)
            Me.ChatSession = APIResponseData("sid").ToString()
            Me.PingInterval = CInt(APIResponseData("pingInterval"))
        Catch e As Exception
            'TODO deal with error in getting chat session
            ChatProperties.SetChatQuitting(True)
        End Try
    End Sub

    Private Sub MakeConstantConnection()
        While Not (ChatProperties.GetChatQuitting() Or ChatProperties.GetChatExiting())
            Me.SendXHR()
            Debug.WriteLine(DateTime.UtcNow.ToShortTimeString + " Sending now")
        End While
        OnDisconnection()
    End Sub

    Private Sub SendXHR()
        Dim NewChatURL = ChatURL & "&t=" & GetUNIXTime()
        Dim OpenChat As HttpWebRequest = DirectCast(WebRequest.Create(NewChatURL), HttpWebRequest)
        OpenChat.UserAgent = My.Resources.ChatConnection_UserAgent
        OpenChat.Method = "GET"
        OpenChat.ContentType = "application/octet-stream"
        OpenChat.KeepAlive = True
        Try
            Dim ChatResponse As HttpWebResponse = DirectCast(OpenChat.GetResponse(), HttpWebResponse)
            Dim ChatReceiveStream As Stream = ChatResponse.GetResponseStream()
            Dim ChatReadStream As New StreamReader(ChatReceiveStream, Encoding.UTF8)
            Dim rgx As New Regex("(�|�)")
            Dim ChatResponseString As String = ChatReadStream.ReadToEnd()
            Dim ChatResponseStrings As String() = rgx.Split(ChatResponseString)
            For Each chatString As String In ChatResponseStrings
                ClassifyChatString(chatString)
            Next
            ChatResponse.Close()
            ChatReceiveStream.Close()
        Catch e As Exception
            Debug.WriteLine("SendXHR: " + e.Message)
            ChatProperties.SetChatQuitting(True)
        End Try
    End Sub

    Private Sub ClassifyChatString(chatString As String)
        If chatString.StartsWith("40") Then
            ChatUI.Dispatcher.BeginInvoke(Sub()
                                              ChatUI.AddToLog(Me.Wiki, chatString)
                                          End Sub)
            Dim ChatJoin As New Thread(AddressOf OnChatConnect)
            ChatJoin.Start()
        End If
        If chatString.StartsWith("42") Then
            ChatUI.Dispatcher.BeginInvoke(Sub()
                                              ChatUI.AddToLog(Me.Wiki, chatString)
                                          End Sub)
            Dim LookAtNewMessage As New Thread(Sub()
                                                   OnEvent(chatString.Substring(2))
                                               End Sub)
            LookAtNewMessage.Start()
        End If
    End Sub

    Private Sub SendDataToChat(data As String)
        Dim DataHeader As List(Of Byte) = New List(Of Byte)
        DataHeader.Add(0)
        Dim DataLength As String = CStr(data.Length)
        For Each c As Char In DataLength
            DataHeader.Add(Val(c))
        Next
        DataHeader.Add(255)
        Dim DataHeaderArray As Byte() = DataHeader.ToArray()
        Dim DataByteArray As Byte() = Encoding.UTF8.GetBytes(data)
        Dim BytesToSend As Byte() = DataHeaderArray.Concat(DataByteArray).ToArray()
        Try
            Dim NewChatURL = ChatURL & "&t=" & GetUNIXTime()
            Dim DataToSend As HttpWebRequest = DirectCast(WebRequest.Create(NewChatURL), HttpWebRequest)
            DataToSend.Method = "POST"
            DataToSend.ContentType = "application/octet-stream"
            DataToSend.ContentLength = BytesToSend.Length
            Dim DataStream As Stream = DataToSend.GetRequestStream()
            DataStream.Write(BytesToSend, 0, BytesToSend.Length)
            DataStream.Close()
            Dim DataResponse As HttpWebResponse = DirectCast(DataToSend.GetResponse(), HttpWebResponse)
            DataStream.Close()
            DataResponse.Close()
        Catch e As Exception
            Debug.WriteLine("SendDataToChat: " + e.Message)
        End Try
    End Sub

    Private Sub OnChatConnect()
        Debug.WriteLine("Requesting initial data")
        SendDataToChat("42[""message"",""{\""id\"":null,\""attrs\"":{\""msgType\"":\""command\"",\""command\"":\""initquery\""}}""]")
    End Sub

    Private Sub SendMessage(message As String)
        If message = Nothing Then
            Return
        End If
        SendDataToChat("42[""message"",""{\""id\"":null,\""attrs\"":{\""msgType\"":\""chat\"",\""text\"":\""" + message + "\""}}""]")
    End Sub

    Private Sub PingChat()
        Dim PingThread As New Thread(Sub()
                                         SendDataToChat("2")
                                         Thread.Sleep(PingInterval)
                                         PingChat()
                                     End Sub)
        PingThread.Start()
    End Sub

    Private Sub OnEvent(chatResponseString As String)
        Dim JsonDecoder = New JsonSerializer()
        Try
            Dim MessageDataArray As JArray = JArray.Parse(chatResponseString)
            Dim MessageData As JObject = JObject.Parse(MessageDataArray.Last.ToString())
            Dim [Event] As String = MessageData("event").ToString()
            ' Determine what type of event
            Select Case [Event]
                Case ("initial")
                    ' Data received on connection to chat after OnChatConnect() called
                    PingChat()
                    ChatProperties.PopulateList(MessageData, "user")
                    ChatProperties.PopulateList(MessageData, "mod")
                    ChatProperties.PopulateList(MessageData, "admin")
                    Exit Select
                Case ("join")
                    ' Data received when a user joins chat
                    OnJoin(MessageData)
                    Exit Select
                Case ("part"), ("logout")
                    ' Data received when a user exits chat
                    OnPart(MessageData)
                    Exit Select
                Case ("chat:add")
                    ' Data received when a post is made
                    OnMessage(MessageData)
                    Exit Select
                Case ("kick")
                    ' Data received when a kick is made
                    OnKick(MessageData)
                Case ("ban")
                    ' Data received when a ban is made
                    OnBan(MessageData)
                    Exit Select
            End Select
            ' Not really any need to deal with any exception... 
        Catch
        End Try
    End Sub

    Private Sub OnJoin(messageData As JObject)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim User As String = messageData("attrs")("name").ToString()
        If Not ChatProperties.GetList("users").Contains(User) Then
            ChatProperties.AddToList("users", User)
            Dim Moderator As String = messageData("attrs")("isModerator").ToString()
            Dim Admin As String = messageData("attrs")("isCanGiveChatMod").ToString()
            If Moderator = "True" AndAlso Not ChatProperties.GetList("mods").Contains(User) Then
                ChatProperties.AddToList("mods", User)
            End If
            If Admin = "True" AndAlso Not ChatProperties.GetList("admins").Contains(User) Then
                ChatProperties.AddToList("admins", User)
            End If
            ChatProperties.AddToLog(User, Nothing, Nothing, Nothing, 2)
        End If
    End Sub

    Private Sub OnPart(messageData As JObject)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim User As String = messageData("attrs")("name").ToString()
        If ChatProperties.GetList("users").Contains(User) Then
            ChatProperties.RemoveFromList("users", User)
            If ChatProperties.GetList("mods").Contains(User) Then
                Me.ChatProperties.RemoveFromList("mods", User)
            End If
            If ChatProperties.GetList("admins").Contains(User) Then
                ChatProperties.RemoveFromList("admins", User)
            End If
            ChatProperties.AddToLog(User, Nothing, Nothing, Nothing, 3)
        End If
    End Sub

    Private Sub OnMessage(messageData As JObject)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim Text As String = messageData("attrs")("text").ToString()
        Dim Name As String = messageData("attrs")("name").ToString()
        Dim IsModerator As Boolean = False
        Dim IsAdmin As Boolean = False
        If Me.ChatProperties.GetList("mods").Contains(Name) Then
            IsModerator = True
        End If
        If Me.ChatProperties.GetList("admins").Contains(Name) Then
            IsAdmin = True
        End If
        If ChatProperties.GetLoggingEnabled = True Then
            ChatProperties.AddToLog(Name, "", Text, Nothing, 1)
        End If
        'TODO Tell record
        If Text(0) = "!"c AndAlso Name <> Username Then
            Dim Chat_Command As New Thread(Sub()
                                               Dim NewCommand As Command = New Command(Text, Name, Me.Wiki, IsModerator, IsAdmin, ChatProperties)
                                               Dim TextToSend As String = NewCommand.Execute()
                                               SendMessage(TextToSend)
                                           End Sub)
            Chat_Command.Start()
        End If
    End Sub

    Private Sub OnKick(messageData As JObject)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim KickedUser As String = messageData("attrs")("kickedUserName").ToString()
        Dim Moderator As String = messageData("attrs")("moderatorName").ToString()
        ChatProperties.AddToLog(KickedUser, Moderator, Nothing, Nothing, 4)
    End Sub

    Private Sub OnBan(messageData As JObject)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim KickedUser As String = messageData("attrs")("kickedUserName").ToString()
        Dim Moderator As String = messageData("attrs")("moderatorName").ToString()
        Dim Time As String = messageData("attrs")("time").ToString()
        ChatProperties.AddToLog(KickedUser, Moderator, Nothing, Time, 5)
    End Sub

    Private Sub OnFailedConnection()
        Throw New NotImplementedException
    End Sub

    Private Sub OnDisconnection()
        If ChatProperties.GetChatQuitting() Then
            Initiate(Cookies)
            ' TODO add to log that bot is restarting
        ElseIf ChatProperties.GetChatExiting Then
            SendDataToChat("42[""message"",""{\""id\"":null,\""attrs\"":{\""msgType\"":\""command\"",\""command\"":\""logout\""}}""]")
            ' TODO add to log that bot was quit by mod
        End If
    End Sub

    Private Sub ConstantConnection_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs)
        Me.MakeConstantConnection()
    End Sub

    Private Sub ConstantConnection_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs)
        Me.OnDisconnection()
    End Sub

End Class
