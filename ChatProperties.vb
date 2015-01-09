Imports Newtonsoft.Json.Linq
Imports System.IO

Public Class ChatProperties

    Private Users As List(Of String) = New List(Of String)
    Private Mods As List(Of String) = New List(Of String)
    Private Admins As List(Of String) = New List(Of String)
    Private UserRecord As Dictionary(Of Byte, String)
    Private TellRecordUsers As Dictionary(Of UInteger, String)
    Private TellRecordMessages As Dictionary(Of UInteger, String)
    Private ChatLog As List(Of String) = New List(Of String)
    Private SpamTimer As Timers.Timer
    Private IsCommandsEnabled = True
    Private IsLoggingEnabled = True

    Public Sub LoadSettings(wiki As String)
        Dim FileName As String = wiki + "Settings.txt"
        If File.Exists(FileName) Then
            Dim Setting As String = ""
            Dim FileReader As New StreamReader(FileName)
            Do While FileReader.Peek <> -1
                Setting = FileReader.ReadLine()
                Me.ApplySettings(Setting)
            Loop
        Else
            File.Create(FileName).Dispose()
        End If
    End Sub

    Private Sub ApplySettings(setting As String)
        Dim SettingComponents As String() = setting.Split("||")
        Select Case SettingComponents(0)
            Case My.Resources.ChatProperties_CommandsEnabledSettingName
                If SettingComponents(1) = "false" Then
                    Me.IsCommandsEnabled = False
                End If
            Case My.Resources.ChatProperties_LoggingSettingName
                If SettingComponents(1) = "true" Then
                    Me.IsLoggingEnabled = True
                End If
        End Select
    End Sub

    Public Sub SaveSettings(wiki As String)
        Dim FileName As String = wiki + "Settings.txt"
        If File.Exists(FileName) Then
            Dim FileWriter As New StreamWriter(FileName)
            FileWriter.WriteLine(My.Resources.ChatProperties_CommandsEnabledSettingName & "||" & CStr(IsCommandsEnabled).ToLower())
            FileWriter.WriteLine(My.Resources.ChatProperties_LoggingSettingName & "||" & CStr(IsLoggingEnabled).ToLower())
            FileWriter.Close()
        Else
            File.Create(FileName)
            Me.SaveSettings(wiki)
        End If
    End Sub

    Public Sub PopulateList(messageData As JObject, userGroup As String)
        Dim MessageJSON As String = messageData("data").ToString()
        messageData = JObject.Parse(MessageJSON)
        Dim UserListArray As JArray = DirectCast(messageData("collections")("users")("models"), JArray)
        For i As Integer = 0 To UserListArray.Count - 1
            Dim User As String = UserListArray(i)("attrs")("name").ToString()
            If userGroup = "user" Then
                Me.Users.Add(User)
            ElseIf userGroup = "mod" Then
                Dim Moderator As String = UserListArray(i)("attrs")("isModerator").ToString()
                If Moderator = "True" Then
                    Me.Mods.Add(User)
                End If
            ElseIf userGroup = "admin" Then
                Dim Admin As String = UserListArray(i)("attrs")("isCanGiveChatMod").ToString()
                If Admin = "True" Then
                    Me.Admins.Add(User)
                End If
            End If
        Next
    End Sub

    Public Function GetList(userGroup As String) As List(Of String)
        Select Case userGroup
            Case "users"
                Return Me.Users
            Case "mods"
                Return Me.Mods
            Case "admins"
                Return Me.Admins
            Case Else
                Return Nothing
        End Select
    End Function

    Public Sub AddToList(userGroup As String, user As String)
        Select Case userGroup
            Case "users"
                Me.Users.Add(user)
            Case "mods"
                Me.Mods.Add(user)
            Case "admins"
                Me.Admins.Add(user)
        End Select
    End Sub

    Public Sub AddToLog(username As String, usernameOfMod As String, text As String, messageType As Byte)
        'Message Types
        '1 - Normal chat message
        '2 - User joined chat
        '3 - User left chat
        '4 - User kicked from chat
        '5 - User banned from chat
        Dim LogMessage As String = "[" & Date.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") & "] "
        Select Case messageType
            Case 1
                LogMessage = LogMessage & "<" & username & "> " & text
                Exit Select
            Case 2
                LogMessage = LogMessage & "-!- " & username & " has joined Special:Chat"
                Exit Select
            Case 3
                LogMessage = LogMessage & "-!- " & username & " has left Special:Chat"
                Exit Select
            Case 4
                LogMessage = LogMessage & "-!- " & username & " was kicked from Special:Chat by " & usernameOfMod
                Exit Select
            Case 5
                LogMessage = LogMessage & "-!- " & username & " was banned from Special:Chat by " & usernameOfMod
                Exit Select
        End Select
        Me.ChatLog.Add(LogMessage)
    End Sub

    Public Sub SaveLog(wiki As String)
        Dim FileName As String = wiki + "Log.txt"
        If File.Exists(FileName) Then
            Dim FileWriter As New StreamWriter(FileName)
            For Each LogLine As String In Me.ChatLog
                FileWriter.WriteLine(LogLine)
            Next
            FileWriter.Close()
        Else
            File.Create(FileName)
            Me.SaveLog(wiki)
        End If
    End Sub

    Public Function GetLoggingEnabled() As Boolean
        Return IsLoggingEnabled
    End Function

End Class
