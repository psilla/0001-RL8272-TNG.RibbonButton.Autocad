using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using _0001_RL8272_TNG.RibbonButton.Autocad.Properties;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using TYPSA.SharedLib.Autocad.Main;


namespace TYPSA.PS.RibbonButton.Autocad
{
    internal class cls_00_ExportTgnCheckToHtml
    {
        public static string ByteArrayIconToBase64(byte[] iconBytes)
        {
            if (iconBytes == null || iconBytes.Length == 0)
                return string.Empty;

            using (MemoryStream ms = new MemoryStream(iconBytes))
            using (Icon icon = new Icon(ms))
            using (Bitmap bmp = icon.ToBitmap())
            using (MemoryStream outMs = new MemoryStream())
            {
                bmp.Save(outMs, ImageFormat.Png);
                return Convert.ToBase64String(outMs.ToArray());
            }
        }

        //public static void ExportToHtml(
        //    Dictionary<string, object> exportData,
        //    List<WarningCheckLogResult> warningChecksLog,
        //    string projectCode,
        //    int totalFiles,
        //    int processedFiles
        //)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    string logoBase64 = ByteArrayIconToBase64(Resources.typsaIcon);

        //    string logoHtml = string.IsNullOrWhiteSpace(logoBase64)
        //        ? string.Empty
        //        : $"<img class='logo' src='data:image/png;base64,{logoBase64}' alt='TYPSA Logo' />";

        //    sb.AppendLine("<!DOCTYPE html>");
        //    sb.AppendLine("<html>");
        //    sb.AppendLine("<head>");
        //    sb.AppendLine("<meta charset='UTF-8'>");
        //    sb.AppendLine("<title>TNG DWGs Drawing Checker Report</title>");

        //    sb.AppendLine("<style>");
        //    sb.AppendLine("body { font-family: Arial, sans-serif; margin: 30px; background:#f4f4f4; color:#222; }");
        //    sb.AppendLine(".container { max-width: 1200px; margin:auto; background:white; padding:35px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); }");

        //    sb.AppendLine(".header { display:flex; align-items:center; gap:25px; margin-bottom:30px; }");
        //    sb.AppendLine(".logo { height:70px; width:auto; }");
        //    sb.AppendLine(".header-title { color:#B5121B; font-size:46px; margin:0; }");

        //    sb.AppendLine("h1 { color:#B5121B; font-size:48px; margin-bottom:25px; }");
        //    sb.AppendLine("h2 { margin-top:40px; color:#333; border-bottom:2px solid #e0e0e0; padding-bottom:8px; font-size:28px; }");

        //    sb.AppendLine(".summary { display:flex; gap:20px; margin:25px 0; }");
        //    sb.AppendLine(".card { flex:1; background:#fafafa; padding:18px; border-radius:8px; border-left:5px solid #B5121B; box-shadow:0 1px 4px rgba(0,0,0,0.05); }");
        //    sb.AppendLine(".card-title { font-size:14px; color:#777; margin-bottom:8px; }");
        //    sb.AppendLine(".card-value { font-size:32px; font-weight:bold; color:#222; }");

        //    sb.AppendLine("table { width:100%; border-collapse:collapse; margin-top:15px; font-size:14px; }");
        //    sb.AppendLine("th { background:#B5121B; color:white; text-align:left; padding:12px; font-size:15px; }");
        //    sb.AppendLine("td { border:1px solid #e3e3e3; padding:10px; }");
        //    sb.AppendLine("tr:nth-child(even) { background:#fafafa; }");
        //    sb.AppendLine("tr:hover { background:#f8eaea; }");

        //    sb.AppendLine(".ok { color:#B5121B; font-weight:bold; }");
        //    sb.AppendLine(".warn { color:#d17b00; font-weight:bold; }");
        //    sb.AppendLine("</style>");

        //    sb.AppendLine("</head>");
        //    sb.AppendLine("<body>");
        //    sb.AppendLine("<div class='container'>");

        //    sb.AppendLine("<div class='header'>");
        //    sb.AppendLine(logoHtml);
        //    sb.AppendLine("<h1 class='header-title'>TNG DWGs Drawing Checker Report</h1>");
        //    sb.AppendLine("</div>");

        //    sb.AppendLine("<div class='summary'>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Project Code</div><div class='card-value'>{System.Net.WebUtility.HtmlEncode(projectCode)}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Execution Date</div><div class='card-value'>{DateTime.Now:yyyy-MM-dd HH:mm}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Files Processed</div><div class='card-value'>{processedFiles}/{totalFiles}</div></div>");
        //    sb.AppendLine("</div>");

