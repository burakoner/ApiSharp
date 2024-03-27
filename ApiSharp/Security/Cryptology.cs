namespace ApiSharp.Security;

/// <summary>
/// Cryptology Methods
/// </summary>
public static class Cryptology
{
    /// <summary>
    /// Encrypt a byte array into a byte array using a key and an IV 
    /// </summary>
    /// <param name="clearData"></param>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] clearData, byte[] key, byte[] iv)
    {
        // Create a MemoryStream to accept the encrypted bytes 
        var ms = new MemoryStream();
        var alg = Rijndael.Create();
        alg.Key = key;
        alg.IV = iv;
        var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(clearData, 0, clearData.Length);
        cs.Close();

        var encryptedData = ms.ToArray();
        return encryptedData;
    }

    /// <summary>
    /// Encrypt String
    /// </summary>
    /// <param name="clearText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string Encrypt(string clearText, string password)
    {
        var clearBytes = Encoding.Unicode.GetBytes(clearText);
        var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
        var encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Encrypt byte with string password
    /// </summary>
    /// <param name="clearData"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] Encrypt(byte[] clearData, string password)
    {
        var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
        return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));
    }

    /// <summary>
    /// Encrypt a file into another file using a password 
    /// </summary>
    /// <param name="fileIn"></param>
    /// <param name="fileOut"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool Encrypt(string fileIn, string fileOut, string password)
    {
        try
        {
            var fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            var fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write);
            var bufferLen = 4096;
            var buffer = new byte[bufferLen];
            int bytesRead;
            do
            {
                // read a chunk of data from the input file 
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                // encrypt it 
                cs.Write(buffer, 0, bytesRead);
            }
            while (bytesRead != 0);

            cs.Close();
            fsIn.Close();

            // Return
            return true;
        }
        catch
        {
            // Return
            return false;
        }
    }

    /// <summary>
    /// Decrypt a byte array into a byte array using a key and an IV 
    /// </summary>
    /// <param name="cipherData"></param>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] cipherData, byte[] key, byte[] iv)
    {
        var ms = new MemoryStream();
        try
        {
            var alg = Rijndael.Create();
            alg.Key = key;
            alg.IV = iv;
            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
        }
        catch
        {

        }
        var decryptedData = ms.ToArray();

        return decryptedData;
    }

    /// <summary>
    /// Decrypt string
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText, string password)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
        var decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
        return Encoding.Unicode.GetString(decryptedData);
    }

    /// <summary>
    /// Decrypt byte
    /// </summary>
    /// <param name="cipherData"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] cipherData, string password)
    {
        var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
        return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));
    }

    /// <summary>
    /// Decrypt a file into another file using a password 
    /// </summary>
    /// <param name="fileIn"></param>
    /// <param name="fileOut"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool Decrypt(string fileIn, string fileOut, string password)
    {
        try
        {
            // First we are going to open the file streams 
            var fsIn = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            var fsOut = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            var pdb = new PasswordDeriveBytes(password, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write);
            var bufferLen = 4096;
            var buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                // read a chunk of data from the input file 
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                // Decrypt it 
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            cs.Close();
            fsIn.Close();

            // Return
            return true;
        }
        catch
        {
            // Return
            return false;
        }

    }

    /// <summary>
    /// Supported hash algorithms
    /// </summary>
    public enum HashType
    {
        HMAC, HMACMD5, HMACSHA1, HMACSHA256, HMACSHA384, HMACSHA512, MD5, SHA1, SHA256, SHA384, SHA512
    }

    private static byte[] GetHash(string source, HashType hash)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(source);

        return hash switch
        {
            HashType.HMAC => HMAC.Create().ComputeHash(inputBytes),
            HashType.HMACMD5 => HMACMD5.Create().ComputeHash(inputBytes),
            HashType.HMACSHA1 => HMACSHA1.Create().ComputeHash(inputBytes),
            HashType.HMACSHA256 => HMACSHA256.Create().ComputeHash(inputBytes),
            HashType.HMACSHA384 => HMACSHA384.Create().ComputeHash(inputBytes),
            HashType.HMACSHA512 => HMACSHA512.Create().ComputeHash(inputBytes),
            HashType.MD5 => MD5.Create().ComputeHash(inputBytes),
            HashType.SHA1 => SHA1.Create().ComputeHash(inputBytes),
            HashType.SHA256 => SHA256.Create().ComputeHash(inputBytes),
            HashType.SHA384 => SHA384.Create().ComputeHash(inputBytes),
            HashType.SHA512 => SHA512.Create().ComputeHash(inputBytes),
            _ => inputBytes,
        };
    }

    /// <summary>
    /// Computes the hash of the string using a specified hash algorithm
    /// </summary>
    /// <param name="source">The string to hash</param>
    /// <param name="hashType">The hash algorithm to use</param>
    /// <returns>The resulting hash or an empty string on error</returns>
    public static string Hash(string source, HashType hashType)
    {
        try
        {
            var hash = GetHash(source, hashType);
            var ret = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                ret.Append(hash[i].ToString("x2"));

            return ret.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetMD5HashFromFile(string fileName)
    {
        var file = new FileStream(fileName, FileMode.Open);
        var md5 = new MD5CryptoServiceProvider();
        var retVal = md5.ComputeHash(file);
        file.Close();

        var sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static string Hmac256(string message, string secret)
    {
        var encoding = Encoding.UTF8;
        using var hmac = new HMACSHA256(encoding.GetBytes(secret));
        var msg = encoding.GetBytes(message);
        var hash = hmac.ComputeHash(msg);
        return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
    }

    public static string Hmac384(string message, string secret)
    {
        var encoding = Encoding.UTF8;
        using var hmac = new HMACSHA384(encoding.GetBytes(secret));
        var msg = encoding.GetBytes(message);
        var hash = hmac.ComputeHash(msg);
        return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
    }

    public static string Hmac512(string message, string secret)
    {
        var encoding = Encoding.UTF8;
        using var hmac = new HMACSHA512(encoding.GetBytes(secret));
        var msg = encoding.GetBytes(message);
        var hash = hmac.ComputeHash(msg);
        return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
    }

    public static string Encode128(string data, string key)
    {
        try
        {
            var clearBytes = Encoding.Unicode.GetBytes(data);
            var pdb = new PasswordDeriveBytes(key, [0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4]);
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(16);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            var encryptedData = ms.ToArray();
            return Convert.ToBase64String(encryptedData);
        }
        catch
        {
            return "Failed!";
        }
    }

    public static string Decode128(string data, string key)
    {
        try
        {
            var clearBytes = Convert.FromBase64String(data);
            var pdb = new PasswordDeriveBytes(key, [0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4]);
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(16);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            var decryptedData = ms.ToArray();
            return Encoding.Unicode.GetString(decryptedData);
        }
        catch
        {
            return "Failed!";
        }
    }

    public static string Encode256(string data, string key)
    {
        try
        {
            var clearBytes = Encoding.Unicode.GetBytes(data);
            var pdb = new PasswordDeriveBytes(key, [0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4]);
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            var encryptedData = ms.ToArray();
            return Convert.ToBase64String(encryptedData);
        }
        catch
        {
            return "Failed!";
        }
    }

    public static string Decode256(string data, string key)
    {
        try
        {
            var clearBytes = Convert.FromBase64String(data);
            var pdb = new PasswordDeriveBytes(key, [0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4]);
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.Close();
            var decryptedData = ms.ToArray();
            return Encoding.Unicode.GetString(decryptedData);
        }
        catch
        {
            return "Failed!";
        }
    }

    public static string ConvertStringToHex(string input, Encoding encoding)
    {
        var stringBytes = encoding.GetBytes(input);
        var sbBytes = new StringBuilder(stringBytes.Length * 2);
        foreach (byte b in stringBytes)
        {
            sbBytes.AppendFormat("{0:X2}", b);
        }
        return sbBytes.ToString();
    }

    public static string ConvertHexToString(string hexInput, Encoding encoding)
    {
        var numberChars = hexInput.Length;
        var bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
        }
        return encoding.GetString(bytes);
    }

    public enum OneWayTicketHashType
    {
        Hexadecimal, AlphaNumeric, AlphaNumericWithSpecialChars
    }

    private static readonly char[] _oneWayTicketHexadecimalChars = [
        '0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'
    ];
    private static readonly char[] _oneWayTicketAlphaNumericChars = [
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
    ];
    private static readonly char[] _oneWayTicketAlphaNumericWithSpecialChars = [
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
        '!','#','$','&','(',')','[',']','{','}','+','-','*','/','%','|','^','~','?',':','.',',',':',';','=','~'
    ];

    public static string OneWayTicket(string password, OneWayTicketHashType type = OneWayTicketHashType.AlphaNumeric, int blockSize = 32)
    {
        if (string.IsNullOrWhiteSpace(password)) return string.Empty;
        var bytes = Encoding.UTF8.GetBytes(password);
        return OneWayTicket(bytes, type, blockSize);
    }

    public static string OneWayTicket(byte[] data, OneWayTicketHashType type = OneWayTicketHashType.AlphaNumeric, int blockSize = 32)
    {
        if (data == null || data.Length == 0) return string.Empty;

        var seed = data.Sum(x => x) + data.Length;
        var rand = new Random(seed);
        var rest = blockSize - (data.Length % blockSize);

        var chars = new char[0];
        if (type == OneWayTicketHashType.Hexadecimal) chars = _oneWayTicketHexadecimalChars;
        else if (type == OneWayTicketHashType.AlphaNumeric) chars = _oneWayTicketAlphaNumericChars;
        else if (type == OneWayTicketHashType.AlphaNumericWithSpecialChars) chars = _oneWayTicketAlphaNumericWithSpecialChars;

        var sb = new StringBuilder();
        for (var i = 0; i < data.Length + rest; i++)
        {
            var randomNumber = rand.Next(chars.Length);
            var charValue = chars[randomNumber];

            sb.Append(charValue);
        }

        return sb.ToString();
    }
}