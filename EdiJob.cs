using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class EdiJob
    {
        public long EDILogId { get; set; }
        public string Email { get; set; }
        public int GeoId { get; set; }
        public string GeoCode { get; set; }
        public int EDITypeId { get; set; }
        public string Operator { get; set; }
        public int CommunicationID { get; set; }
        public string FTPPath { get; set; }
    }
}
