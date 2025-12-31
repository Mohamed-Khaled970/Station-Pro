using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Common
{
    public interface ITenantEntity
    {
        int TenantId { get; set; }
    }
}
