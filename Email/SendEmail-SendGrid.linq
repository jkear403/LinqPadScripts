<Query Kind="Program">
  <NuGetReference>Sendgrid</NuGetReference>
  <Namespace>SendGrid</Namespace>
  <Namespace>SendGrid.Helpers.Mail</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
</Query>

void Main()
{
	var apiKey = "<SendGridApiKey>";
	var client = new SendGridClient(apiKey);
	var from = new EmailAddress("no-reply@mail.domain.com", "Company");
	var subject = "Hello";
	var to = new EmailAddress("someone@domain.com");
	var plainTextContent = "Html Emails not Supported";

	var emailTemplate = File.ReadAllText(@"c:\Base\EmailTemplates\General.html");
	var htmlContent = emailTemplate.Replace("{{fullname}}", "John Doe");
	var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, emailTemplate);
	var response = client.SendEmailAsync(msg);
	Console.WriteLine(response);
}

// Define other methods and classes here