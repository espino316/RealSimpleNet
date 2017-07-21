using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RealSimpleNet.Helpers
{
    /// <summary>
    /// Helper to crypt strings and files
    /// </summary>
    public class Crypt
    {
        static byte[] key;
        static byte[] IV;
        static string str_key = "bNxlyD8c05Zw4t5DR957043z";
        static string str_IV = "C28x6ovpJ4OrZ148H0bLh42J";
        private static TripleDESCryptoServiceProvider des =
            new TripleDESCryptoServiceProvider();

        public Crypt()
        {
            ASCIIEncoding textConverter = new ASCIIEncoding();
            key = textConverter.GetBytes(str_key);
            IV = textConverter.GetBytes(str_IV);
        }

        public static void SetKeys()
        {
            ASCIIEncoding textConverter = new ASCIIEncoding();
            key = textConverter.GetBytes(str_key);
            IV = textConverter.GetBytes(str_IV);
        }

        public static String Encrypt(String strEnc)
        {
            SetKeys();

            ASCIIEncoding textConverter = new ASCIIEncoding();
            byte[] toEncrypt;

            MemoryStream msEncrypt = new MemoryStream();
            CryptoStream csEncrypt = new CryptoStream(msEncrypt,
                des.CreateEncryptor(key, IV), CryptoStreamMode.Write);

            toEncrypt = textConverter.GetBytes(strEnc);

            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();

            return Convert.ToBase64String(msEncrypt.ToArray());
        }


        public static String Decrypt(String strDesEnc)
        {
            SetKeys();

            ASCIIEncoding textConverter = new ASCIIEncoding();
            byte[] fromEncrypt;
            byte[] encrypted;
            byte[] ret;

            encrypted = Convert.FromBase64String(strDesEnc);

            MemoryStream msDecrypt = new MemoryStream(encrypted);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                des.CreateDecryptor(key, IV), CryptoStreamMode.Read);

            fromEncrypt = new byte[encrypted.Length];

            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

            ret = StripZeros(fromEncrypt);

            strDesEnc = textConverter.GetString(ret);

            return strDesEnc;
        }

        private static byte[] StripZeros(byte[] bites)
        {
            List<byte> list = new List<byte>();

            foreach (byte b in bites)
            {
                if (b != (byte)0)
                {
                    list.Add(b);
                }
            }

            return list.ToArray();

        }

        /// <summary>
        /// Sigue sin funcionar
        /// </summary>
        /// <param name="sInputFilename"></param>
        /// <returns></returns>
        public static MemoryStream EncryptFile(string sInputFilename)
        {
            SetKeys();

            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            MemoryStream temp = new MemoryStream();
            MemoryStream output = new MemoryStream();

            ICryptoTransform desencrypt = des.CreateEncryptor(key, IV);
            CryptoStream cryptostream = new CryptoStream(temp, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];

            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            //Imprimir el contenido del archivo descifrado.             
            temp.WriteTo(output);

            //Cerrar los streams
            cryptostream.Close();
            fsInput.Close();

            return output;
        }

        /// <summary>
        /// Desencripta el archivo especificado
        /// </summary>
        /// <param name="sInputFilename">El archivo a desencriptar</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream DecryptFile(string sInputFilename)
        {
            SetKeys();

            //Crear una secuencia de archivo para volver a leer el archivo cifrado. 
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);

            //Crear un descriptor de DES desde la instancia de DES. 
            ICryptoTransform desdecrypt = des.CreateDecryptor(key, IV);

            //Crear una secuencia de cifrado para leer y 
            //realizar una transformación de cifrado DES en los bytes de entrada. 
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);

            //Imprimir el contenido del archivo descifrado. 
            StreamReader sr = new StreamReader(cryptostreamDecr);
            byte[] buff = Encoding.ASCII.GetBytes(sr.ReadToEnd());

            fsread.Flush();
            fsread.Close();
            fsread.Dispose();
            sr.Close();
            sr.Dispose();

            return new MemoryStream(buff);
        }

        public static void EncryptFile(string sInputFilename, string sOutputFilename)
        {
            SetKeys();

            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);

            ICryptoTransform desencrypt = des.CreateEncryptor(key, IV);
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];

            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        public static void DecryptFile(string sInputFilename, string sOutputFilename)
        {
            SetKeys();

            //Crear una secuencia de archivo para volver a leer el archivo cifrado. 
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);

            //Crear un descriptor de DES desde la instancia de DES. 
            ICryptoTransform desdecrypt = des.CreateDecryptor(key, IV);

            //Crear una secuencia de cifrado para leer y 
            //realizar una transformación de cifrado DES en los bytes de entrada. 
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);

            //Imprimir el contenido del archivo descifrado. 
            StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }

        public static void EncryptFile(Stream inputData, string sOutputFilename)
        {
            SetKeys();

            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);

            ICryptoTransform desencrypt = des.CreateEncryptor(key, IV);
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[inputData.Length];

            inputData.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();
            inputData.Close();
            fsEncrypted.Close();
        }

        public static void EncryptDataSet(DataSet ds, string filename)
        {
            SetKeys();

            Stream inputData = DataSetAsStream(ds);

            FileStream fsEncrypted = new FileStream(filename, FileMode.Create, FileAccess.Write);

            ICryptoTransform desencrypt = des.CreateEncryptor(key, IV);
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[inputData.Length];

            inputData.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();
            inputData.Close();
            fsEncrypted.Close();
        }

        public static Stream DataSetAsStream(DataSet ds)
        {
            MemoryStream result = new MemoryStream();
            ds.WriteXml(result);

            result.Position = 0;
            return result;
        }

        public static string SHA1(string str)
        {
            SHA1 sha1 = SHA1Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();

            stream = sha1.ComputeHash(encoding.GetBytes(str));

            for (int i = 0; i < stream.Length; i++)
            {
                sb.AppendFormat("{0:x2}", stream[i]);
            }

            return sb.ToString();
        }

        public static string MD5(string str)
        {
            MD5 sha1 = MD5CryptoServiceProvider.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();

            stream = sha1.ComputeHash(encoding.GetBytes(str));

            for (int i = 0; i < stream.Length; i++)
            {
                sb.AppendFormat("{0:x2}", stream[i]);
            }

            return sb.ToString();
        }        
    } // end class
} // end namespace
