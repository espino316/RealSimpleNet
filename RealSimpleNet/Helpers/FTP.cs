using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RealSimpleNet.Helpers
{
    class FTP
    {
        /// <summary>
        /// El servidor al cual conectarse
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// El usuario que se conecta
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// El password del usuario
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// Sube un archivo al servidor FTP
        /// </summary>
        /// <param name="fileName"></param>
        public void Upload(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + Server + "/" + fileInfo.Name);

            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(this.User, this.Pwd);

            StreamReader sourceStream = new StreamReader(fileName);

            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());

            sourceStream.Close();

            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();

            requestStream.Write(fileContents, 0, fileContents.Length);

            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();

        } // end Upload
    } // end class FTP
} // end namespace
