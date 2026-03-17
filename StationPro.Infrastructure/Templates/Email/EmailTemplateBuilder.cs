// =============================================================================
// FILE: StationPro.Infrastructure/Templates/Email/EmailTemplateBuilder.cs
// =============================================================================

namespace StationPro.Infrastructure.Templates.Email
{
    public static class EmailTemplateBuilder
    {
        // ── Forgot Password ───────────────────────────────────────────────────
        public static string BuildForgotPasswordEmail(string resetLink)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>Reset your StationPro password</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ margin: 0 !important; padding: 0 !important; background: #0f172a; }}
        img {{ border: 0; outline: none; text-decoration: none; }}
        table {{ border-collapse: collapse !important; }}

        @media only screen and (max-width: 600px) {{
            .email-wrapper  {{ padding: 16px !important; }}
            .email-card     {{ border-radius: 12px !important; }}
            .email-header   {{ padding: 24px 20px !important; }}
            .email-body     {{ padding: 24px 20px !important; }}
            .email-footer   {{ padding: 16px 20px !important; }}
            .logo-text      {{ font-size: 22px !important; }}
            .logo-icon      {{ font-size: 32px !important; }}
            .body-title     {{ font-size: 18px !important; }}
            .body-text      {{ font-size: 14px !important; }}
            .cta-button     {{ padding: 13px 24px !important; font-size: 15px !important; }}
            .fallback-text  {{ font-size: 11px !important; }}
            .warning-text   {{ font-size: 12px !important; }}
        }}
    </style>
</head>
<body style='margin:0; padding:0; background:#0f172a; font-family: Arial, Helvetica, sans-serif;'>

    <table width='100%' cellpadding='0' cellspacing='0' role='presentation'
           style='background:#0f172a; min-height:100vh;'>
        <tr>
            <td align='center' class='email-wrapper' style='padding: 40px 16px;'>

                <table class='email-card' width='560' cellpadding='0' cellspacing='0' role='presentation'
                       style='background:#1e293b; border-radius:16px; overflow:hidden;
                              box-shadow: 0 25px 50px rgba(0,0,0,0.6);
                              max-width:560px; width:100%;'>

                    <!-- HEADER -->
                    <tr>
                        <td class='email-header' align='center'
                            style='background: linear-gradient(135deg,#6366f1 0%,#8b5cf6 100%);
                                   padding: 36px 40px;'>
                            <div class='logo-icon'
                                 style='font-size:44px; line-height:1; margin-bottom:10px;'>🎮</div>
                            <div class='logo-text'
                                 style='color:#ffffff; font-size:26px; font-weight:700;
                                        letter-spacing:-0.5px; margin-bottom:4px;'>StationPro</div>
                            <div style='color:rgba(255,255,255,0.75); font-size:13px;'>
                                Gaming Store Management Platform
                            </div>
                        </td>
                    </tr>

                    <!-- BODY -->
                    <tr>
                        <td class='email-body' style='padding: 36px 40px;'>

                            <div class='body-title'
                                 style='color:#f1f5f9; font-size:22px; font-weight:700;
                                        margin-bottom:12px; line-height:1.3;'>
                                🔑 Reset Your Password
                            </div>

                            <div class='body-text'
                                 style='color:#94a3b8; font-size:15px; line-height:1.7;
                                        margin-bottom:28px;'>
                                We received a request to reset the password for your StationPro account.
                                Click the button below to choose a new password.
                            </div>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0' role='presentation'>
                                <tr>
                                    <td align='center' style='padding: 4px 0 28px;'>
                                        <a href='{resetLink}' class='cta-button'
                                           style='background: linear-gradient(135deg,#6366f1,#8b5cf6);
                                                  color:#ffffff; text-decoration:none;
                                                  padding: 15px 40px; border-radius: 10px;
                                                  font-size:16px; font-weight:700;
                                                  display:inline-block; letter-spacing:0.3px;'>
                                            Reset My Password
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- Divider -->
                            <table width='100%' cellpadding='0' cellspacing='0' role='presentation'>
                                <tr>
                                    <td style='border-top: 1px solid #334155; padding-bottom: 20px;'></td>
                                </tr>
                            </table>

