<Query Kind="VBProgram">
  <Namespace>System.Net</Namespace>
</Query>

Sub Main
	Dim ipAddress As String= "172.30.200.200"
	Dim cidrMask As String= "172.30.200.224/27"

	If IsInRange(ipAddress, cidrMask) Then
		Console.WriteLine("IP is in Range")
	Else
		Console.WriteLine("IP NOT in Range")
	End If
End Sub

Private Function IsInRange(Byval ipAddr As String, Byval CIDRmask As String) As Boolean

    Console.WriteLine("ipRange=" & CIDRmask)
	Console.WriteLine("requestorIp=" & ipAddr)

	Dim parts() As String = CIDRmask.Split("/"c)

	Dim IP_addr As Integer = BitConverter.ToInt32(IPAddress.Parse(parts(0)).GetAddressBytes(), 0)
	Dim CIDR_addr As Integer = BitConverter.ToInt32(IPAddress.Parse(ipAddr).GetAddressBytes(), 0)
	Dim CIDR_mask As Integer = IPAddress.HostToNetworkOrder(-1 << (32 - Integer.Parse(parts(1))))

	Console.WriteLine(IP_addr)
	Console.WriteLine(CIDR_addr)
	Console.WriteLine(CIDR_mask)
	Console.WriteLine((IP_addr & CIDR_mask))
	Console.WriteLine((CIDR_addr & CIDR_mask))
	Return ((IP_addr And CIDR_mask) = (CIDR_addr And CIDR_mask))
End Function

' Define other methods and classes here
