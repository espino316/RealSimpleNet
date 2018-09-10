using System;

namespace RealSimpleReleases.lib.models
{
    class MonitoredFile
    {
        public string filename { get; set; }
        public string checksum { get; set; }

        public MonitoredFile(string filename, string checksum)
        {
            this.filename = filename;
            this.checksum = checksum;
        }

        public MonitoredFile() { }
    }
}
