using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.Models
{
    public class Voucher
    {

        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid Id { get; set; } = new Guid();

        [Required]
        [Display(Name = "Vendor Name")]
        [DataType(DataType.Text)]
        [StringLength(60, MinimumLength = 3)]
        public string? VendorName { get; set; }
        [DisplayName("Vendor Number")]
        [Required]
        public string? VendorNum { get; set; }

        [Required]               
        [Column(TypeName = "decimal(18, 2)")]
        [Range(1, 100000000)]

        public decimal Amount  { get; set; }
        [Display(Name = "Invoice Date")]
        [Required]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        [Remote("IsValidVDate", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime InvoiceDate { get; set; }

        public string? Details { get; set; }
        [Display(Name = "Beneficiary Name")]
        [Required]
        public string? BeneficiaryName { get; set; }

        [Display(Name = "Created At")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Display(Name = "Approved/Declined")]
        public string? Status { get; set; }
      
        [Display(Name = "Approval Notes")]
        public string? ApprovalNotes { get; set; }

        [Display(Name = "Status")]
        public string? Approved { get; set; }

        [Display(Name = "Other Notes")]
        public string? OtherNotes { get; set; }

        [Required]
        public string? Type { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Voucher Currency")]
        [Required]
        public string? VoucherCurrency { get; set; }
       
        public HRReviewerStatuses HRReviewerStatuts { get; set; }
        public HRMgrStatutses HRMgrStatuts { get; set; }

        public FinanceInitiatorStatuses FinanceInitiatorStatus { get; set; }
        public FinanceReviewerStatuses FinanceReviewerStatus { get; set; }
        public FinanceMgrStatuses FinanceMgrStatus { get; set; }

        public virtual Requester? Requester { get; set; }
        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public virtual string? RequesterId { get; set; }

        [Display(Name = "Assignee")]

        public string? CurrentReviewerId { get; set; }
        public string? ChangedBy { get; set; }


    }
    public enum HRReviewerStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum FinanceInitiatorStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum FinanceReviewerStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum FinanceMgrStatuses
    {
        None,
        Pending,
        Approve, Decline
    }
    public enum HRMgrStatutses
    {
        None,
        Pending,
        Approve, Decline
    }
}
