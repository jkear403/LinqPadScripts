<Query Kind="Program">
  <Namespace>System.Net</Namespace>
</Query>

void Main()
{
	string ipAddress = "172.30.200.250";
	string cidrMask = "172.30.200.224/27";
	
	if (IsInRange(ipAddress, cidrMask)) {
		Console.WriteLine("IP is in Range");
	} else {
		Console.WriteLine("IP NOT in Range");
	}
}

private bool IsInRange(string ipAddress, string CIDRmask)
{
	string[] parts = CIDRmask.Split('/');

	int IP_addr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
	int CIDR_addr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
	int CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

	Console.WriteLine(IP_addr);
	Console.WriteLine(CIDR_addr);
	Console.WriteLine(CIDR_mask);
	Console.WriteLine((IP_addr & CIDR_mask));
	Console.WriteLine((CIDR_addr & CIDR_mask));
	return ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
}
// Define other methods and classes here
