using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class AspNetUserToken
    {
        public string UserId { get; set; }
        [MaxLength(128)]
        public string LoginProvider { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual AspNetUser User { get; set; }
    }
}
