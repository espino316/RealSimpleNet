using RealSimpleNet.Helpers;

namespace RealSimpleReleases.lib.models
{
    class Credentials
    {
        private string url;
        private string user;
        private string pwd;

        public string Url { get; set; }

        public string User { get; set; }

        public string Pwd { get; set; }

        public Credentials() { }

        public Credentials(string url, string user, string pwd)
        {
            this.Url = url;
            this.User = user;
            this.Pwd = pwd;
        }

        public void Encrypt()
        {
            this.Url = Crypt.Encrypt(this.Url);
            this.User = Crypt.Encrypt(this.User);
            this.Pwd = Crypt.Encrypt(this.Pwd);
        }

        public void Decrypt()
        {
            this.Url = Crypt.Decrypt(this.Url);
            this.User = Crypt.Decrypt(this.User);
            this.Pwd = Crypt.Decrypt(this.Pwd);
        }
    }
}
