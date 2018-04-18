<Query Kind="VBProgram" />

Sub Main
	Dim testStr = "INBOUND"
	Console.WriteLine(CamelCase(testStr))
End Sub

Private Function CamelCase(Byval inputStr As String) As String
	Return inputStr.Substring(0, 1).ToUpper() & inputStr.Substring(1).ToLower()
End Function
' Define other methods and classes here
