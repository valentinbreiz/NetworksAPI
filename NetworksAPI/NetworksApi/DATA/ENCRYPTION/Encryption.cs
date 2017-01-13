namespace NetworksApi.DATA.ENCRYPTION
{
    using System;
    using System.Text;

    public class Encryption
    {
        public static string Decrypt(string Data) => 
            Encoding.ASCII.GetString(Convert.FromBase64String(Data));

        public static string Encrypt(string Data) => 
            Convert.ToBase64String(Encoding.ASCII.GetBytes(Data));
    }
}

