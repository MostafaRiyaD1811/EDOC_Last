using Document.ViewModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Document.Models
{
    public class DomainAccount
    {
        [Required]
        [Display(Name = "Requester")]
        [RegularExpression(@"[a-zA-Z\s]{1,50}", ErrorMessage = "Enter a Valid Name without numeric characters")]
        public string? RequesterName { get; set; }

        [Display(Name = "Phone")]
        [Required]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(15)]
        [MinLength(10)]
        [RegularExpression(@"^(\+\s ?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s? (x|ext\.?)\s?\d+)?$", ErrorMessage = "The PhoneNumber field is not a valid phone number")]
        public string? RequesterPhone { get; set; }
        [Display(Name = "Requester Id")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Enter a Valid Code")]
        [MaxLength(5)]
        [Required]
        public string? RequesterCode { get; set; }

        [Display(Name = "Requester Extension")]
        [RegularExpression("^[0-9]*$")]
        [MaxLength(4)]
        public string? RequesterExtension { get; set; }

        [Required]
        [Display(Name = "Department")]
        public string? RequesterDept { get; set; }
        [Required]
        [Display(Name = "Job Title")]
        public string? RequesterJobTitle{ get; set; }
        [Required]
        [Display(Name = "Requester Company")]
        public string? RequesterCompany { get; set; }

        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        [DisplayName("Display Name")]
        public string? DisplayName { get; set; } 
        public string? OU { get; set; }
        [DisplayName("Login Name")]
        public string? LoginName { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        [Display(Name = "Created At")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Assignee")]
        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }

        [Display(Name = "Approval Notes")]
        public String? ApprovalNotes { get; set; }
        public ITAdminStatuses ITAdminStatus { get; set; }

        public ITMgrStatuses ITMgrStatus { get; set; }
        public MgrStatuses MgrStatus { get; set; }

        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        [ForeignKey("CurrentReview")]
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
        public string? ChangedBy { get; set; }
    }


    public enum ITAdminStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum ITMgrStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
}
    

