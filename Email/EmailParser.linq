<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>Sendgrid</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Bson</Namespace>
  <Namespace>Newtonsoft.Json.Converters</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>Newtonsoft.Json.Schema</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
  <Namespace>SendGrid</Namespace>
  <Namespace>SendGrid.Helpers.Mail</Namespace>
  <Namespace>SendGrid.Helpers.Mail.Model</Namespace>
  <Namespace>SendGrid.Helpers.Reliability</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static HashSet<string> firstNames = new HashSet<string>();
static HashSet<string> lastNames = new HashSet<string>();
static string[] possibleGreetings = new string[] { "greetings", "greeting", "dear", "hey", "hi", "all", "morning", "evening", "afternoon", "good" };
static string[] possibleEndings = new string[] { "best", "regards", "thanks", "wishes", "kind", "fond", "sincerely", "thank", "appreciation", "yours", "gratitude", "you" };
static Dictionary<string, AiTask> vivianTasks = new Dictionary<string, AiTask>();

void Main()
{
	//Console.WriteLine("Starting with PreLoad - " + DateTime.Now.ToString());
	PrePopulateData();
	//Console.WriteLine("Done with PreLoad - " + DateTime.Now.ToString());
	var vivianTasksStr = JsonConvert.SerializeObject(vivianTasks);
	//Console.WriteLine(vivianTasksStr);

	var listenerAlive = true;
	var l = new System.Net.HttpListener();
	l.Prefixes.Add("http://+:2207/parseemail/");
	l.Start();

	Console.WriteLine("HTTP Listener Open on Port 2207");
	while (listenerAlive)
	{
		try
		{
			System.Net.HttpListenerContext context = l.GetContext();
			System.Net.HttpListenerContext tmpContext = context;
			Task t = Task.Factory.StartNew(() => ParseEmail(tmpContext), TaskCreationOptions.LongRunning);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
			listenerAlive = false;
		}

	}

	l.Stop();
}

private void PrePopulateData()
{
	// Load FirstNames
	foreach (string f in File.ReadLines(@"C:\Base\Resources\FirstNameDB.csv"))
	{
		firstNames.Add(f);
	}

	// Load LastNames
	foreach (string l in File.ReadLines(@"C:\Base\Resources\LastNameDB.csv"))
	{
		lastNames.Add(l);
	}

	// Load Tasks
	var vivianTasksStr = File.ReadAllText(@"C:\Base\Resources\Tasks.json");
	vivianTasks = JsonConvert.DeserializeObject<Dictionary<string, AiTask>>(vivianTasksStr);
}

private void ParseEmail(System.Net.HttpListenerContext emailContext)
{
	Console.WriteLine("Connected @ " + DateTime.Now.ToString());
	var request = emailContext.Request;
	Dictionary<string, string> emailSections = new Dictionary<string, string>();
	var response = emailContext.Response;
	var responseString = "";
	request.HttpMethod.Dump();
	if (request.RawUrl.ToLower().Contains("/parseemail"))
	{
		var s = request.InputStream;
		var sr = new System.IO.StreamReader(s);
		//sr.ReadToEnd().Dump();
		string line = sr.ReadLine();
		string section = "";
		string sectionKey = "";

		while (line != null)
		{

			if (line.Trim() == "--xYzZY")
			{
				emailSections.Add(sectionKey, section.TrimStart());
				section = "";
				sectionKey = "";
			}
			else
			{
				if (line.StartsWith("Content-Disposition:"))
				{
					int startIndex = line.LastIndexOf("name=\"") + 6;
					int keyLength = line.LastIndexOf("\"") - startIndex;
					sectionKey = line.Substring(startIndex, keyLength);
				}
				else
				{
					section += line + Environment.NewLine;
				}

			}

			line = sr.ReadLine();
		}
		responseString = Newtonsoft.Json.JsonConvert.SerializeObject("");
	}
	else
	{
		responseString = Newtonsoft.Json.JsonConvert.SerializeObject("invalid");
	}

	response.AppendHeader("Access-Control-Allow-Origin", "*");
	var buff = System.Text.Encoding.UTF8.GetBytes(responseString);
	response.ContentLength64 = buff.Length;
	request.Cookies.Dump();
	response.Cookies.Add(new System.Net.Cookie("ListenCookie", "CookieAdded @ " + DateTime.Now.ToString()));
	var outStream = response.OutputStream;

	outStream.Write(buff, 0, buff.Length);
	outStream.Close();

	if (emailSections.Count > 0)
	{
		Console.WriteLine(emailSections["from"]);
		Console.WriteLine(emailSections["text"]);
		var msgSubject = "";
		var toAddress = "";

		if (emailSections.ContainsKey("subject"))
		{
			msgSubject = emailSections["subject"];
			if (!msgSubject.StartsWith("RE:"))
			{
				msgSubject = "RE: " + msgSubject;
			}
		}
		else
		{
			msgSubject = "Viaboards";
		}

		if (emailSections["from"] != null)
		{
			toAddress = emailSections["from"];
			Regex emailEx = new Regex(@".*<((?:[A-Za-z0-9#\-_~!$&'()*+,;=:]|[A-Za-z0-9#\-_~!$&'()*+,;=:][A-Za-z0-9#\-_~!$&'()*+,;=:\.]*[A-Za-z0-9#\-_~!$&'()*+,;=:])@(?:[A-Za-z0-9]\.|[A-Za-z0-9][A-Za-z0-9#\-]{0,61}[A-Za-z0-9]\.)+(?:[A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9#\-]{0,61}[A-Za-z0-9]))\.?>");
			Match emailMatch = emailEx.Match(toAddress);
			Console.WriteLine("Groups Found=" + emailMatch.Groups.Count.ToString());
			if (emailMatch.Groups.Count >= 2)
			{
				toAddress = emailMatch.Groups[1].Value;
				Console.WriteLine(toAddress);
			}

			SendEmailWithSendGrid(toAddress, CraftResponse(emailSections["text"]), msgSubject);
		}
	}
}

