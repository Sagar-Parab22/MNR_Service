using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class StandardEdiDetail
    {
        public int EDIId { get; set; }
        public string UNB { get; set; }
        public string A1 { get; set; }
        public string UNH { get; set; }
        public string DTM { get; set; }
        public string RFF1 { get; set; }
        public string RFF { get; set; }
        public string ACA { get; set; }
        public string LBR { get; set; }
        public string G1NAD { get; set; }
        public string G2NAD { get; set; }
        public string EQF { get; set; }
        public string ECI { get; set; }
        public string CUI { get; set; }
        public string DAM { get; set; }
        public string CTO { get; set; }
        public string TMA { get; set; }
        public string UNT { get; set; }
        public string UNZ { get; set; }
        public string Operator { get; set; }
    }
}
