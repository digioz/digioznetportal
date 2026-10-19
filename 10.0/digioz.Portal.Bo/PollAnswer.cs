using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class PollAnswer
    {
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string PollId { get; set; }
        
        [Required(ErrorMessage = "Answer text is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Answer must be between 1 and 200 characters.")]
        public string Answer { get; set; }
    }
}
