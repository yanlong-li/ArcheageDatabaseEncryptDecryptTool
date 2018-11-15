
using System.IO;
using System.Security.Cryptography;
namespace aadbencrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            Encrypt("decrypted.sqlite3", "compact.sqlite3", "decrypted.data", "head.sqlite3");
        }
        public static void Encrypt(string encryptedFile, string decryptedFile, string keyFile,string headFile)
        {
            byte[] key = new byte[16];
            byte[] head = new byte[16];
            using (var stream = new FileStream(keyFile, FileMode.Open, FileAccess.Read))
            {
                stream.Read(key, 0, (int)stream.Length);
            }
            using (var memoryEncryptedStream = new MemoryStream())
            using (var memoryDecryptedStream = new MemoryStream())
            {
                using (var stream = new FileStream(headFile, FileMode.Open, FileAccess.Read))

                {

                    stream.Read(head, 0, (int)stream.Length);

                }
                memoryEncryptedStream.Write(head, 0, 16);
                var temp = File.ReadAllBytes(encryptedFile);
                memoryEncryptedStream.Write(temp, 0, temp.Length);


                memoryEncryptedStream.Position = 0;

                //decrypt
                using (ICryptoTransform transform = new RijndaelManaged
                {
                    BlockSize = 128,
                    IV = key,
                    KeySize = 128,
                    Key = key,
                    Padding = PaddingMode.ANSIX923,
                    Mode = CipherMode.CBC
                }.CreateEncryptor())
                using (var cryptoStream = new CryptoStream(memoryDecryptedStream, transform, CryptoStreamMode.Write))
                {
                    byte[] buffer = new byte[1024];
                    int len;
                    while ((len = memoryEncryptedStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cryptoStream.Write(buffer, 0, len);
                    }
                    cryptoStream.Close();
                    transform.Dispose();
                }
                //write file without head 16byte
                using (var decryptedFileStream = new FileStream(decryptedFile, FileMode.Create, FileAccess.Write))
                {
                    var outTemp = memoryDecryptedStream.ToArray();
                    decryptedFileStream.Write(outTemp, 16, outTemp.Length - 16);
                    decryptedFileStream.Close();
                }
                memoryEncryptedStream.Close();
                memoryDecryptedStream.Close();
            }
        }
    }
}
