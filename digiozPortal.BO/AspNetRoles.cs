using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

namespace digiozPortal.BO
{
    [Dapper.Contrib.Extensions.Table("AspNetRoles")]
    public partial class AspNetRoles
    {
        [ExplicitKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }

    }
}