        //    sb.AppendLine("<h2>Generated Worksheets</h2>");
        //    sb.AppendLine("<table>");
        //    sb.AppendLine("<tr><th>Worksheet</th><th>Status</th></tr>");

        //    foreach (var kvp in exportData)
        //    {
        //        sb.AppendLine("<tr>");
        //        sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(kvp.Key)}</td>");
        //        sb.AppendLine("<td class='ok'>Generated</td>");
        //        sb.AppendLine("</tr>");
        //    }

        //    sb.AppendLine("</table>");

        //    sb.AppendLine("<h2>Warning Selected Checks Log</h2>");

        //    if (warningChecksLog != null && warningChecksLog.Count > 0)
        //    {
        //        sb.AppendLine("<table>");
        //        sb.AppendLine("<tr><th>File Name</th><th>Check Name</th><th>Message</th></tr>");

        //        foreach (var item in warningChecksLog)
        //        {
        //            sb.AppendLine("<tr>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.FileName)}</td>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.CheckName)}</td>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.Message)}</td>");
        //            sb.AppendLine("</tr>");
        //        }

        //        sb.AppendLine("</table>");
        //    }
        //    else
        //    {
        //        sb.AppendLine("<p class='ok'>All selected checks returned applicable data.</p>");
        //    }

        //    sb.AppendLine("</div>");
        //    sb.AppendLine("</body>");
        //    sb.AppendLine("</html>");

        //    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //    string htmlPath = Path.Combine(
        //        desktop, $"TNG_DWGsDrawingChecker_Report_{projectCode}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
        //    );

        //    File.WriteAllText(htmlPath, sb.ToString(), Encoding.UTF8);

        //    System.Diagnostics.Process.Start(htmlPath);
        //}

        //public static void ExportToHtml(
        //    Dictionary<string, object> exportData,
        //    List<WarningCheckLogResult> warningChecksLog,
        //    string projectCode,
        //    int totalFiles,
        //    int processedFiles
        //)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    // -----------------------------
        //    // Execution User
        //    // -----------------------------

        //    string executionUser;

        //    try
        //    {
        //        executionUser = Autodesk.AutoCAD.ApplicationServices.Application
        //            .GetSystemVariable("LOGINNAME")?.ToString();

        //        if (string.IsNullOrWhiteSpace(executionUser))
        //            executionUser = Environment.UserName;
        //    }
        //    catch
        //    {
        //        executionUser = Environment.UserName;
        //    }

        //    string logoBase64 = ByteArrayIconToBase64(Resources.typsaIcon);

        //    string logoHtml = string.IsNullOrWhiteSpace(logoBase64)
        //        ? string.Empty
        //        : $"<img class='logo' src='data:image/png;base64,{logoBase64}' alt='TYPSA Logo' />";

        //    // -----------------------------
        //    // Coverage Rate Calculation
        //    // -----------------------------

        //    int missingCount = warningChecksLog?.Count ?? 0;

        //    int realChecksCount = exportData.ContainsKey("Warning Selected Checks Log")
        //        ? exportData.Count - 1
        //        : exportData.Count;

        //    int totalPossibleChecks = totalFiles * realChecksCount;

        //    double coverageRate = totalPossibleChecks > 0
        //        ? ((double)(totalPossibleChecks - missingCount) / totalPossibleChecks) * 100
        //        : 100;

        //    int coverageRateRounded = (int)Math.Round(coverageRate);

        //    string scoreClass =
        //        coverageRateRounded >= 85 ? "score-good" :
        //        coverageRateRounded >= 60 ? "score-medium" :
        //        "score-bad";

        //    int successfulChecks = Math.Max(0, totalPossibleChecks - missingCount);

        //    sb.AppendLine("<!DOCTYPE html>");
        //    sb.AppendLine("<html>");
        //    sb.AppendLine("<head>");
        //    sb.AppendLine("<meta charset='UTF-8'>");
        //    sb.AppendLine("<title>TNG DWGs Drawing Checker Report</title>");

        //    sb.AppendLine("<style>");
        //    sb.AppendLine("body { font-family: Arial, sans-serif; margin: 30px; background:#f4f4f4; color:#222; }");
        //    sb.AppendLine(".container { max-width: 1350px; margin:auto; background:white; padding:35px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); }");

