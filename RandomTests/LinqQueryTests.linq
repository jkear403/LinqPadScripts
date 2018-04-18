<Query Kind="VBProgram">
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Linq.dll</Reference>
  <Namespace>System.Data</Namespace>
  <Namespace>System.Data.Linq</Namespace>
  <Namespace>System.Data.Linq.Mapping</Namespace>
  <Namespace>System.Data.Linq.SqlClient</Namespace>
  <Namespace>System.Data.Linq.SqlClient.Implementation</Namespace>
  <Namespace>System.Data.Sql</Namespace>
</Query>

Sub Main
	Dim ordstatDt = CreateDataTable()
	Dim pickupStart = (From r In ordstatDt.AsEnumerable()
						Order By r.Field(Of DateTime)("ins_date") Descending
						Select r
						Where r.Field(Of String)("status_code") = "ARRSHIP").FirstOrDefault()

	If pickupStart IsNot Nothing
		Console.WriteLine("PickStart=" & pickupStart("ins_date").ToString())
	End If

	Dim pickupEnd = (From r In ordstatDt.AsEnumerable()
					   Order By r.Field(Of DateTime)("ins_date") Descending
					   Select r
					   Where r.Field(Of String)("status_code") = "PICKD").FirstOrDefault()

	If pickupEnd IsNot Nothing
		Console.WriteLine("PickEnd=" & pickupEnd("ins_date").ToString())
	End If

	Dim delStart = (From r In ordstatDt.AsEnumerable()
					 Order By r.Field(Of DateTime)("ins_date") Descending
					 Select r
					 Where r.Field(Of String)("status_code") = "ARRCONS").FirstOrDefault()

	If delStart IsNot Nothing
		Console.WriteLine("DeliveryStart=" & delStart("ins_date").ToString())
	End If

	Dim delEnd = (From r In ordstatDt.AsEnumerable()
					Order By r.Field(Of DateTime)("ins_date") Descending
					Select r
					Where r.Field(Of String)("status_code") = "DELVD").FirstOrDefault()

	If delEnd IsNot Nothing
		Console.WriteLine("DeliveryEnd=" & delEnd("ins_date").ToString())
	End If

	Dim currentPos = (From r In ordstatDt.AsEnumerable()
				  Order By r.Field(Of DateTime)("ins_date") Descending
				  Select r
				  Where r.Field(Of String)("status_code") = "POSITION").FirstOrDefault()

	If currentPos IsNot Nothing
		Console.WriteLine("CurrentPosition=" & currentPos("ins_date").ToString())
	End If
End Sub

Private Function CreateDataTable() As DataTable
	Dim dt As New DataTable()
	Dim col1 As New DataColumn("status_code", Type.GetType("System.String"))
	Dim col2 As New DataColumn("stat_comment", Type.GetType("System.String"))
	Dim col3 As New DataColumn("trip_number", Type.GetType("System.String"))
	Dim col4 As New DataColumn("ins_date", Type.GetType("System.DateTime"))
	Dim col5 As New DataColumn("order_id", Type.GetType("System.Int32"))
	dt.Columns.Add(col1)
	dt.Columns.Add(col2)
	dt.Columns.Add(col3)
	dt.Columns.Add(col4)
	dt.Columns.Add(col5)
	
	Dim row As DataRow
	row = dt.NewRow()
	row(0) = "ASSIGN"
	row(1) = ""
	row(2) = "567756"
	row(3) = DateTime.Parse("8/21/2017 4:11:04 PM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "DISP"
	row(1) = "(No zone match)"
	row(2) = "567756"
	row(3) = DateTime.Parse("8/22/2017 5:50:01 AM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "POSITION"
	row(1) = "[0372930N1205252W]2.32M W of TURLOCK, CA"
	row(2) = "567756"
	row(3) = DateTime.Parse("8/22/2017 5:57:57 AM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "POSITION"
	row(1) = "[0372310N1204303W]0.41M SE of LIVINGSTON, CA"
	row(2) = "567756"
	row(3) = DateTime.Parse("8/22/2017 6:18:04 AM")
	row(4) = 1525004
	dt.Rows.Add(row)
	
	row = dt.NewRow()
	row(0) = "ARRSHIP"
	row(1) = ""
	row(2) = "567756"
	row(3) = DateTime.Parse("8/22/2017 7:55:09 AM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "PICKD"
	row(1) = ""
	row(2) = "567756"
	row(3) = DateTime.Parse("8/22/2017 9:32:14 AM")
	row(4) = 1525004
	dt.Rows.Add(row)
	
	row = dt.NewRow()
	row(0) = "ARRCONS"
	row(1) = ""
	row(2) = "567756"
	row(3) = DateTime.Parse("8/23/2017 9:38:50 AM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "POSITION"
	row(1) = "[0340059N1173221W]8.23M ESE of ONTARIO, CA"
	row(2) = "567756"
	row(3) = DateTime.Parse("8/23/2017 10:48:11 AM")
	row(4) = 1525004
	dt.Rows.Add(row)

	row = dt.NewRow()
	row(0) = "POSITION"
	row(1) = "[0340059N1173221W]8.23M ESE of ONTARIO, CA"
	row(2) = "567756"
	row(3) = DateTime.Parse("8/23/2017 12:07:20 PM")
	row(4) = 1525004
	dt.Rows.Add(row)
'	
'	row = dt.NewRow()
'	row(0) = "DELVD"
'	row(1) = ""
'	row(2) = "567756"
'	row(3) = DateTime.Parse("8/23/2017 1:24:44 PM")
'	row(4) = 1525004
'	dt.Rows.Add(row)
	
	
	Return dt
End Function
	
' Define other methods and classes here
