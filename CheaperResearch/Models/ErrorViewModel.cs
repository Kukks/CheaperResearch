using System;
using System.ComponentModel.DataAnnotations;

namespace CheaperResearch.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class CreateModule
    {
        [Required]
        public string Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        public string ParentId { get; set; }
    }
    
}