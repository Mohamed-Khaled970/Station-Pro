using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class EndSessionRequest
    {
        public int SessionId { get; set; }

        /// <summary>1 = Cash, 2 = Card</summary>
        public int PaymentMethod { get; set; } = 1;
    }
}
