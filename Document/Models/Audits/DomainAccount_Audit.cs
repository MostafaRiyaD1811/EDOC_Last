using Document.ViewModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Document.Models
{
    public class DomainAccount_Audit
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? RequesterName { get; set; }

        public string? RequesterPhone { get; set; }
        public string? RequesterCode { get; set; }
        public string? RequesterExtension { get; set; }
        public string? RequesterDept { get; set; }
        public string? RequesterJobTitle{ get; set; }
        public string? RequesterCompany { get; set; }

        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public string? DisplayName { get; set; } 
        public string? OU { get; set; }
        public string? LoginName { get; set; }
        public Guid RequestId { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }

        public String? ApprovalNotes { get; set; }
        public ITAdminStatuses ITAdminStatus { get; set; }

        public ITMgrStatuses ITMgrStatus { get; set; }
        public MgrStatuses MgrStatus { get; set; }

        public string? RequesterId { get; set; }
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
    }

}
    

