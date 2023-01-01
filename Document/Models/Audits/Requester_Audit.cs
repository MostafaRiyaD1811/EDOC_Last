using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Document.Models
{
    public class Requester_Audit
    {
        [Key]

        public Guid Id { get; set; } = new Guid();

        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Position { get; set; }
        public string? Dept { get; set; }
        public string? Name { get; set; }
        public string? MgrCode { get; set; }

        public string? MgrName { get; set; }     
        public string? LDap { get; set; }
        public string? Company { get; set; }
       
        public int HRUser { get; set; }
        public int HRReviewer {get; set;}
        public int HRMgr {get; set;}
        public int FinanceInitiator { get; set; }
        public int FinanceReviewr { get; set; }
        public int FinanceMgr { get; set; }
        public int ITMgr { get; set; }
        public int ITAdmin { get; set; }
        public int? SystemAdmin {get; set;}
        public int? Adminstration { get; set; }
        public DateTime? LastUpdated { get; set; }


        public int TechnicalPlanning { get; set; }

       
        //public string? HRUser { get; set; }
        //public string? HRReviewer { get; set; }
        //public string? HRmanager{ get; set; }
        public Requester? Manager { get; set; }

        public string Describe()
        {
            throw new NotImplementedException();
        }
    }
}
