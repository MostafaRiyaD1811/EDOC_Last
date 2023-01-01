using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Document.Models
{
    public class PR_Audit
    {
        [Key]

        public Guid Id { get; set; } = new Guid();

        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0,int.MaxValue);
        public Guid RequestId { get; set; }= Guid.NewGuid();
        public string? PONo { get; set; }
        public string? VendorNum { get; set; }

        public string? InvoiceNo { get; set; }
        public string? VendorName { get; set; }
        public string? POAttach { get; set; }
        public string? CreationNotes { get; set; }

        public string? InvoiceAttach { get; set; }
        public string? OtherAttach { get; set; }
        public String? ApprovalNotes { get; set; }
        public DateTime ApprovedAt { get; set; } 
        public FinanceReviewerStatuses FinanceReviewerStatus { get; set; }
        public string? RequesterId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Status { get; set; }


        public string? CurrentReviewerId { get; set; }
    }                
}
