using StationPro.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        public Task SendAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}
