
using System.IO;

using System.Security.Cryptography;


namespace ConsoleApplication1

{

    class Program

    {

        static void Main(string[] args)

        {

            Decrypt("compact.sqlite3", "decrypted.sqlite3", "decrypted.data");

        }

        public static void Decrypt(string encryptedFile, string decryptedFile, string keyFile)

        {


            byte[] key = new byte[16];

            using (var stream = new FileStream(keyFile, FileMode.Open, FileAccess.Read))

            {

                stream.Read(key, 0, (int)stream.Length);

            }

            using (var memoryEncryptedStream = new MemoryStream())

            using (var memoryDecryptedStream = new MemoryStream())

            {

                //read file with 16byte{ zero}

                for (int i = 0; i < 16; i++)

                {

                    memoryEncryptedStream.WriteByte(0);

                }

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

                    Padding = PaddingMode.None,

                    Mode = CipherMode.CBC

                }.CreateDecryptor())

                using (var cryptoStream = new CryptoStream(memoryDecryptedStream, transform, CryptoStreamMode.Write))

                {


                    byte[] buffer = new byte[1024];

                    int len;

                    while ((len = memoryEncryptedStream.Read(buffer, 0, buffer.Length)) > 0)

                    {

                        cryptoStream.Write(buffer, 0, len);
                        //cryptoStream.FlushFinalBlock();

                    }


                    cryptoStream.Close();

                    transform.Dispose();

                }


                //write file without head 16byte

                using (var decryptedFileStream = new FileStream(decryptedFile, FileMode.Create, FileAccess.Write))

                {

                    var outTemp = memoryDecryptedStream.ToArray();

                    decryptedFileStream.Write(outTemp, 16, outTemp.Length-16);

                    decryptedFileStream.Close();

                }


                memoryEncryptedStream.Close();

                memoryDecryptedStream.Close();

            }

        }

    }

}