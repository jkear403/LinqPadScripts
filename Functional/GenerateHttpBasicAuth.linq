<Query Kind="VBProgram" />

Sub Main
	Dim authStr As String = "user:password"
	Dim authBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(authStr)
	Dim base64edFile = Convert.ToBase64String(authBytes)
	Console.WriteLine(base64edFile)
End Sub

' Define other methods and classes here