private void SendEmailWithSendGrid(string toAddress, string message, string emailSubject)
{
	var apiKey = "<SENDGRIDAPIKEY>";
	var client = new SendGridClient(apiKey);
	var from = new EmailAddress("info@mail.domain.com", "Info");
	var subject = emailSubject;
	var to = new EmailAddress(toAddress);
	var plainTextContent = message;
	var htmlContent = message;
	var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
	//var response = client.SendEmailAsync(msg);
	//response.Dump();
}

private string CraftResponse(string message)
{
	var responseMsg = "";

	StringReader strReader = new StringReader(message);
	var msgLine = strReader.ReadLine();
	var lineCt = 1;
	var userName = "";
	var bodyOnly = new List<string>();

	// Find the body only lines
	while (msgLine != null || lineCt >= 30)
	{
		Console.WriteLine(msgLine);

		int validationCheck = ValidateMessageLine(msgLine.Trim(), lineCt);
		if (lineCt > 2 && userName == "")
		{
			userName = CheckForName(msgLine);
			if (userName != "")
			{ validationCheck = 3; }
		}
		if (validationCheck == 1)
		{
			bodyOnly.Add(msgLine);
		}
		else if (validationCheck == 3)
		{
			Console.WriteLine("Ending Of Email Found");
			break;
		}

		msgLine = strReader.ReadLine();
		lineCt++;
	}

	// Find the user name
	strReader = new StringReader(message);
	msgLine = strReader.ReadLine();
	while (msgLine != null || lineCt >= 30)
	{
		if (lineCt > 2 && userName == "")
		{
			userName = CheckForName(msgLine);
			if (userName != "")
			{ break; }
		}

		msgLine = strReader.ReadLine();
		lineCt++;
	}

	// process body only lines with Vivian
	Console.WriteLine("Asking AI First");
	var whatVivianThinks = AskVivian(bodyOnly);
	if (whatVivianThinks == null || whatVivianThinks == "")
	{
		foreach (string l in bodyOnly)
		{
			// process body only lines with Wolfram
			Console.WriteLine("Would Call Wolfram");
			var whatWolframThinks = ""; //AskWolframAlpha(new JObject(), new Dictionary<string, string>(), "GET", l);
			//Console.WriteLine(whatWolframThinks);
			if (whatWolframThinks == null && responseMsg == "")
			{
				responseMsg = "I'm sorry I couldn't quite understand. Please rephrase what you asked.";
			}
			else
			{
				responseMsg += Environment.NewLine + whatWolframThinks;
			}
		}
	}
	else
	{
		responseMsg = Environment.NewLine + whatVivianThinks;
	}

	var emailMsg = "Hi" + userName + ",<br /><br />" +
		responseMsg +
		"<br /><br />" +
		"Best Regards," +
		"<br />" +
		"Vivian";

	//Console.WriteLine(emailMsg);
	return emailMsg;
}

