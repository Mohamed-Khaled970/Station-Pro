using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<(bool Success, int TenantId, string Error)> RegisterAsync(string storeName, string email, string phone, string password);
        Task<(bool Success, int TenantId, string Error)> LoginAsync(string email, string password);
        Task<(bool Success, string Error)> ChangePasswordAsync(int tenantId, string currentPassword, string newPassword);
        Task<(bool Success, string Error)> ForgotPasswordAsync(string email);
        Task<(bool Success, string Error)> ResetPasswordAsync(string token, string newPassword);
        Task<bool> IsResetTokenValidAsync(string token);
        Task<bool> IsTenantActiveAsync(int tenantId);
    }
}