        //    sb.AppendLine(".header { display:flex; align-items:center; gap:25px; margin-bottom:30px; }");
        //    sb.AppendLine(".logo { height:70px; width:auto; }");
        //    sb.AppendLine(".header-title { color:#B5121B; font-size:46px; margin:0; }");

        //    sb.AppendLine("h2 { margin-top:45px; color:#333; border-bottom:2px solid #e0e0e0; padding-bottom:8px; font-size:28px; }");

        //    sb.AppendLine(".summary { display:flex; gap:20px; margin:25px 0; flex-wrap:wrap; }");
        //    sb.AppendLine(".card { flex:1; min-width:220px; background:#fafafa; padding:18px; border-radius:8px; border-left:5px solid #B5121B; box-shadow:0 1px 4px rgba(0,0,0,0.05); }");
        //    sb.AppendLine(".card-title { font-size:14px; color:#777; margin-bottom:8px; }");
        //    sb.AppendLine(".card-value { font-size:32px; font-weight:bold; color:#222; }");

        //    sb.AppendLine("table { width:100%; border-collapse:collapse; margin-top:15px; font-size:14px; }");
        //    sb.AppendLine("th { background:#B5121B; color:white; text-align:left; padding:12px; font-size:15px; }");
        //    sb.AppendLine("td { border:1px solid #e3e3e3; padding:10px; }");
        //    sb.AppendLine("tr:nth-child(even) { background:#fafafa; }");
        //    sb.AppendLine("tr:hover { background:#f8eaea; }");

        //    sb.AppendLine(".ok { color:#B5121B; font-weight:bold; }");
        //    sb.AppendLine(".score-good { color:#2e7d32; font-weight:bold; }");
        //    sb.AppendLine(".score-medium { color:#ef6c00; font-weight:bold; }");
        //    sb.AppendLine(".score-bad { color:#c62828; font-weight:bold; }");

        //    sb.AppendLine("details { margin-top:20px; }");
        //    sb.AppendLine("summary { cursor:pointer; font-size:20px; font-weight:bold; color:#B5121B; margin-bottom:10px; }");

        //    sb.AppendLine(".footer { margin-top:60px; padding-top:20px; border-top:1px solid #ddd; text-align:center; font-size:12px; color:#777; }");

        //    sb.AppendLine("</style>");
        //    sb.AppendLine("</head>");
        //    sb.AppendLine("<body>");
        //    sb.AppendLine("<div class='container'>");

        //    // Header
        //    sb.AppendLine("<div class='header'>");
        //    sb.AppendLine(logoHtml);
        //    sb.AppendLine("<h1 class='header-title'>TNG DWGs Drawing Checker Report</h1>");
        //    sb.AppendLine("</div>");

        //    // Main cards
        //    sb.AppendLine("<div class='summary'>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Project Code</div><div class='card-value'>{System.Net.WebUtility.HtmlEncode(projectCode)}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Execution Date</div><div class='card-value'>{DateTime.Now:yyyy-MM-dd HH:mm}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Files Processed</div><div class='card-value'>{processedFiles}/{totalFiles}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Executed By</div><div class='card-value'>{System.Net.WebUtility.HtmlEncode(executionUser)}</div></div>");
        //    sb.AppendLine("</div>");

        //    // Quick summary
        //    sb.AppendLine("<h2>Execution Summary</h2>");
        //    sb.AppendLine("<div class='summary'>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Worksheets Generated</div><div class='card-value'>{realChecksCount}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Warning Checks Detected</div><div class='card-value'>{missingCount}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Successful Checks</div><div class='card-value'>{successfulChecks}</div></div>");
        //    sb.AppendLine($"<div class='card'><div class='card-title'>Coverage Rate</div><div class='card-value {scoreClass}'>{coverageRateRounded}%</div></div>");
        //    sb.AppendLine("</div>");

        //    // Worksheets
        //    sb.AppendLine("<h2>Generated Worksheets</h2>");
        //    sb.AppendLine("<table>");
        //    sb.AppendLine("<tr><th>Worksheet</th><th>Status</th></tr>");

        //    foreach (var kvp in exportData)
        //    {
        //        sb.AppendLine("<tr>");
        //        sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(kvp.Key)}</td>");
        //        sb.AppendLine("<td class='ok'>Generated</td>");
        //        sb.AppendLine("</tr>");
        //    }

        //    sb.AppendLine("</table>");

