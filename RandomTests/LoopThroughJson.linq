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
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

Sub Main
	Dim chooseFile As New OpenFileDialog()
	chooseFile.Multiselect = False
	chooseFile.DefaultExt = "json"

	If chooseFile.ShowDialog() = DialogResult.OK
		Dim jsonStr As String = File.ReadAllText(chooseFile.FileName)
		Dim jsonObj As JObject = JObject.Parse(jsonStr)

		JsonLoop(jsonObj)
	End IF

End Sub

Private Sub JsonLoop(Byval jsonObj As JObject)
	Dim flattenedObj As New Dictionary(Of String, Object)
	For Each p As JProperty In jsonObj.Properties()
		Dim jobjectTest As JObject
		Dim isJObject As Boolean = False
		Try
			jobjectTest = JObject.Parse(p.Value.ToString())
			isJObject = True
			JsonLoop(jobjectTest)
		Catch ex As Exception
			Console.WriteLine(ex.Message)
		End Try

		Dim jarrayTest As JArray
		Dim isJArray As Boolean = False
		If isJObject = False
			Try
				jarrayTest = JArray.Parse(p.Value.ToString())
				isJArray = True
				For Each o As JObject In jarrayTest
					JsonLoop(o)
				Next
			Catch

			End Try
		End If

		If isJObject = False And isJArray = False
			Console.WriteLine(p.Name & "=" & p.Value.ToString())
		End IF
		
	Next
End Sub
' Define other methods and classes here