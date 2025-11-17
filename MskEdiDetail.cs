using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class MskEdiDetail
    {
        public int EDIId { get; set; }
        public string GeoCodeTranslation { get; set; }
        public string EstimateNo { get; set; }
        public string EstimateDt { get; set; }
        public string ContainerNo { get; set; }
        public string Mode { get; set; }
        public string DmgReason { get; set; }
        public string LocationCode { get; set; }
        public string TotalLHrs { get; set; }
        public decimal OTHrs { get; set; }
        public decimal DTHrs { get; set; }
        public decimal MISCtHrs { get; set; }
        public decimal LabourAmount { get; set; }
        public string TotalValue { get; set; }
        public string RpCode { get; set; }
        public string CompCode { get; set; }
        public string qty { get; set; }
        public string rowTotal { get; set; }
        public string rowLhrs { get; set; }
        public string Responsible { get; set; }
        public string PQt1 { get; set; }
        public string PartNo { get; set; }
        public string PQt2 { get; set; }
        public string PartNo2 { get; set; }
        public string PQt3 { get; set; }
        public string PartNo3 { get; set; }
        public string PQt4 { get; set; }
        public string PartNo4 { get; set; }
        public string PQt5 { get; set; }
        public string PartNo5 { get; set; }
        public long EstimateId { get; set; }
        public long EstimateDTLId { get; set; }
        public string Comments { get; set; }
    }
}
