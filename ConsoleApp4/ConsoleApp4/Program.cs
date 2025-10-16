using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string input = "HelloWorld123";

        // Convert input string to bytes
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        // Create MD5 instance
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            Console.WriteLine("MD5 Hash: " + sb.ToString());
        }
    }
}

