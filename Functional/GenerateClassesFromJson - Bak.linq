<Query Kind="VBProgram">
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

Dim classDictionary As New Dictionary(Of String, String)

Sub Main
	Dim chooseFile As New OpenFileDialog()
	chooseFile.Multiselect = False
	chooseFile.DefaultExt = "json"

	If chooseFile.ShowDialog() = DialogResult.OK
		Dim jsonObjStr As String = File.ReadAllText(chooseFile.FileName)
		Dim jsonObj As JObject = JObject.Parse(jsonObjStr)
		Dim className As String = Path.GetFileNameWithoutExtension(chooseFile.FileName)
		CreateModelClass(jsonObj, className)
		Console.WriteLine(classDictionary)
	End If
	
End Sub

Private Sub CreateModelClass(Byval jsonObj As Jobject, Byval className As String)
	Dim modelClass As String = "Public Class " & className & Environment.NewLine
	For Each p As JProperty In jsonObj.Properties()
		modelClass &= "    Public Property " & p.Name & " As " & DetermineType(p.Name, p.Value.ToString()) & Environment.NewLine
	Next
	modelClass &= "End Class"
	classDictionary.Add(className, modelClass)
End Sub

Private Function DetermineType(Byval propName As String, Byval val As String) As String
	Dim decimalTest As Decimal
	Dim booleanTest As Boolean
	Dim doubleTest As Double
	Dim integerTest As Integer
	Dim dateTest As DateTime
	Dim jobjectTest As JObject
	Dim jarrayTest As JArray

	If Double.TryParse(val, doubleTest) AndAlso val.Contains(".") Then
		Return "Double"
	End If
	
	If Decimal.TryParse(val, decimalTest) AndAlso val.Contains(".") Then
		Return "Decimal"
	End If

	If Integer.TryParse(val, integerTest) Then
		Return "Integer"
	End If

	If DateTime.TryParse(val, dateTest) Then
		Return "DateTime"
	End If

	Try
		jobjectTest = JObject.Parse(val)
		If jobjectTest("$date") IsNot Nothing Then
			Return DetermineType(propName, jobjectTest("$date").ToString())
		Else
			CreateModelClass(jobjectTest, propName.Substring(0, 1).ToUpper() & propName.Substring(1))
			Return propName.Substring(0, 1).ToUpper() & propName.Substring(1)
		End If
	Catch

	End Try

	Try
		jarrayTest = JArray.Parse(val)
		If jarrayTest.Count > 0 Then
			Return "List(Of " & DetermineType(propName, jarrayTest(0).ToString()) & ")"
		Else
			Return "List(Of Object)"
		End If	
	Catch

	End Try

	If Boolean.TryParse(val, booleanTest) Then
		Return "Boolean"
	End If
	
	Return "String"
End Function
' Define other methods and classes here
