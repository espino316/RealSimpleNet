using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace RealSimpleReleases.lib
{
    class ArgumentsParser
    {
        private Dictionary<string, string> CommandList = new Dictionary<string, string>();
        private JavaScriptSerializer serializer = new JavaScriptSerializer();
        private models.Manifest currentManifest;
        public ArgumentsParser()
        {
            this.CommandList.Add(@"add-file[\s][^ ]*.", "AddFile");
            this.CommandList.Add("init", "Init");
            this.CommandList.Add("add", "Add");
        }

        public void Parse(string[] args)
        {
            // add-file some/file dest/path
            string commandStr = String.Join(" ", args);

            foreach (KeyValuePair<string, string> command in this.CommandList)
            {
                if (this.Match(command.Key, commandStr))
                {
                    Console.WriteLine(string.Format("Match! {0}", command.Value));
                    return;
                } // end if match
            } // end for each
        } // end function Parse

        private bool Match(string pattern, string value)
        {
            Regex regex = new Regex(pattern);
            Match m = regex.Match(value);
            return (m.Value == value);
        } // end function Match

        private void AddFile(string source)
        {

        } // end function add file

        public void Init(string appname, string main, string url, string user, string pwd)
        {
            //  Creates the manifest, the json file
            //  The directory
            string dir = Directory.GetCurrentDirectory() + "\\";
            models.Manifest manifest = new models.Manifest();
            manifest.appname = appname;
            manifest.version = "1.0.0.0";
            manifest.main = main;
            manifest.ftpcredentials =
                new models.Credentials(
                    url,
                    user,
                    pwd
                );
            manifest.ftpcredentials.Encrypt();

            this.ParseDirectory(dir, dir, ref manifest);

            string json = serializer.Serialize(manifest);
            //json = RealSimpleNet.Helpers.Crypt.Encrypt(json);

            File.WriteAllText(
                "manifest.json",
                json
            ); // end write all text
        } // end function init

        public void ReadManifest()
        {
            string json = File.ReadAllText("manifest.json");
            //json = RealSimpleNet.Helpers.Crypt.Decrypt(json);
            this.currentManifest = serializer.Deserialize<models.Manifest>(json);
            this.currentManifest.ftpcredentials.Decrypt();
        }

        public void ParseDirectory(string rootDir, string dir, ref models.Manifest manifest)
        {
            string[] files = Directory.GetFiles(dir);
            string[] directories = Directory.GetDirectories(dir);

            foreach (string file in files)
            {
                manifest.files.Add(
                    new models.MonitoredFile(
                        file.Replace(rootDir, ""),
                        RealSimpleNet.Helpers.Crypt.Checksum(file)
                    ) // end new model.MonitoredFile
                ); // end manifest add                
            } // end foreach

            foreach (string directory in directories)
            {
                ParseDirectory(rootDir, directory, ref manifest);
            } // end for each directory
        } // end function ParseDirectory

        /// <summary>
        /// Publish a new release of the software
        /// </summary>
        /// <param name="version"></param>
        public void PublishRelease(string version = null)
        {
            File.WriteAllText("latest", version);
            
            this.ReadManifest();
            //  Upgrade version
            currentManifest.version = version;
            
            string dir = Directory.GetCurrentDirectory();
            
            RealSimpleNet.Helpers.FTP ftp = new RealSimpleNet.Helpers.FTP();
            ftp.Server = currentManifest.ftpcredentials.Url;
            ftp.User = currentManifest.ftpcredentials.User;
            ftp.Pwd = currentManifest.ftpcredentials.Pwd;

            ftp.Upload("latest", "latest");
            ftp.CreateDirectory(version);
            
            string[] sep = new string[] { "\\" };
            foreach (models.MonitoredFile file in currentManifest.files)
            {
                string[] parts = file.filename.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    int i = 0;
                    string treeDir = "";
                    for(i = 0; i < parts.Length-1; i++)
                    {
                        treeDir += "/" + parts[i];
                        ftp.CreateDirectory(version + treeDir);
                    }
                } // end if parts > 1

                string destination = version + "/" + file.filename.Replace("\\", "/");
                ftp.Upload(file.filename, destination);
            } // end for each
            
        } // end publish release

        /// <summary>
        /// Backup the current file
        /// Create version folder
        /// Prepare directory structure if any
        /// Move current file to current version folder
        /// Download the new file
        /// </summary>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        private void BackupMonitoredFile(
            string version,
            string fileName)
        {
            //  Create version folder if not exists
            if (!Directory.Exists(version))
            {
                Directory.CreateDirectory(version);
            } // end if directory not exists

            // If the file is in subfolders, prepare the directory tree
            string[] sep = new string[] { "\\" };
            string[] parts = fileName.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                int i = 0;
                string treeDir = "";
                for (i = 0; i < parts.Length - 1; i++)
                {
                    treeDir += "\\" + parts[i];
                    Directory.CreateDirectory(version + treeDir);
                } // end for
            } // end if parts > 1

            //  Here we finally move the file
            File.Move(fileName, string.Format("{0}\\{1}", version, fileName));

        } // end function backup monitored file

        /// <summary>
        /// Downloads a monitored file
        /// </summary>
        /// <param name="http"></param>
        /// <param name="version"></param>
        /// <param name="fileName"></param>
        private void DownloadMonitoredFile(
            ref RealSimpleNet.Helpers.Http http,
            string version,
            models.Manifest manifest,
            string fileName)
        {
            //  Prepare folder structure

            //      The separator
            string[] sep = new string[] { "\\" };

            //      Split by directorty separator
            string[] parts = fileName.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            //      If there's more than one part, then the file is a subdirectory
            if (parts.Length > 1)
            {
                int i = 0;
                string treeDir = "";

                //  Loop through the directories
                for (i = 0; i < parts.Length - 1; i++)
                {
                    //  Setup the tree (parent and current directories)
                    treeDir += "\\" + parts[i];

                    //  If not exists, created
                    if (!Directory.Exists(treeDir))
                    {
                        Directory.CreateDirectory(treeDir);
                    } // end if not exists
                } // end for
            } // end if parts > 1

            //  Setup source and destination
            string source = string.Format("{0}/{1}/{2}", manifest.ftpcredentials.Url, version, fileName);
            string destination = fileName;

            //  Actually download the file
            http.Download(
                source,
                destination,
                manifest.ftpcredentials.User,
                manifest.ftpcredentials.Pwd
            ); // end download
        } // end function DownloadManifestFile
        
        /// <summary>
        /// Prints to screen and logs to file
        /// </summary>
        /// <param name="msg"></param>
        private void Log(string msg, params object[] args)
        {
            Console.WriteLine(string.Format(msg, args));
        } // end void

        /// <summary>
        /// Updates the application
        /// First read the manifest
        /// Then look for the latest version
        /// If the latest version is bigger than the current version, updates the application
        ///     Download new manifest
        ///     Compare the files in both manifests
        ///     If there's an upgrade, backup current file and download the new one
        /// </summary>
        public void Update()
        {
            //  Read the manifest
            this.ReadManifest();
            //  Get the current manifest
            string currentVersion = this.currentManifest.version;
            Log("Current version is {0}", currentVersion);

            //  This will download the files
            RealSimpleNet.Helpers.Http http = new RealSimpleNet.Helpers.Http();

            //  Donwload the latest flag
            http.Download(
                currentManifest.ftpcredentials.Url + "/latest",
                "latest.tmp",
                currentManifest.ftpcredentials.User,
                currentManifest.ftpcredentials.Pwd
            ); // end http download

            //  Read the downloaded flag
            string version = File.ReadAllText("latest.tmp");

            Log("Last version is {0}", version);

            //  Here we compare versions
            if (version.CompareTo(currentManifest.version) <= 0)
            {
                //  If same version, exit
                Log("Old version. Exiting");
                return;
            }

            Log("New version, updating");

            //  Another version
            //  Download new manifest
            http.Download(
                currentManifest.ftpcredentials.Url + "/" + version + "/manifest.json",
                "manifest.json.tmp",
                currentManifest.ftpcredentials.User,
                currentManifest.ftpcredentials.Pwd
            ); // end http download

            Log("New manifest downloaded");

            //  Load new manifest
            string tmpJson = File.ReadAllText("manifest.json.tmp");
            models.Manifest tmpManifest = serializer.Deserialize<models.Manifest>(tmpJson);
            tmpManifest.ftpcredentials.Decrypt();

            Log("New manifest loaded in memory. Start comparing files.");

            //  Compare files from new manifest with the oldone
            foreach (models.MonitoredFile file in tmpManifest.files)
            {
                //  Indicates if the file exist in the old manifest
                bool exists = false;

                //  Loop through the current files
                foreach (models.MonitoredFile localFile in currentManifest.files)
                {                   
                    //  If it's the same file 
                    if (file.filename.Equals(localFile.filename))
                    {
                        //  Exists in both manifests                        
                        exists = true;

                        //  Compare the checksum
                        if (!file.checksum.Equals(localFile.checksum))
                        {
                            Log("New version file {0}. Updating...", file.filename);

                            //  Here exists and has different chechsum
                            //      Backup the file, move it
                            this.BackupMonitoredFile(currentVersion, localFile.filename);
                            Log("{0} backed up.", file.filename);

                            //      Donwload new file
                            this.DownloadMonitoredFile(
                                ref http,
                                version,
                                currentManifest,
                                file.filename
                            ); // end DownloadMonitoredFile
                            Log("{0} downloaded.", file.filename);
                        } // end if diff checsum
                    } // end if same file
                } // end foreach local file

                //  If the file is new, downloaded
                if (!exists)
                {
                    Log("New file {0}. Downloading...", file.filename);
                    //  Donwload new file
                    this.DownloadMonitoredFile(
                        ref http,
                        version,
                        currentManifest,
                        file.filename
                    ); // end DownloadMonitoredFile
                    Log("{0} downloaded.", file.filename);
                } // end if not exists
            } // end foreach file

            // if the file exists in local, but not in remote, do nothing

            //  Remove temp files
            File.Delete("latest");
            File.Move("latest.tmp", "latest");
            File.Delete("manifest.json");
            File.Move("manifest.json.tmp", "manifest.json");

        } // void update
    } // end ArgumentsParser class
} // end namespace lib
