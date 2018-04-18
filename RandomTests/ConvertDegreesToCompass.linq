<Query Kind="VBStatements" />

Dim compassValues As New List(Of String) From {"N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N"}
Dim directionDegrees As Integer = 205
Dim calcDegrees As Integer = (directionDegrees + 360) MOD 360
Console.WriteLine(calcDegrees)
Dim compassIndex As Integer = Math.Round(calcDegrees / 22.5)
Console.WriteLine(compassIndex)
Console.WriteLine(compassValues(compassIndex))