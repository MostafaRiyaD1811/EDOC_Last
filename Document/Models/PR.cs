using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Document.Models
{
    public class PR
    {
         [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0,int.MaxValue);
        public Guid Id { get; set; }= Guid.NewGuid();
        [DisplayName("PO No.")]
        [System.ComponentModel.DataAnnotations.Required]
        [RegularExpression(@"^(SOK.PO.)[0-9]{2}-[0-9]+$")]
        public string? PONo { get; set; }
        [DisplayName("Vendor Number")]
        [System.ComponentModel.DataAnnotations.Required]
        public string? VendorNum { get; set; }
        [DisplayName("Invoice No.")]
        [System.ComponentModel.DataAnnotations.Required]

        public string? InvoiceNo { get; set; }
        [DisplayName("Vendor Name")]
        [System.ComponentModel.DataAnnotations.Required]
        public string? VendorName { get; set; }
        [DisplayName("PO")]
        public string? POAttach { get; set; }
        [DisplayName("PO Description")]
        public string? CreationNotes { get; set; }

        [DisplayName("Invoice")]
        public string? InvoiceAttach { get; set; }
        [DisplayName("Other Attachment")]
        public string? OtherAttach { get; set; }
        [Display(Name = "Approval Notes")]
        public String? ApprovalNotes { get; set; }
        [Display(Name = "Approved At")]
        public DateTime ApprovedAt { get; set; } 
        public FinanceReviewerStatuses FinanceReviewerStatus { get; set; }
        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Status { get; set; }

        [Display(Name = "Assignee")]

        public string? CurrentReviewerId { get; set; }
        public string? ChangedBy { get; set; }
    }                
}
