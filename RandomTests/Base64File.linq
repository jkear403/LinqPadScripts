<Query Kind="VBProgram" />

Sub Main
	Dim fileToConvert As Byte() = System.IO.File.ReadAllBytes("C:\Temp\TestImages\W2nd89KWAjdrDh28w-2017-12-28-a1fd-c.jpg")
	Dim base64edFile = Convert.ToBase64String(fileToConvert)
	Console.WriteLine(base64edFile)
End Sub

' Define other methods and classes here