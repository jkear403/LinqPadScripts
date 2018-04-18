<Query Kind="Program">
  <Namespace>System.Dynamic</Namespace>
</Query>

void Main()
{
	dynamic newPerson1 = new Person();
	newPerson1.firstName = "Bob";
	newPerson1.lastName = "Smith";
	newPerson1.Prototype.fullName();
	//Console.WriteLine(newPerson1.Prototype.fullName());

	dynamic newPerson2 = new Person();
	newPerson2.firstName = "Jim";
	newPerson2.lastName = "Campbell";
	newPerson2.Prototype.fullName = (Action)(() => Console.WriteLine("Changed this Function"));
	newPerson2.Prototype.fullName();
	//Console.WriteLine(newPerson2.Prototype.fullName());
	newPerson1.Prototype.fullName();
}

public class Person : DynamicObject
{
	public dynamic Prototype = new ExpandoObject();
	public Person()
	{
		this.AddMethod((Action)(() => Console.WriteLine(this.firstName + " " + this.lastName)),"fullName");
	}
	public string firstName { get; set; }
	public string lastName { get; set; }

	public void AddProperty(string name, object value)
	{
		((IDictionary<string, object>)this.Prototype).Add(name, value);
	}

	public dynamic GetProperty(string name)
	{
		if (((IDictionary<string, object>)this.Prototype).ContainsKey(name))
			return ((IDictionary<string, object>)this.Prototype)[name];
		else
			return null;
	}

	public void AddMethod(Action methodBody, string methodName)
	{
		this.AddProperty(methodName, methodBody);
	}
}