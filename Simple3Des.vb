Imports System.Security.Cryptography

Public NotInheritable Class Simple3Des

    Private TripleDes As New TripleDESCryptoServiceProvider

    Sub New(ByVal key As String)
        ' Initialize the crypto provider.
        TripleDes.Key = TruncateHash(key, TripleDes.KeySize \ 8)
        TripleDes.IV = TruncateHash("", TripleDes.BlockSize \ 8)
    End Sub

    Private Function TruncateHash(ByVal key As String, ByVal length As Integer) As Byte()
        Dim sha1 As New SHA1CryptoServiceProvider
        ' Hash the key. 
        Dim keyBytes() As Byte = System.Text.Encoding.Unicode.GetBytes(key)
        Dim hash() As Byte = sha1.ComputeHash(keyBytes)
        ' Truncate or pad the hash. 
        ReDim Preserve hash(length - 1)
        Return hash
    End Function

    Public Function EncryptData(ByVal plaintext As String) As String
        ' Convert the plaintext string to a byte array. 
        Dim PlaintextBytes() As Byte = System.Text.Encoding.Unicode.GetBytes(plaintext)
        ' Create the stream. 
        Dim ms As New System.IO.MemoryStream
        ' Create the encoder to write to the stream. 
        Dim EncStream As New CryptoStream(ms, TripleDes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write)
        ' Use the crypto stream to write the byte array to the stream.
        EncStream.Write(PlaintextBytes, 0, PlaintextBytes.Length)
        EncStream.FlushFinalBlock()
        ' Convert the encrypted stream to a printable string. 
        Return Convert.ToBase64String(ms.ToArray)
    End Function

    Public Function DecryptData(ByVal encryptedtext As String) As String
        ' Get bytes from Base64 string
        Dim EncryptedBytes() As Byte = {}
        Try
            EncryptedBytes = Convert.FromBase64String(encryptedtext)
        Catch ex As System.FormatException
            Return ""
        End Try
        ' Create the stream. 
        Dim ms As New System.IO.MemoryStream
        ' Create the decoder to write to the stream. 
        Dim DecStream As New CryptoStream(ms, TripleDes.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write)
        ' Use the crypto stream to write the byte array to the stream.
        DecStream.Write(EncryptedBytes, 0, EncryptedBytes.Length)
        DecStream.FlushFinalBlock()
        ' Convert the plaintext stream to a string. 
        Return System.Text.Encoding.Unicode.GetString(ms.ToArray)
    End Function

End Class
