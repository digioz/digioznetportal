using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace digioz.Portal.Bo
{
    public partial class AspNetUserLogin
    {
        [MaxLength(128)]
        public string LoginProvider { get; set; }
        [MaxLength(128)]
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        public string UserId { get; set; }

        public virtual AspNetUser User { get; set; }
    }
}
