Public Class Command

    Private ChatProperties As ChatProperties
    Private Username, Wiki As String
    Private IsMod As Boolean = False
    Private IsAdmin As Boolean = False
    Private Command As String
    Private Arguments As List(Of String) = New List(Of String)

    Sub New(text As String, username As String, wiki As String, isMod As Boolean, isAdmin As Boolean, properties As ChatProperties)
        Me.Username = username
        Me.Wiki = wiki
        Me.ChatProperties = properties
        Me.IsMod = isMod
        Me.IsAdmin = isAdmin
        Dim CommandPieces As String() = text.Split(">")
        Me.Command = CommandPieces(0)
        For i As Integer = 1 To CommandPieces.Length - 1 Step 1
            Me.Arguments.Add(CommandPieces(i))
        Next
    End Sub

    Public Function Execute() As String
        Select Case Me.Command.ToLower()
            Case "!hello"
                Return Me.Cmd_Hello()
            Case "!give"
                Return Me.Cmd_Give()
            Case "!savelog"
                Me.ChatProperties.SaveLog(Me.Wiki)
                Return Nothing
            Case "!define"
                Return Me.Cmd_Define()
            Case "!exit"
                Return Me.Cmd_Exit()
        End Select
        Return Nothing
    End Function

    Private Function Cmd_Hello() As String
        Return "Hello, " & Me.Username & "!"
    End Function

    Private Function Cmd_Give() As String
        If Me.Arguments.Count > 1 Then
            Return "/me gives " & Arguments(0) & " to " & Arguments(1)
        Else
            Return Nothing
        End If
    End Function

    Private Function Cmd_Define() As String
        Dim Definition As MerriamDefiniton = New MerriamDefiniton(Me.Arguments(0))
        If Me.Arguments.Count = 2 Then
            Try
                Return Definition.GetDefinition((CInt(Me.Arguments(1)) - 1))
            Catch ex As Exception
                Return Definition.GetDefinition(0)
            End Try
        End If
        Return Definition.GetDefinition(0)
    End Function

    Private Function Cmd_Exit() As String
        If IsMod Then
            Me.ChatProperties.SetChatExiting(True)
            Return "Exiting..."
        Else : Return Nothing
        End If
    End Function

End Class
