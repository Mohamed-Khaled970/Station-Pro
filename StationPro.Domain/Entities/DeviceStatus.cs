using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Entities
{
    public enum DeviceStatus
    {
        Available = 1,
        InUse = 2,
        Maintenance = 3,
        Offline = 4
    }
}
