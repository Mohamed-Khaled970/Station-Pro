using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITenantRepository _tenantRepo;
        private readonly IEmailService _email;

        public AuthService(ITenantRepository tenantRepo, IEmailService email)
        {
            _tenantRepo = tenantRepo;
            _email = email;
        }



        // ── Register ──────────────────────────────────────────────────────────
        public async Task<(bool Success, int TenantId, string Error)> RegisterAsync(
            string storeName, string email, string phone, string password)
        {
            if (await _tenantRepo.EmailExistsAsync(email))
                return (false, 0, "An account with this email already exists.");

            var tenant = new Tenant
            {
                Name = storeName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Plan = SubscriptionPlan.Free,   // internal placeholder — admin sets real plan on approval
                IsActive = false,                   // requires admin approval
                CreatedAt = DateTime.UtcNow
            };

            await _tenantRepo.AddAsync(tenant);
            return (true, tenant.Id, string.Empty);
        }



        // ── Login ─────────────────────────────────────────────────────────────
        public async Task<(bool Success, int TenantId, string Error)> LoginAsync(
            string email, string password)
        {
            var tenant = await _tenantRepo.GetByEmailAsync(email);

            if (tenant == null || !BCrypt.Net.BCrypt.Verify(password, tenant.PasswordHash))
                return (false, 0, "Invalid email or password.");

            if (!tenant.IsActive)
                return (false, 0, "Your account is pending admin approval.");

            return (true, tenant.Id, string.Empty);
        }


        // ── Change Password ───────────────────────────────────────────────────
        public async Task<(bool Success, string Error)> ChangePasswordAsync(
            int tenantId, string currentPassword, string newPassword)
        {
            var tenant = await _tenantRepo.GetByIdAsync(tenantId);
            if (tenant == null)
                return (false, "Account not found.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, tenant.PasswordHash))
                return (false, "Current password is incorrect.");

            tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepo.UpdateAsync(tenant);
            return (true, string.Empty);
        }

        // ── Forgot Password ───────────────────────────────────────────────────
        public async Task<(bool Success, string Error)> ForgotPasswordAsync(string email)
        {
            var tenant = await _tenantRepo.GetByEmailAsync(email);

            if (tenant == null)
                return (true, string.Empty);   // don't reveal whether email exists

            var token = Convert.ToHexString(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

            tenant.PasswordResetToken = token;
            tenant.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepo.UpdateAsync(tenant);

            var resetLink = $"https://stationpro.com/Auth/ResetPassword?token={token}";
            await _email.SendAsync(
                to: tenant.Email,
                subject: "Reset your StationPro password",
                body: $"Click the link to reset your password (expires in 1 hour):\n\n{resetLink}"
            );

            return (true, string.Empty);
        }

        // ── Reset Password ────────────────────────────────────────────────────
        public async Task<(bool Success, string Error)> ResetPasswordAsync(
            string token, string newPassword)
        {
            var tenant = await _tenantRepo.GetByResetTokenAsync(token);
            if (tenant == null)
                return (false, "Invalid or expired reset link. Please request a new one.");

            tenant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            tenant.PasswordResetToken = null;
            tenant.PasswordResetTokenExpiry = null;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _tenantRepo.UpdateAsync(tenant);
            return (true, string.Empty);
        }

        // ── Token validation ──────────────────────────────────────────────────
        public async Task<bool> IsResetTokenValidAsync(string token)
            => await _tenantRepo.GetByResetTokenAsync(token) != null;

        public async Task<bool> IsTenantActiveAsync(int tenantId)
        {
            var tenant = await _tenantRepo.GetByIdAsync(tenantId);
            return tenant?.IsActive ?? false;
        }
    }
}
