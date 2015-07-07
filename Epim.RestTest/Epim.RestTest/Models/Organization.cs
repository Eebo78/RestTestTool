using System.Security.Cryptography.X509Certificates;

namespace Epim.RestTest.Models
{
    public class Organization
    {
        private string _name;
        public string Name {
            get { return _name; }
            set
            {
                var tmp = value;
                _name = tmp.Substring(0, 1).ToUpper() + tmp.Substring(1, tmp.Length - 1).ToLower();
            }
        }
        public long OrgNo { get; set; }
        public long Gln { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
}
