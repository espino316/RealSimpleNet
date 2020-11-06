using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;

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
                
        public void UpdateManifestFiles()
        {
            string dir = Directory.GetCurrentDirectory() + "\\";
            ReadManifest();
            this.ParseDirectory(dir, dir, ref this.currentManifest);
        }

        private void SaveManifest(models.Manifest manifest)
        {
            manifest.ftpcredentials.Encrypt();
            string json = serializer.Serialize(manifest);
            json = RealSimpleNet.Helpers.Json.PrettyPrint(json);

            File.WriteAllText(
                "manifest.json",
                json
            ); // end write all text
        }

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

            this.ParseDirectory(dir, dir, ref manifest);

            //  Saves the manifest to file
            SaveManifest(manifest);
        } // end function init

        public void ReadManifest()
        {
            if (!File.Exists("manifest.json"))
            {
                throw new Exception("Manifest not present. You're looking to configure a new release? Try with 'init' command.");
            }
            string json = File.ReadAllText("manifest.json");
            //json = RealSimpleNet.Helpers.Crypt.Decrypt(json);
            currentManifest = serializer.Deserialize<models.Manifest>(json);
            currentManifest.ftpcredentials.Decrypt();
        }

        public void ParseDirectory(string rootDir, string dir, ref models.Manifest manifest)
        {
            string[] files = Directory.GetFiles(dir);
            string[] directories = Directory.GetDirectories(dir);

            foreach (string file in files)
            {
                Log("Parsing file: {0}", file);

                if (
                    file.ToLower() == (AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName).ToLower() ||
                    file.ToLower() == (AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".dll")).ToLower()
                )
                {
                    Console.WriteLine("Ignoring " + AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName);
                    continue;
                } // end if same name

                manifest.AddFile(
                    new models.MonitoredFile(
                        file.Replace(rootDir, ""),
                        RealSimpleNet.Helpers.Crypt.Checksum(file)
                    ) // end new model.MonitoredFile
                ); // end manifest add

                string main = manifest.main;
                if (manifest.files.Find(f => f.filename == main) == null)
                {
                    throw new Exception(string.Format("Main file '{0}' not present in manifest", main));
                } // end if main is not in the manifest
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
            if (version == null)
            {
                Log("No version specified, using 1.0.0.0");
                version = "1.0.0.0";
            }

            if (File.Exists("latest"))
            {
                string latest = File.ReadAllText("latest");
                if (latest == version)
                {                    
                    version = UpgradeVersionNumber(version);
                    Log(string.Format("Same version. Upgrading version to {0}", version));
                } // end if latest = version
            } // end if latest exists

            File.WriteAllText("latest", version);

            //  Look for changes
            UpdateManifestFiles();

            //  Set new version
            currentManifest.version = version;

            RealSimpleNet.Helpers.FTP ftp = new RealSimpleNet.Helpers.FTP();
            ftp.Server = currentManifest.ftpcredentials.Url;
            ftp.User = currentManifest.ftpcredentials.User;
            ftp.Pwd = currentManifest.ftpcredentials.Pwd;

            //  Save new version
            SaveManifest(currentManifest);

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

                //  Get the destination file
                string destination = version + "/" + file.filename.Replace("\\", "/");

                //  Upload via ftp
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
            if (!File.Exists(fileName))
            {
                //throw new Exception("File do not exists and cannot be moved " + fileName);
                Log("File do not exists. Don't need to backup");
                return;
            } // end if file not exists

            //  Check do not exists already
            if (File.Exists(string.Format("{0}\\{1}", version, fileName)))
            {
                File.Delete(string.Format("{0}\\{1}", version, fileName));
            } // end if file exists

            //  Actually move the file
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
            Console.WriteLine("Download Monitored file: " + fileName);
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
            string destination = fileName + ".tmp";

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
        /// Validates all files in manifest exists and ok (checksum verified)
        /// </summary>
        /// <param name="manifest">The manifest to validate</param>
        private void ValidateManifest(models.Manifest manifest)
        {
            manifest.files.ForEach(f => {
                if (!File.Exists(f.filename))
                {
                    throw new FileNotFoundException(string.Format("{0} does not exists", f.filename));
                } // end if file do not exists

                if (f.filename != "manifest.json" && f.checksum != RealSimpleNet.Helpers.Crypt.Checksum(f.filename))
                {
                    throw new Exception(string.Format("Checksum of file {0} does not match", f.filename));
                } // end if check sum do not match
            }); // end for each file in the manifest
        } // end function ValidateManifest

        /// <summary>
        /// Upgrades the application
        /// First read the manifest
        /// Then look for the latest version
        /// If the latest version is bigger than the current version, updates the application
        ///     Download new manifest
        ///     Compare the files in both manifests
        ///     If there's an upgrade, backup current file and download the new one
        /// </summary>
        public void Upgrade()
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
            if (version.CompareTo(currentManifest.version) < 0)
            {
                //  If same version, exit
                Log("Old version. Exiting");
                return;
            }

            if (version.CompareTo(currentManifest.version) == 0)
            {
                //  If same version, exit
                Log("Same version. Check if local files exists");

                //  Check files in manifest exists
                bool existFiles = true;
                currentManifest.files.ForEach(f => existFiles = existFiles && File.Exists(f.filename));

                if (existFiles)
                {
                    Log("Local files exists. Exiting");
                    StartMain(currentManifest);
                    return;
                } else
                {
                    Log("Local files do not exists. Upgrading.");
                } // end if then else existFiles
            } // end if same version

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

            List<models.MonitoredFile> updatedFiles = new List<models.MonitoredFile>();

            //  Compare files from new manifest with the oldone
            foreach (models.MonitoredFile file in tmpManifest.files)
            {
                Log("Comparing " + file.filename + "...");
                models.MonitoredFile localFile = null;
                localFile = currentManifest.files.FirstOrDefault(f => f.filename == file.filename);

                if (localFile == null || !File.Exists(file.filename))
                {
                    Log("New file {0}. Downloading...", file.filename);
                    //  Donwload new file
                    this.DownloadMonitoredFile(
                        ref http,
                        version,
                        currentManifest,
                        file.filename
                    ); // end DownloadMonitoredFile
                    //  Add to the list
                    updatedFiles.Add(file);
                    Console.WriteLine("add updated file " + file.filename);
                    Log("{0} downloaded.", file.filename);
                } else
                {
                    //  Compare the checksum
                    if (!file.checksum.Equals(localFile.checksum))
                    {
                        Log("New version file {0}. Updating...", file.filename);
                        //      Donwload new file
                        this.DownloadMonitoredFile(
                            ref http,
                            version,
                            currentManifest,
                            file.filename
                        ); // end DownloadMonitoredFile

                        //  Add to the list
                        updatedFiles.Add(file);
                        Console.WriteLine("add updated file " + file.filename);

                        Log("{0} downloaded.", file.filename + ".tmp");
                    } // end if diff checsum
                } // end if then else local file is null
            } // end foreach file

            //  Loop the updated files
            //  Backup the files
            //  Rename the files
            updatedFiles.ForEach(f => {

                Console.WriteLine("updateFile: " + f.filename);

                //  Here exists and has different chechsum
                //      Backup the file, move it
                this.BackupMonitoredFile(currentVersion, f.filename);

                Log("{0} backed up.", f.filename);

                //  Delete if exists
                if (File.Exists(f.filename))
                {
                    File.Delete(f.filename);
                } // end if file exists

                //  Rename the downloaded file is exists
                if (!File.Exists(f.filename + ".tmp"))
                {
                    throw new Exception(string.Format("{0} file was not downloaded correctly", f.filename));
                } // end if not exists downloaded file

                //  Rename the files
                Console.WriteLine("Moving file " + f.filename + ".tmp");
                File.Move(f.filename + ".tmp", f.filename);
            }); // end for each file

            // if the file exists in local, but not in remote, do nothing

            //  Validates the latest manifest
            Log("Validating the manifest");
            ValidateManifest(tmpManifest);
            StartMain(currentManifest);
        } // void update

        /// <summary>
        /// Validates main exists, and if so, start the main process
        /// </summary>
        /// <param name="currentManifest"></param>
        private void StartMain(models.Manifest currentManifest)
        {
            //  Validate main exists
            if (!File.Exists(currentManifest.main))
            {
                throw new Exception(string.Format("Main file '{0}' do not exists", currentManifest.main));
            }
            else
            {
                Log("Starting main {0}", currentManifest.main);
                Process.Start(currentManifest.main);                
            } // end if file exists
        } // end function StartMain

        private string UpgradeVersionNumber(string versionNumber)
        {
            string[] numbers = versionNumber.Split('.');
            int lastVersionNumber;
            string result;

            if (int.TryParse(numbers[numbers.Length - 1], out lastVersionNumber))
            {
                lastVersionNumber++;
                numbers[numbers.Length - 1] = lastVersionNumber.ToString();
                result = string.Join(".", numbers);
            }
            else
            {
                throw new Exception(string.Format("Version elements are not integers '{0}'", versionNumber));
            }// end if versionNumber is int

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Error forming the version number");
            }

            //  Return result
            return result;
        } // end function UpgradeVersionNumber
    } // end ArgumentsParser class
} // end namespace lib
