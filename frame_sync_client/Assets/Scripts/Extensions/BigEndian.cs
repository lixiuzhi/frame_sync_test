using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class BigEndian
{
    public static short ToBigEndian(this short value)
    {
        return System.Net.IPAddress.HostToNetworkOrder(value);
    }
    public static ushort ToBigEndian(this ushort value)
    {
        return (ushort)System.Net.IPAddress.HostToNetworkOrder((short)value);
    }
    public static int ToBigEndian(this int value)
    {
        return System.Net.IPAddress.HostToNetworkOrder(value);
    }
    public static long ToBigEndian(this long value)
    {
        return System.Net.IPAddress.HostToNetworkOrder(value);
    }

    public static UInt32 ToBigEndian(this UInt32 value)
    {
        return (UInt32)System.Net.IPAddress.HostToNetworkOrder((int)value);
    }

    public static short FromBigEndian(this short value)
    {
        return System.Net.IPAddress.NetworkToHostOrder(value);
    }
    public static int FromBigEndian(this int value)
    {
        return System.Net.IPAddress.NetworkToHostOrder(value);
    }
    public static long FromBigEndian(this long value)
    {
        return System.Net.IPAddress.NetworkToHostOrder(value);
    }

    public static ushort FromBigEndian(this ushort value)
    {
        return (ushort)System.Net.IPAddress.NetworkToHostOrder((short)value);
    }

    public static UInt32 FromBigEndian(this UInt32 value)
    {
        return (UInt32)System.Net.IPAddress.NetworkToHostOrder((int)value);
    }
}
