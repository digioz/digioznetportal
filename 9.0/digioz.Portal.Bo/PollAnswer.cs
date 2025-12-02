using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class PollAnswer
    {
        public string Id { get; set; }
        public string PollId { get; set; }
        
        [Required(ErrorMessage = "Answer text is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Answer must be between 1 and 200 characters.")]
        public string Answer { get; set; }
    }
}
