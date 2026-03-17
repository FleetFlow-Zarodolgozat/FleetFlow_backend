using System.Net;

namespace backend.Services
{
    public static class EmailTemplates
    {
        public const string DefaultCompanyName = "FleetFlow";
        public const string DefaultContactEmail = "fleetflow.info@gmail.com";

        public static string BuildHtml(string subject, string bodyHtml, string? logoContentId = null, string? companyName = null, string? contactEmail = null)
        {
            companyName ??= DefaultCompanyName;
            contactEmail ??= DefaultContactEmail;
            var safeSubject = WebUtility.HtmlEncode(subject);
            var headerLogo = string.IsNullOrWhiteSpace(logoContentId)
                ? ""
                : $"<img src=\"cid:{WebUtility.HtmlEncode(logoContentId)}\" alt=\"{WebUtility.HtmlEncode(companyName)}\" style=\"height:44px; width:auto; display:block;\" />";
            return "<!doctype html>" +
                   "<html lang=\"en\">" +
                   "<head>" +
                   "<meta charset=\"utf-8\" />" +
                   "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />" +
                   $"<title>{safeSubject}</title>" +
                   "</head>" +
                   "<body style=\"margin:0; padding:0; background:#f6f8fb; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; color:#111827;\">" +
                   "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f6f8fb; padding:24px 0;\">" +
                   "<tr><td align=\"center\">" +
                   "<table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:640px; max-width:92vw;\">" +
                   "<tr><td style=\"padding:0 16px 12px 16px;\">" +
                   "<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\">" +
                   $"<tr><td style=\"vertical-align:middle;\">{headerLogo}</td>" +
                   $"<td style=\"vertical-align:middle; text-align:right; color:#6b7280; font-size:12px;\">{WebUtility.HtmlEncode(companyName)}</td></tr>" +
                   "</table></td></tr>" +
                   "<tr><td style=\"background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; padding:22px 22px 14px 22px;\">" +
                   $"<div style=\"font-size:18px; font-weight:700; margin:0 0 12px 0;\">{safeSubject}</div>" +
                   $"<div style=\"font-size:14px; line-height:1.6;\">{bodyHtml}</div>" +
                   "</td></tr>" +
                   "<tr><td style=\"padding:14px 16px 0 16px;\">" +
                   $"<div style=\"color:#6b7280; font-size:12px; line-height:1.55;\">This is an automated email from {WebUtility.HtmlEncode(companyName)}. Please do not reply to this message.<br />" +
                   $"Contact: <a href=\"mailto:{WebUtility.HtmlEncode(contactEmail)}\" style=\"color:#2563eb; text-decoration:none;\">{WebUtility.HtmlEncode(contactEmail)}</a></div>" +
                   $"<div style=\"color:#9ca3af; font-size:11px; margin-top:10px;\">&copy; {DateTime.UtcNow.Year} {WebUtility.HtmlEncode(companyName)}. All rights reserved.</div>" +
                   "</td></tr>" +
                   "</table></td></tr></table></body></html>";
        }

        public static string Paragraphs(params string[] lines)
        {
            return string.Join("", lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => $"<p style=\"margin:0 0 12px 0;\">{WebUtility.HtmlEncode(l).Replace("\n", "<br />")}</p>"));
        }
    }
}
