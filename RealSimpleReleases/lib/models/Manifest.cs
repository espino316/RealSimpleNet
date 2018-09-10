using System.Collections.Generic;

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
    }
}