private int ValidateMessageLine(string messageLine, int lineCt)
{


	if (messageLine == null)
	{ return 0; }

	if (messageLine == "")
	{ return 0; }

	var wordCt = messageLine.Split(' ').Length;
	var foundWordCt = 0;

	// Check For A Greeting
	foreach (string s in possibleGreetings)
	{
		if (messageLine.ToLower().Contains(s))
		{
			foundWordCt++;
		}
	}

	double score = (double)foundWordCt / (double)wordCt;
	if (score > 0.3)
	{ return 0; }

	// Check For An Ending
	foundWordCt = 0;
	foreach (string s in possibleEndings)
	{
		if (messageLine.ToLower().Contains(s))
		{
			foundWordCt++;
		}
	}

	score = (double)foundWordCt / (double)wordCt;
	if (score > 0.3)
	{ return 3; }

	return 1;
}

private string CheckForName(string messageLine)
{
	if (messageLine == null)
	{ return ""; }

	if (messageLine == "")
	{ return ""; }

	var wordCt = 0;
	if (messageLine.Contains(" "))
	{ wordCt = messageLine.Split(' ').Length; }
	else
	{ wordCt = 1; }

	HashSet<string> allNames = new HashSet<string>(firstNames);
	allNames.UnionWith(lastNames);

	var foundWordCt = 0;
	foreach (string s in messageLine.Split(' '))
	{
		if (allNames.Contains(s))
		{
			foundWordCt++;
		}
	}

	double score = (double)foundWordCt / (double)wordCt;
	if (score > 0.8)
	{
		//Console.WriteLine(messageLine);
		return " " + messageLine;
	}

	return "";
}

private string AskVivian(List<string> messageLines)
{
	string returnStr = null;
	Dictionary<string, AiTask> tasksCopy = vivianTasks;
	foreach (string vtk in tasksCopy.Keys)
	{
		double actionCt = 0;
		double primaryKeywordsCt = 0;
		double otherKeyWordsCt = 0;
		double typesCt = 0;
		int wordCt = 0;

		foreach (string l in messageLines)
		{
			wordCt += l.Split(' ').Length;
			foreach (string a in tasksCopy[vtk].Actions)
			{
				if (l.ToLower().Contains(a))
				{
					actionCt++;
				}
			}

			foreach (string p in tasksCopy[vtk].PrimaryKeywords)
			{
				if (l.ToLower().Contains(p))
				{
					primaryKeywordsCt++;
				}
			}

			foreach (string o in tasksCopy[vtk].OtherKeywords)
			{
				if (l.ToLower().Contains(o))
				{ otherKeyWordsCt += 0.5; }
			}

			foreach (string t in tasksCopy[vtk].Types)
			{
				if (l.ToLower().Contains(t))
				{
					typesCt += 0.5;
				}
			}
		}

		//Console.WriteLine("Actions=" + actionCt);
		//Console.WriteLine("Primary=" + primaryKeywordsCt);
		//Console.WriteLine("Other=" + otherKeyWordsCt);
		//Console.WriteLine("Types=" + typesCt);
		//Console.WriteLine("Words=" + wordCt);

		if (actionCt > 1) { actionCt = 1; }
		if (primaryKeywordsCt > 1) { primaryKeywordsCt = 1; }

		tasksCopy[vtk].ActionCt = actionCt;
		tasksCopy[vtk].PrimaryKeywordsCt = primaryKeywordsCt;
		tasksCopy[vtk].OtherKeywordsCt = otherKeyWordsCt;
		tasksCopy[vtk].TypesCt = typesCt;
		tasksCopy[vtk].Score = (actionCt + primaryKeywordsCt + otherKeyWordsCt + typesCt);
		//Console.WriteLine(tasksCopy[vtk].Score);
	}

	double highestScore = 1.9;
	string bestMatch = "";

	foreach (string vtk in tasksCopy.Keys)
	{
		if (tasksCopy[vtk].Score > highestScore && tasksCopy[vtk].ActionCt >= 1 && tasksCopy[vtk].PrimaryKeywordsCt >= 1)
		{
			returnStr = tasksCopy[vtk].Response;
			bestMatch = vtk;

			highestScore = tasksCopy[vtk].Score;
		}
	}

	switch (bestMatch)
	{
		case "createcard":
			returnStr = CreateCardInVia(FindFieldValues(tasksCopy[bestMatch], messageLines));
			break;
	}

	return bestMatch;
}

