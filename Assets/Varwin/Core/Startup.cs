using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod()
    {
        
    }
}


public static class UniqueString
{
    public static string Get()
    {
        string macAddresses = string.Empty;

        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus == OperationalStatus.Up)
            {
                macAddresses += nic.GetPhysicalAddress().ToString();
                break;
            }
        }

        string unique = macAddresses;

        return GetHash(unique);

    }

    public static string GetHash(string inputString)
    {
        HashAlgorithm algorithm = MD5.Create();  //or use SHA256.Create();
        byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        return BitConverter.ToString(bytes).Replace("-", String.Empty);
    }
}

