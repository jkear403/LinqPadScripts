<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Collections.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.XML.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xml.Linq.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <NuGetReference>DocumentFormat.OpenXml</NuGetReference>
  <Namespace>DocumentFormat.OpenXml</Namespace>
  <Namespace>DocumentFormat.OpenXml.Packaging</Namespace>
  <Namespace>DocumentFormat.OpenXml.Spreadsheet</Namespace>
  <Namespace>System.Data</Namespace>
  <Namespace>System.Data.Common</Namespace>
  <Namespace>System.Data.Sql</Namespace>
  <Namespace>System.Data.SqlClient</Namespace>
  <Namespace>System.Xml</Namespace>
</Query>

void Main()
{
	try
	{
		CreateSpreadsheet(Path.Combine(@"C:\Tmp\", "Test.xlsx"), TempTable(), false);
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
	}
}

private DataTable TempTable()
{
	DataTable dtTemp = new DataTable();
	const string sqlConnectionString = "Data Source=<SERVER>;initial catalog=<DATABASE>;user id=sa;password=<PASSWORD>;Persist Security Info=true";
	SqlConnection sqlServerConnection = new SqlConnection(sqlConnectionString);

	using (sqlServerConnection)
	{
		sqlServerConnection.Open();
		
		string getAcct = string.Format("SELECT * FROM [dbo].[Users]");

		SqlCommand cmd = new SqlCommand(getAcct, sqlServerConnection);
		SqlDataAdapter da = new SqlDataAdapter(cmd);
		da.Fill(dtTemp);
		
		sqlServerConnection.Close();
	}

	return dtTemp;
}

private void CreateSpreadsheet(string fileName, DataTable table, bool header)
{
	SpreadsheetDocument doc = null;

	try
	{
		doc = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

		WorkbookPart wbPart = doc.AddWorkbookPart();
		wbPart.Workbook = new Workbook();
		wbPart.Workbook.Sheets = new Sheets();

		WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
		SheetData sheetData = new SheetData();
		wsPart.Worksheet = new Worksheet(sheetData);

		Sheets sheets = doc.WorkbookPart.Workbook.GetFirstChild<Sheets>();
		string rID = doc.WorkbookPart.GetIdOfPart(wsPart);
		uint sID = 1;

		if (sheets.Elements<Sheet>().Count() > 0)
		{
			sID = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
		}

		Sheet sheet = new Sheet()
		{
			Id = rID,
			SheetId = sID,
			Name = "Sheet1"
		};
		sheets.Append(sheet);

		Row headerRow = new Row();
		List<string> columns = new List<string>();

		// create header
		if (header)
		{
			foreach (DataColumn column in table.Columns)
			{
				columns.Add(column.ColumnName);
				
				Cell cell = new Cell();
				cell.DataType = CellValues.String;
				cell.CellValue = new CellValue(column.ColumnName);
				headerRow.AppendChild(cell);
			}

			sheetData.AppendChild(headerRow);
		}

		// add rows to spreadsheet
		foreach (DataRow row in table.Rows)
		{
			Row newRow = new Row();

			if (header)
			{
				foreach (string name in columns)
				{
					Cell cell = new Cell();
					switch (row.Table.Columns[name].DataType.ToString())
					{
						case "System.Int32":
							cell.DataType = CellValues.Number;
							break;
						case "System.Boolean":
							cell.DataType = CellValues.Boolean;
							break;
						case "System.DateTime":
							cell.DataType = CellValues.Date;
							break;
						case "System.String":
							cell.DataType = CellValues.String;
							break;
						default:
							cell.DataType = CellValues.String;
							break;
					}
					
					cell.CellValue = new CellValue(row[name].ToString());
					newRow.AppendChild(cell);
				}
			}
			else
			{
				for (int i = 0; i <= row.ItemArray.Count() - 1; i++)
				{

					Cell cell = new Cell();
					switch (row.Table.Columns[i].DataType.ToString())
					{
						case "System.Int32":
							cell.DataType = CellValues.Number;
							break;
						case "System.Boolean":
							cell.DataType = CellValues.Boolean;
							break;
						case "System.DateTime":
							cell.DataType = CellValues.Date;
							break;
						case "System.String":
							cell.DataType = CellValues.String;
							break;
						default:
							cell.DataType = CellValues.String;
							break;
					}
					cell.CellValue = new CellValue(row[i].ToString());
					newRow.AppendChild(cell);
				}
			}

			sheetData.AppendChild(newRow);
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
	}
	finally
	{
		doc.Close();
	}
}
// Define other methods and classes here