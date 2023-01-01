using Document.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.ViewModels
{
    public class PRViewModel
    {
        [DisplayName("Request Number")]        
        public int ReqNum { get; set; } = RandomNumberGenerator.GetInt32(0,int.MaxValue);

        [Display(Name = "Approved At")]
        public DateTime ApprovedAt { get; set; }

        [DisplayName("Vendor Name")]
        [Required]
        public string? VendorName { get; set; }

        [DisplayName("Vendor Number")]
        [Required]
        public string? VendorNum { get; set; }

        [DisplayName("PO Description")]
        public string? CreationNotes { get; set; }

        public Guid Id { get; set; }

        [Required]
        [DisplayName("PO No.")]
        [RegularExpression(@"^(SOK.PO.)[0-9]{2}-[0-9]+$")]

        public string? PONo { get; set; }
        [DisplayName("Invoice No.")]
        [Required]
        public string? InvoiceNo { get; set; }
        [DisplayName("PO")]
        [NotMapped]
        [Required]

        // [RegularExpression(@"^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|docx|DOCX)$", ErrorMessage = "The File  has a not valid extention")]
        [AllowedExtensions(new string[] { ".doc", ".pdf", ".docx" })]
        [Display(Name = "PO Attach (Allowed Extensions(.doc, .pdf, .docx))")]

        public IFormFile? POAttach { get; set; }
        [DisplayName("Invoice")]
        [NotMapped]
        [Required]
        //[RegularExpression(@"^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|docx|DOCX)$", ErrorMessage = "The File  has a not valid extention")]
        [AllowedExtensions(new string[] { ".doc", ".pdf", ".docx" })]
        [Display(Name = "Invoice Attach (Allowed Extensions(.doc, .pdf, .docx))")]

        public IFormFile? InvoiceAttach { get; set; }
        public string? Status { get; set; }
        [NotMapped]
        [AllowedExtensions(new string[] {".doc", ".pdf", ".docx"})]
        [Display(Name = "Other Attach (Allowed Extensions(.doc, .pdf, .docx))")]

        public IFormFile? OtherAttach { get; set; }
        [Display(Name = "Approval Notes")]
        public String? ApprovalNotes { get; set; }
        public FinanceReviewerStatuses FinanceReviewerStatus {get; set;}
        [ForeignKey("Requester")]

        public string? RequesterId { get; set; }

        [Display(Name = "Assignee")]

        public string? CurrentReviewerId { get; set; }
        public string? Attach1 { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Attach2 { get; set; }
        public string? Attach3 { get; set; }
    }
}