        //    // Warning checks collapsable
        //    sb.AppendLine("<h2>Validation Log</h2>");

        //    if (warningChecksLog != null && warningChecksLog.Count > 0)
        //    {
        //        sb.AppendLine("<details>");
        //        sb.AppendLine("<summary>Show Warning Selected Checks Log</summary>");

        //        sb.AppendLine("<table>");
        //        sb.AppendLine("<tr><th>File Name</th><th>Check Name</th><th>Message</th></tr>");

        //        foreach (var item in warningChecksLog)
        //        {
        //            sb.AppendLine("<tr>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.FileName)}</td>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.CheckName)}</td>");
        //            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.Message)}</td>");
        //            sb.AppendLine("</tr>");
        //        }

        //        sb.AppendLine("</table>");
        //        sb.AppendLine("</details>");
        //    }
        //    else
        //    {
        //        sb.AppendLine("<p class='score-good'>All selected checks returned applicable data.</p>");
        //    }

        //    // Footer
        //    sb.AppendLine("<div class='footer'>");
        //    sb.AppendLine("Generated by TYPSA DWG Drawing Checker<br>");
        //    sb.AppendLine($"Report generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}<br>");
        //    sb.AppendLine("© TYPSA Group");
        //    sb.AppendLine("</div>");

        //    sb.AppendLine("</div>");
        //    sb.AppendLine("</body>");
        //    sb.AppendLine("</html>");

        //    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //    string htmlPath = Path.Combine(
        //        desktop,
        //        $"TNG_DWGsDrawingChecker_Report_{projectCode}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
        //    );

        //    File.WriteAllText(htmlPath, sb.ToString(), Encoding.UTF8);
        //    System.Diagnostics.Process.Start(htmlPath);
        //}

