<Query Kind="VBProgram">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

Sub Main
	Dim testObj As New JObject()
	testObj("field1") = "Test"
	Console.WriteLine(testObj("field1"))
	Console.WriteLine(testObj("field2").Value(Of Nullable(Of Boolean)))
	
	Dim newList As New List(Of String)
	newList.Add("Status1")
	newList.Add("Status2")
	newList.Add("Status3")
	newList.Add("Status4")
	newList.Add("Status5")
	newList.Add("Status6")
	
	Dim listStr As String = Newtonsoft.Json.JsonConvert.SerializeObject(newList)
	Console.WriteLine(listStr)
	
	Dim listTest As List(Of String) = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of String))(listStr)
	
	listTest.Dump()
End Sub

' Define other methods and classes here