private string AskWolframAlpha(JObject jsonObj, Dictionary<string, string> headers, string requestType, string query)
{
	try
	{
		//Console.WriteLine(query);
		System.Net.WebRequest request = System.Net.WebRequest.Create("https://api.wolframalpha.com/v2/query?input=" + query.Replace(" ", "+") + "&format=html&output=JSON&appid=<WOLFRAMALPHAAPIKEY>");

		ASCIIEncoding encoding = new ASCIIEncoding();
		string content = jsonObj.ToString();

		byte[] bytesToWrite = encoding.GetBytes(content);

		if (headers != null)
		{
			foreach (string hk in headers.Keys)
			{
				request.Headers.Add(hk, headers[hk]);
			}
		}

		request.Method = requestType;

		if (request.Method != "GET")
		{
			request.ContentType = "application/json";
			request.ContentLength = bytesToWrite.Length;

			Stream newStream = request.GetRequestStream();
			newStream.Write(bytesToWrite, 0, bytesToWrite.Length);
			newStream.Close();
		}

		System.Net.WebResponse response = request.GetResponse();
		Stream dataStream = response.GetResponseStream();
		StreamReader reader = new StreamReader(dataStream);

		string resp = reader.ReadToEnd();
		string analyzedResult = null;
		JObject respObj = JObject.Parse(resp);
		//Console.WriteLine(respObj.ToString());
		if (respObj["queryresult"]["pods"] != null)
		{
			analyzedResult = "";
			//Console.WriteLine(respObj["queryresult"]["pods"].ToString());
			foreach (JObject p in JArray.Parse(respObj["queryresult"]["pods"].ToString()))
			{
				if (p["position"].ToString() == "100" && p["definitions"] != null)
				{
					analyzedResult += analyzedResult + p["definitions"]["desc"].ToString() + Environment.NewLine + Environment.NewLine;
				}
				else if (p["position"].ToString() == "200" && p["subpods"] != null)
				{
					var subpodsArray = JArray.Parse(p["subpods"].ToString());
					if (subpodsArray.Count > 0)
					{
						analyzedResult += subpodsArray[0]["plaintext"].ToString() + Environment.NewLine + Environment.NewLine;
					}
				}
				else if (p["position"].ToString() == "200" && p["markup"] != null && p["markup"]["data"] != null)
				{
					analyzedResult += p["markup"]["data"].ToString() + "<br /><br />";
				}
				else if (p["position"].ToString() == "300" && p["markup"] != null && p["markup"]["data"] != null)
				{
					analyzedResult += p["markup"]["data"].ToString() + "<br /><br />";
				}
				else if (p["position"].ToString() == "400" && p["markup"] != null && p["markup"]["data"] != null)
				{
					analyzedResult += p["markup"]["data"].ToString() + "<br /><br />";
				}
				else if (p["position"].ToString() == "400" && p["subpods"] != null && p["definitions"] != null)
				{
					analyzedResult += p["title"].ToString() + Environment.NewLine;
					var subpodsArray = JArray.Parse(p["subpods"].ToString());
					if (subpodsArray.Count > 0)
					{
						analyzedResult += subpodsArray[0]["plaintext"].ToString() + Environment.NewLine + Environment.NewLine;
					}
				}
			}
		}

		return analyzedResult;

	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
	}
	return null;
}

private string CreateCardInVia(Dictionary<string, string> fnv)
{
	string betterResponse = null;

	return betterResponse;
}

private Dictionary<string, string> FindFieldValues(AiTask atk, List<string> emailMsg)
{
	Dictionary<string, string> fnv = new Dictionary<string, string>();
	foreach (string l in emailMsg)
	{
		foreach (string a in atk.Actions)
		{
			if (l.ToLower().Contains(a))
			{

			}
		}

		foreach (string p in atk.PrimaryKeywords)
		{

		}

		foreach (string o in atk.OtherKeywords)
		{

		}

		foreach (string t in atk.Types)
		{

		}
	}
	return fnv;
}

private class AiTask
{
	public List<string> Actions;
	public double ActionCt;
	public List<string> PrimaryKeywords;
	public double PrimaryKeywordsCt;
	public List<string> OtherKeywords;
	public double OtherKeywordsCt;
	public List<string> Types;
	public double TypesCt;
	public double Score;
	public string Response;
}

// Define other methods and classes here