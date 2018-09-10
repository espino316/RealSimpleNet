using System;
using System.IO;
using System.Net;
using System.Text;

namespace RealSimpleNet.Helpers
{
    public class FTP
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

        public bool DirectoryExists(string directory)
        {
            bool IsExists = true;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Server + "/" + directory);
                request.Credentials = new NetworkCredential(User, Pwd);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                if (response.ContentLength == -1)
                {
                    return false;
                }
            }
            catch (WebException ex)
            {
                IsExists = false;
            }

            return IsExists;
        }

        public void CreateDirectory(string directory)
        {
            try
            {
                //create the directory
                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(Server + "/" + directory);
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(User, Pwd);
                requestDir.UsePassive = true;
                requestDir.UseBinary = true;
                requestDir.KeepAlive = false;
                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        } // end create directory

        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="localFileName"></param>
        public void Download(string fileName, string localFileName)
        {
            Http http = new Http();
            http.Download("ftp://" + Server + "/" + fileName, localFileName, User, Pwd);
        } // end function Download
        
        /// <summary>
        /// Sube un archivo al servidor FTP
        /// </summary>
        /// <param name="fileName"></param>
        public void Upload(string source, string destination)
        {
            
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Server + "/" + destination);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(this.User, this.Pwd);

            StreamReader sourceStream = new StreamReader(source);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();

            request.ContentLength = fileContents.Length;
            Stream requestStream = request.GetRequestStream();        
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Upload File Complete {0}, status {1}", destination, response.StatusDescription);
            response.Close();

        } // end Upload
    } // end class FTP
} // end namespace
