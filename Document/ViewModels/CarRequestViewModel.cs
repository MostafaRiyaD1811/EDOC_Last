using Document.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.ViewModels
{
    public class CarRequestViewModel 
    {
        
        public Guid Id { get; set; }
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);

        [Display(Name = "Requester Name")]
        public string? RequesterName { get; set; }
        [Display(Name = "Requested For")]
        [Required]
        [RegularExpression(@"[a-zA-Z\s\n,]{1,300}" , ErrorMessage ="Enter a Valid Name without numeric characters")]
        public string? Name { get; set; }
       
        [Display(Name = "Mobile No.")]
        [Required]
        [MaxLength(15)]
        [MinLength(10)]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\s ?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s? (x|ext\.?)\s?\d+)?$", ErrorMessage = "The PhoneNumber field is not a valid phone number")]
        public string? MobileNo { get; set; }
        [Display(Name = "Departure address")]
        [Required]
        public string? DepartureAddress { get; set; }
        [Display(Name = "Destination address")]
        [Required]
        public string? DestinationAddress { get; set; }

        [Remote("IsValidDate", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        [Display(Name = "Departure date/time")]
        [Required]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime")]

        public DateTime Departure { get; set; }
        [Display(Name = "Return back date/time")]
        [Required]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        [Remote("IsValidDateRet", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
      
        public DateTime ReturnBack { get; set; }
        [Display(Name = "Luggage description (Number/Size)")]
        [Required]
        public string? LuggageDescription { get; set; }

        [Display(Name = "Car type")]
        [Required]
        public string? CarType { get; set; }
        public string? Attach1 { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Type of mission")]
        public string? Justification { get; set; }

        [Display(Name = "Code")]
        public string? CreatorId { get; set; }

        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public string? Status { get; set; }

        public string? Notes { get; set; }
       
       //[RegularExpression(@"^[a-zA-Z\s,-.]{1,60}.(doc|DOC|pdf|PDF|docx|DOCX)$", ErrorMessage = "The file type is not valid ")]
        [AllowedExtensions(new string[] { ".doc", ".pdf", ".docx" })]
        [Display(Name = "Attachment (Allowed Extensions(.doc, .pdf, .docx))")]

        public IFormFile? Attach { get; set; }

        [Display(Name = "Approval")]
        public DeptAdminStatutses DeptAdminStatuts { get; set; }
        public TransTeamLeaderStatuses TransTeamLeaderStatus { get; set; }
        public MgrStatutses MgrStatuts {get; set;}

        [ForeignKey("CurrentReviewer")]
        public string? CurrentReviewerId {get; set;}
        public virtual Requester? CurrentReviewer { get; set; }
        public virtual Requester? Requester { get; set; }

        
    }
    public class AllowedFilesExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedFilesExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var file = value as List<IFormFile>;

            if (file != null)
            {
                foreach (var item in file)
                {
                    var extension = Path.GetExtension(item.FileName);
                    if (!_extensions.Contains(extension.ToLower()))
                    {
                        return new ValidationResult(GetErrorMessage());
                    }
                }
               
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"The file type is not valid";
        }
    }
    public enum DeptAdminStatutses
    {
        None,
        Pending,
        Approve, Decline
    }
    public enum MgrStatutses
    {
        None,
        Pending,
        Approve, Decline
    }
}

