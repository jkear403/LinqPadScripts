<Query Kind="Program" />

void Main()
{
	Console.WriteLine(DateTime.Now.ToUniversalTime().ToString());
	Console.WriteLine(ConvertGMTToLocal(DateTime.Now.ToUniversalTime()));
}

private DateTime ConvertGMTToLocal(DateTime gmtTime)
{
	DateTime localDateTime = new DateTime();

	try
	{
		//string timeZone = SpireonIntegration.Data.Settings.FindSettingValueByName(Guid.Empty.ToString(), "SpireonIntegration.TmsTimeZone", newSession);
		//string daylightSavings = SpireonIntegration.Data.Settings.FindSettingValueByName(Guid.Empty.ToString(), "SpireonIntegration.TmsDaylightSaving", newSession);
		string timeZone = "Pacific Standard Time";
		string daylightSavings = "";
		
		DateTime utcDateTime = gmtTime.ToUniversalTime();

		TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
		localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);
	}
	catch (Exception ex)
	{
		Console.WriteLine(DateTime.Now.ToString() + "-" + "ConvertGMTToLocal() Error: " + ex.Message);
	}


	return localDateTime;
}
// Define other methods and classes here
