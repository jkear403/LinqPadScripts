<Query Kind="Program" />

void Main()
{
	Random rndGen = new Random();
	List<TestObject> listTest = new List<TestObject>();
	for (int i = 1; i <= 20; i++)
	{
		TestObject newObj = new TestObject();
		newObj.oid = i;
		newObj.randomNumber = rndGen.Next(100, 999);
		newObj.description = "Test Decription - " + i.ToString("0#") + " | " + newObj.randomNumber.ToString();
		listTest.Add(newObj);
	}

	Console.WriteLine("-------------------------------------------------");
	PrintTestObjLst(listTest);
	Console.WriteLine("-------------------------------------------------");
	TestObject result = listTest.Aggregate((x, y) => x.randomNumber > y.randomNumber ? x : y);
	Console.WriteLine("Result From Aggregate");
	Console.WriteLine(string.Format("{0}  {1}  {2}",result.oid.ToString("0#"), result.randomNumber, result.description));

}

public void PrintTestObjLst(List<TestObject> testObjLst)
{
	foreach (TestObject t in testObjLst)
	{
		Console.WriteLine(string.Format("{0}  {1}  {2}",t.oid.ToString("0#"), t.randomNumber, t.description));
	}
}

public class TestObject
{
	public int oid { get; set; }
	public int randomNumber { get; set; }
	public string description { get; set; }
}
// Define other methods and classes here
