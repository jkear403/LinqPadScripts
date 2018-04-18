<Query Kind="VBProgram">
  <Reference>&lt;RuntimeDirectory&gt;\System.Text.RegularExpressions.dll</Reference>
</Query>

Sub Main
	Dim runCt As Integer = 1500
	Dim ct As Integer = 0
	Dim doNotCreateList As New List(Of String) From {"N7A", "N7B", "C3", "G62", "N7", "GS", "GE", "ISA", "IEA", "MS3", "LAD", "MEA", "L11", "M7", "B2", "S5", "ST", "SE", "AT8" }
	For Each f As String In System.IO.Directory.GetFiles("C:\Temp\EdiSegments")
		Console.WriteLine(f)
		Dim fileContents As String = System.IO.File.ReadAllText(f)
		'Console.WriteLine(fileContents)
		
		' regex expressions
		'-------------------------------
		' segmentAbbr
		' <font size=\+2>(.*) <\/font>
		'-------------------------------
		Dim segmentAbbr As String = ""
		Dim regex As New Regex("<font size=\+2>(.*) <\/font>", RegexOptions.Multiline)
		Dim regMatches As MatchCollection = regex.Matches(fileContents)
		For Each m As Match In regMatches
			If m.Groups.Count > 1
				segmentAbbr = NormalizeData(m.Groups(1).Value.Trim())
			End If
		Next

		If Not doNotCreateList.Contains(segmentAbbr) Then
			'-------------------------------
			' segmentTitle
			' <font size=\+1>(.*)<\/font>
			'-------------------------------
			Dim segmentTitle As String = ""
			regex = New Regex("<font size=\+1>(.*)<\/font>", RegexOptions.Multiline)
			regMatches = regex.Matches(fileContents)
			For Each m As Match In regMatches
				If m.Groups.Count > 1
					segmentTitle = NormalizeData(m.Groups(1).Value.Trim())
				End If
			Next
			
			'-------------------------------
			' segmentFields
			' <td width=300><a href=".*">(.*)<\/a><\/td>
			'-------------------------------
			Dim fieldList As New List(Of String)
			regex = New Regex("<td width=300><a href=.*>(.*)<\/a><\/td>", RegexOptions.Multiline)
			regMatches = regex.Matches(fileContents)
			For Each m As Match In regMatches
				If m.Groups.Count > 1
					fieldList.Add(NormalizeData(m.Groups(1).Value.Trim()))
				End If
			Next

			CreateClassFile(segmentTitle, fieldList, segmentAbbr)
		End If


		If ct >= runCt Then
			Exit For
		End If
		ct += 1
	Next
End Sub

Private Sub CreateClassFile(Byval className As String, Byval classFields As List(Of String), Byval comment As String)
	Dim addNamespace As Boolean = True
	Dim namespaceName As String = "Models.Edi"
	Dim classStr As String = ""

	If addNamespace Then
		classStr &= "Namespace " & namespaceName & Environment.NewLine
	End If
	classStr &= "    ' " & comment & Environment.NewLine
	classStr &= "    Public Class " & className & Environment.NewLine

	'classFields = CorrectDuplicateFields(classFields)
	For Each prop As String In classFields
		classStr &= "        Public Property " & prop & " As String" & Environment.NewLine
	Next
	classStr &= Environment.NewLine
	classStr &= "        Public Shared Function SegmentAbbr() As String" & Environment.NewLine
	classStr &= "            Return """ & comment & """" & Environment.NewLine
	classStr &= "        End Function" & Environment.NewLine
	classStr &= "    End Class" & Environment.NewLine
	If addNamespace Then
		classStr &= "End Namespace" & Environment.NewLine
	End If
	
	Console.WriteLine(classStr)
	If Not File.Exists("C:\Temp\EdiSegments\VbFiles\" & className & ".vb") Then
		System.IO.File.WriteAllText("C:\Temp\EdiSegments\VbFiles\" & className & ".vb", classStr)	
	End If
	
End Sub

Private Function CorrectDuplicateFields(Byval classFields As List(Of String)) As List(Of String)
	Dim correctFieldList As New List(Of String)

	For i As Integer = 0 To classFields.Count - 1
		correctFieldList.Add(classFields(i))
	Next
	
	For Each prop1 As String In classFields
		Dim correctedFieldCount As Integer = 0
		Dim fieldCt As Integer = 0
		Dim listIndex As String = 0
		For i As Integer = 0 To correctFieldList.Count - 1
			If correctFieldList(i).Contains(prop1) And prop1 <> correctFieldList(i) Then
				correctedFieldCount += 1
			End If

			If correctFieldList(i).Contains(prop1) And prop1 = correctFieldList(i) Then
				fieldCt += 1
				If fieldCt > 2 Then
					Exit For
				End If
			End If

			listIndex += 1
		Next
		
		If fieldCt > 2 Then
			correctedFieldCount += 1
			Dim newFieldName As String = correctFieldList(listIndex) & correctedFieldCount
			correctFieldList(listIndex) = newFieldName
		End If
	Next
	
	Return correctFieldList
End Function

Private Function NormalizeData(ByVal input As String) As String
	input = input.Replace(" ", "")
	input = input.Replace("/", "")
	input = input.Replace("\", "")
	input = input.Replace("-", "")
	input = input.Replace(":", "")
	input = input.Replace("&", "")
	input = input.Replace("(", "")
	input = input.Replace(")", "")
	input = input.Replace(";", "")
	input = input.Replace(".", "")
	input = input.Replace(",", "")
	input = input.Replace("'", "")
	input = input.Replace("BeginningSegmentfor", "")
	input = input.Replace("Identification", "Id")
	input = input.Replace("YesNoConditionor", "")
	input = input.Replace("AssociationofAmericanRailroads", "")
	input = input.Replace("orAppointmentReasonCode", "")
	input = input.Replace("InterlineSettlementSystemStatusActionor", "")
	input = input.Replace("StandardCarrierAlphaCode", "CarrierAlphaCode")
	input = input.Replace("Date", "[Date]")
	input = input.Replace("AcademicGradeor", "")
	input = input.Replace("NationalMotorFreightTransportationAssociation", "LocationName")
	input = input.Replace("StateorProvinceCode", "StateCode")
	input = input.Replace("TypeofPersonalorBusinessAssetCode", "BusinessAssetCode")
	input = input.Replace("NonconformanceResultantResponseCode", "ResultantResponseCode")
	input = input.Replace("ConsolidatedOmnibusBudgetReconciliationActCOBRAQualifyingEventCode", "COBRAQualifyingEventCode")
	input = input.Replace("ImplementationTransactionSetSyntaxErrorCode", "SyntaxErrorCode")
	input = input.Replace("CodeForLicensingCertificationRegistrationorAccreditationAgency", "AccreditationAgencyCode")
	input = input.Replace("CanadianWheatBoardCWBMarketing", "CWBMarketing")
	input = input.Replace("ServicePromotionAllowanceorChargeCode", "ServicePromotionChargeCode")
	input = input.Replace("AcademicFieldofStudyLevelorTypeCode", "AcademicFieldofStudyLevel")
	input = input.Replace("InstitutionalGovernanceorFundingSourceLevelCode", "InstitutionalGovernanceorCode")
	input = input.Replace("CryptographicServiceMessageCSM", "CS")
	input = input.Replace("SampleFrequencyValueperUnitofMeasurementCode", "SampleFrequencyUom")
	Return input
End Function
' Define other methods and classes here