                            <!-- Fallback link -->
                            <div class='body-text'
                                 style='color:#64748b; font-size:13px; text-align:center;
                                        margin-bottom:24px; line-height:1.6;'>
                                Button not working? Paste this link into your browser:<br/>
                                <a href='{resetLink}' class='fallback-text'
                                   style='color:#6366f1; font-size:12px;
                                          word-break:break-all; word-wrap:break-word;'>
                                    {resetLink}
                                </a>
                            </div>

                            <!-- Warning box -->
                            <table width='100%' cellpadding='0' cellspacing='0' role='presentation'>
                                <tr>
                                    <td class='warning-text'
                                        style='background:#1e3a5f; border-left:3px solid #3b82f6;
                                               border-radius:6px; padding:14px 16px;
                                               color:#93c5fd; font-size:13px; line-height:1.6;'>
                                        ⏰ <strong>This link expires in 1 hour.</strong><br/>
                                        If you didn't request a password reset, you can safely
                                        ignore this email — your password won't change.
                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>

                    <!-- FOOTER -->
                    <tr>
                        <td class='email-footer' align='center'
                            style='background:#0f172a; padding:20px 40px;'>
                            <div style='color:#475569; font-size:12px; margin-bottom:4px;'>
                                © 2025 StationPro · All rights reserved
                            </div>
                            <div style='color:#334155; font-size:11px;'>
                                This is an automated message, please do not reply.
                            </div>
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
        }

        // ── Generic fallback ──────────────────────────────────────────────────
        public static string BuildGenericEmail(string subject, string plainBody)
        {
            var contentHtml = System.Net.WebUtility.HtmlEncode(plainBody);

            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{System.Net.WebUtility.HtmlEncode(subject)}</title>
    <style>
        * {{ margin:0; padding:0; box-sizing:border-box; }}
        body {{ margin:0 !important; padding:0 !important; background:#0f172a; }}
        table {{ border-collapse:collapse !important; }}
        @media only screen and (max-width:600px) {{
            .email-wrapper {{ padding:16px !important; }}
            .email-card    {{ border-radius:12px !important; }}
            .email-header  {{ padding:24px 20px !important; }}
            .email-body    {{ padding:24px 20px !important; }}
        }}
    </style>
</head>
<body style='margin:0; padding:0; background:#0f172a; font-family: Arial, Helvetica, sans-serif;'>
    <table width='100%' cellpadding='0' cellspacing='0' role='presentation' style='background:#0f172a;'>
        <tr>
            <td align='center' class='email-wrapper' style='padding:40px 16px;'>
                <table class='email-card' width='560' cellpadding='0' cellspacing='0' role='presentation'
                       style='background:#1e293b; border-radius:16px; overflow:hidden;
                              box-shadow:0 25px 50px rgba(0,0,0,0.6); max-width:560px; width:100%;'>
                    <tr>
                        <td class='email-header' align='center'
                            style='background:linear-gradient(135deg,#6366f1 0%,#8b5cf6 100%); padding:32px 40px;'>
                            <div style='font-size:40px; margin-bottom:8px;'>🎮</div>
                            <div style='color:#fff; font-size:24px; font-weight:700;'>StationPro</div>
                            <div style='color:rgba(255,255,255,0.75); font-size:13px; margin-top:4px;'>
                                Gaming Store Management Platform
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td class='email-body' style='padding:36px 40px;'>
                            <div style='color:#f1f5f9; font-size:20px; font-weight:700; margin-bottom:16px;'>
                                {System.Net.WebUtility.HtmlEncode(subject)}
                            </div>
                            <div style='color:#94a3b8; font-size:15px; line-height:1.7; white-space:pre-line;'>
                                {contentHtml}
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='background:#0f172a; padding:20px 40px;'>
                            <div style='color:#475569; font-size:12px;'>
                                © 2025 StationPro · All rights reserved
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}