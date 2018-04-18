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

Dim classDictionary As New Dictionary(Of String, Dictionary(Of String, String))
Dim addNamespace As Boolean = True
Dim namespaceName As String = "Models.Test"
Sub Main
	Dim chooseFile As New OpenFileDialog()
	chooseFile.Multiselect = False
	chooseFile.DefaultExt = "json"

	If chooseFile.ShowDialog() = DialogResult.OK
		Dim jsonObjStr As String = File.ReadAllText(chooseFile.FileName)
		Dim jsonObj As JObject = JObject.Parse(jsonObjStr)
		Dim className As String = Path.GetFileNameWithoutExtension(chooseFile.FileName)
		CreateModelClass(jsonObj, className)
		'Console.WriteLine(classDictionary)
		PrintClasses()
		'PrintJsonSchema()
	End If
	
End Sub

Private Sub PrintJsonSchema()
	Dim classStr As String = ""

	For Each className As String In classDictionary.Keys
		classStr &= className & ": {" & Environment.NewLine
		For Each prop As String In classDictionary(className).Keys
			classStr &= "    " & prop & " : """ & classDictionary(className)(prop) & """," & Environment.NewLine
		Next
		classStr &= "}" & Environment.NewLine
		classStr &= "-------------------------------------" & Environment.NewLine
	Next

	Console.WriteLine(classStr)
End Sub

Private Sub PrintClasses()
	Dim classStr As String = ""

	For Each className As String In classDictionary.Keys
		If addNamespace Then
			classStr &= "Namespace " & namespaceName & Environment.NewLine
		End If
		classStr &= "Public Class " & className & Environment.NewLine
		For Each prop As String In classDictionary(className).Keys
			classStr &= "    Public Property " & prop & " As " & classDictionary(className)(prop) & Environment.NewLine
		Next
		classStr &= "End Class" & Environment.NewLine
		If addNamespace Then
			classStr &= "End Namespace" & Environment.NewLine
		End If
		classStr &= "-------------------------------------" & Environment.NewLine
	Next

	Console.WriteLine(classStr)
End Sub



Private Sub CreateModelClass(Byval jsonObj As Jobject, Byval className As String)
	Dim propDictionary As New Dictionary(Of String, String)
	If classDictionary.ContainsKey(className) Then
		propDictionary = classDictionary(className)
	End If
	
	For Each p As JProperty In jsonObj.Properties()
		If Not propDictionary.ContainsKey(p.Name) Then
			propDictionary.Add(p.Name, DetermineType(p.Name, p.Value.ToString()))
		End If
	Next

	If classDictionary.ContainsKey(className) Then
		classDictionary(className) = propDictionary
	Else
		classDictionary.Add(className, propDictionary)
	End If
'	Dim modelClass As String = "Public Class " & className & Environment.NewLine
'	For Each p As JProperty In jsonObj.Properties()
'		modelClass &= "    Public Property " & p.Name & " As " & DetermineType(p.Name, p.Value.ToString()) & Environment.NewLine
'	Next
'	modelClass &= "End Class"
'	classDictionary.Add(className, modelClass)
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
			Dim typeName As String = "Object"
			For Each i As JObject In jarrayTest
				typeName = "List(Of " & DetermineType(propName, i.ToString()) & ")"
			Next
			Return typeName
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