        public static void ExportToHtml(
            Dictionary<string, object> exportData,
            List<WarningCheckLogResult> warningChecksLog,
            string projectCode,
            int totalFiles,
            int processedFiles
        )
        {
            StringBuilder sb = new StringBuilder();

            // -----------------------------
            // Execution User
            // -----------------------------

            string executionUser;

            try
            {
                executionUser = Autodesk.AutoCAD.ApplicationServices.Application
                    .GetSystemVariable("LOGINNAME")?.ToString();

                if (string.IsNullOrWhiteSpace(executionUser))
                    executionUser = Environment.UserName;
            }
            catch
            {
                executionUser = Environment.UserName;
            }

            string logoBase64 = ByteArrayIconToBase64(Resources.typsaIcon);

            string logoHtml = string.IsNullOrWhiteSpace(logoBase64)
                ? string.Empty
                : $"<img class='logo' src='data:image/png;base64,{logoBase64}' alt='TYPSA Logo' />";

            // -----------------------------
            // Coverage Rate Calculation
            // -----------------------------

            int missingCount = warningChecksLog?.Count ?? 0;

            int realChecksCount = exportData.ContainsKey("Warning Selected Checks Log")
                ? exportData.Count - 1
                : exportData.Count;

            int totalPossibleChecks = totalFiles * realChecksCount;

            double coverageRate = totalPossibleChecks > 0
                ? ((double)(totalPossibleChecks - missingCount) / totalPossibleChecks) * 100
                : 100;

            int coverageRateRounded = (int)Math.Round(coverageRate);

            string scoreClass =
                coverageRateRounded >= 85 ? "score-good" :
                coverageRateRounded >= 60 ? "score-medium" :
                "score-bad";

            int successfulChecks = Math.Max(0, totalPossibleChecks - missingCount);

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>ATENEA DWGs Model Checker Report</title>");

            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 30px; background:#f4f4f4; color:#222; }");
            sb.AppendLine(".container { max-width: 1350px; margin:auto; background:white; padding:35px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,0.08); }");

            sb.AppendLine(".header { display:flex; align-items:center; gap:25px; margin-bottom:30px; }");
            sb.AppendLine(".logo { height:70px; width:auto; }");
            sb.AppendLine(".header-title { color:#B5121B; font-size:46px; margin:0; }");

            sb.AppendLine("h2 { margin-top:45px; color:#333; border-bottom:2px solid #e0e0e0; padding-bottom:8px; font-size:28px; }");

            sb.AppendLine(".summary { display:flex; gap:20px; margin:25px 0; flex-wrap:wrap; }");
            sb.AppendLine(".card { flex:1; min-width:220px; background:#fafafa; padding:18px; border-radius:8px; border-left:5px solid #B5121B; box-shadow:0 1px 4px rgba(0,0,0,0.05); }");
            sb.AppendLine(".card-title { font-size:14px; color:#777; margin-bottom:8px; }");
            sb.AppendLine(".card-value { font-size:32px; font-weight:bold; color:#222; }");

            sb.AppendLine("table { width:100%; border-collapse:collapse; margin-top:15px; font-size:14px; }");
            sb.AppendLine("th { background:#B5121B; color:white; text-align:left; padding:12px; font-size:15px; }");
            sb.AppendLine("td { border:1px solid #e3e3e3; padding:10px; }");
            sb.AppendLine("tr:nth-child(even) { background:#fafafa; }");
            sb.AppendLine("tr:hover { background:#f8eaea; }");

            sb.AppendLine(".ok { color:#B5121B; font-weight:bold; }");
            sb.AppendLine(".score-good { color:#2e7d32; font-weight:bold; }");
            sb.AppendLine(".score-medium { color:#ef6c00; font-weight:bold; }");
            sb.AppendLine(".score-bad { color:#c62828; font-weight:bold; }");

            sb.AppendLine("details { margin-top:20px; }");
            sb.AppendLine("summary { cursor:pointer; font-size:20px; font-weight:bold; color:#B5121B; margin-bottom:10px; }");

            sb.AppendLine(".footer { margin-top:60px; padding-top:20px; border-top:1px solid #ddd; text-align:center; font-size:12px; color:#777; }");

            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine(logoHtml);
            sb.AppendLine("<h1 class='header-title'>ATENEA DWGs Model Checker Report</h1>");
            sb.AppendLine("</div>");

            // Main cards
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Project Code</div><div class='card-value'>{System.Net.WebUtility.HtmlEncode(projectCode)}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Execution Date</div><div class='card-value'>{DateTime.Now:yyyy-MM-dd HH:mm}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Files Processed</div><div class='card-value'>{processedFiles}/{totalFiles}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Executed By</div><div class='card-value'>{System.Net.WebUtility.HtmlEncode(executionUser)}</div></div>");
            sb.AppendLine("</div>");

            // Quick summary
            sb.AppendLine("<h2>Execution Summary</h2>");
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Worksheets Generated</div><div class='card-value'>{realChecksCount}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Warning Checks Detected</div><div class='card-value'>{missingCount}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Successful Checks</div><div class='card-value'>{successfulChecks}</div></div>");
            sb.AppendLine($"<div class='card'><div class='card-title'>Coverage Rate</div><div class='card-value {scoreClass}'>{coverageRateRounded}%</div></div>");
            sb.AppendLine("</div>");

            // Worksheets
            sb.AppendLine("<h2>Generated Worksheets</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Worksheet</th><th>Status</th></tr>");

            foreach (var kvp in exportData)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(kvp.Key)}</td>");
                sb.AppendLine("<td class='ok'>Generated</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            // Warning checks collapsable
            sb.AppendLine("<h2>Validation Log</h2>");

            if (warningChecksLog != null && warningChecksLog.Count > 0)
            {
                sb.AppendLine("<details>");
                sb.AppendLine("<summary>Show Warning Selected Checks Log</summary>");

                sb.AppendLine("<table>");
                sb.AppendLine("<tr><th>File Name</th><th>Check Name</th><th>Message</th></tr>");

                foreach (var item in warningChecksLog)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.FileName)}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.CheckName)}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(item.Message)}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</table>");
                sb.AppendLine("</details>");
            }
            else
            {
                sb.AppendLine("<p class='score-good'>All selected checks returned applicable data.</p>");
            }

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("Generated by TYPSA DWG Model Checker<br>");
            sb.AppendLine($"Report generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}<br>");
            sb.AppendLine("© TYPSA Group");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            // Ruta del escritorio
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Nombre seguro del proyecto
            string safeProjectCode = string.Concat(projectCode.Select(c =>
                Path.GetInvalidFileNameChars().Contains(c) ? '_' : c
            ));

            // Ruta del HTML
            string htmlPath = Path.Combine(
                desktop, $"TNG_DWGsDrawingChecker_Report_{safeProjectCode}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            );

            // Guardamos el archivo
            File.WriteAllText(htmlPath, sb.ToString(), new UTF8Encoding(true));

            // Abrimos el HTML con el navegador predeterminado
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = htmlPath,
                    UseShellExecute = true
                }
            );
        }


    }
}
