using System.Collections.Generic;
using System.Linq;

namespace RealSimpleReleases.lib.models
{
    class Manifest
    {
        public string appname { get; set; }
        public string version { get; set; }
        public string main { get; set; }
        public Credentials ftpcredentials { get; set; }
        public List<MonitoredFile> files { get; set; }

        public Manifest()
        {
            this.files = new List<MonitoredFile>();
            this.ftpcredentials = new Credentials();
        }

        /// <summary>
        /// Adds a file to the manifest, if new, else, overwrites its checksum
        /// </summary>
        /// <param name="file">The MonitoredFile to add</param>
        public void AddFile(MonitoredFile file)
        {
            if (files == null)
            {
                files = new List<MonitoredFile>();
            } // end if files is null

            MonitoredFile current = files.FirstOrDefault(f => f.filename == file.filename);

            if (current != null)
            {
                current.checksum = file.checksum;
                return;
            } // end if current not null

            //  If reaches here, is a new file
            files.Add(file);

        } // end function AddFile
    } // end class Manifest
} // end namespace lib.Models
