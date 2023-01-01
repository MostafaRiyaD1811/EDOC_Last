using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Document.Models
{
    public class Requester
    {
        [Display(Name = "Code")]
        [Key]
        public string EmployeeCode { get; set; }
        public string? Position { get; set; }
        public string? Dept { get; set; }
        public string? Name { get; set; }
        public string? MgrName { get; set; }
        public string? MgrCode { get; set; }
        public string? LDap { get; set; }
        public string? Company { get; set; }
        //public string? ModifiedBy {get; set;}
        //public string? ModifiedAt { get; set;}

        //[InverseProperty("CurrentReviewer")]
        //public virtual ICollection<CarRequest>? CarRequestsUnderReview { get; set; } = new HashSet<CarRequest>();
        
        public virtual ICollection<Voucher>? Voucher { get; set; } = new HashSet<Voucher>();
        public virtual ICollection<DomainAccount>? DomainAccount { get; set; } = new HashSet<DomainAccount>();
        public virtual ICollection<JobPlanUpdate>? JobPlanUpdate { get; set; } = new HashSet<JobPlanUpdate>();
        public virtual ICollection<TravelDesk>? TravelDesk { get; set; } = new HashSet<TravelDesk>();
        public virtual ICollection<PR>? PR { get; set; } = new HashSet<PR>();
        public virtual ICollection<Requester>? ManagedEmployees { get; set; } = new HashSet<Requester>();
        public int HRUser { get; set; }
        public int HRReviewer {get; set;}
        public int HRMgr {get; set;}
        public int FinanceInitiator { get; set; }
        public int FinanceReviewr { get; set; }
        public int FinanceMgr { get; set; }
        public int ITMgr { get; set; }
        public int ITAdmin { get; set; }
        public int? Adminstration { get; set; }
        public int Transportation { get; set; }
        public int? SystemAdmin {get; set;}

        public string? ChangedBy {get; set;}
        public DateTime? LastUpdated { get; set; }


        public int TechnicalPlanning { get; set; }

        //public string? HRUser { get; set; }
        //public string? HRReviewer { get; set; }
        //public string? HRmanager{ get; set; }
        public Requester? Manager { get; set; }

      
    }
}
