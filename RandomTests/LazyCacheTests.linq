<Query Kind="Program">
  <NuGetReference>LazyCache</NuGetReference>
  <Namespace>LazyCache</Namespace>
  <Namespace>LazyCache.Mocks</Namespace>
</Query>

void Main()
{
	IAppCache cache = new CachingService();
	
	// Get Cached Object
	var initial = cache.Get<int>("test-cache");
	Console.WriteLine(initial);
	
	//Removed Cached Object
	cache.Remove("test-cache");
	
	//Get Or Add Cached Object
	var testCache = cache.GetOrAdd("test-cache", () => 5);
	Console.WriteLine(testCache);
	
	//Add Cached Object
	cache.Add("test-cache", 20);
}

// Define other methods and classes here