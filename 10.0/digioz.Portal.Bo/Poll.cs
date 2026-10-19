using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class Poll
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        
        [Required(ErrorMessage = "Poll question is required.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Poll question must be between 5 and 500 characters.")]
        public string Slug { get; set; }
        
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Featured { get; set; }
        public bool AllowMultipleOptionsVote { get; set; }
        public bool? Visible { get; set; }
        public bool? Approved { get; set; }
    }
}
