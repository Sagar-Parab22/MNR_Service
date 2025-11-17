using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class dputEmailStructure
    {
        public int ID { get; set; }
        public string FilePath { get; set; }
        public string Receiver { get; set; }
        public bool Status { get; set; }
        public DateTime SendDate { get; set; }
        public string KeyPath { get; set; }
        public string HostKeyFingerprint { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
