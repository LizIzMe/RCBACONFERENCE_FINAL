using OfficeOpenXml;
using RCBACONFERENCE.Data;
using RCBACONFERENCE.Models;
using System.IO;
using System.Linq;

namespace RCBACONFERENCE.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public MemoryStream GenerateReceiptReport(string researchEventId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var researchEvent = _context.ResearchEvent
                .FirstOrDefault(e => e.ResearchEventId == researchEventId);

            var approvedReceipts = _context.Receipt
                .Where(r => r.Status == "Approved" && r.ResearchEventId == researchEventId)
                .Join(_context.UsersConference,
                      receipt => receipt.UserId,
                      user => user.UserId,
                      (receipt, user) => new
                      {
                          user.UserId,
                          Name = $"{user.FirstName} {user.LastName}",
                          user.Affiliation,
                          user.CountryRegion,
                          user.Classification,
                          Price = user.Classification == "Student" ? 6000 : 7500
                      })
                .ToList();

            var totalRevenue = approvedReceipts.Sum(r => r.Price);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Approved Receipts");

                worksheet.Cells[1, 1].Value = $"{researchEvent?.ResearchEventId ?? "Unknown"} : {researchEvent?.EventName ?? "Unknown"}";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 1].Value = "UserId";
                worksheet.Cells[3, 2].Value = "Name";
                worksheet.Cells[3, 3].Value = "Affiliation";
                worksheet.Cells[3, 4].Value = "Country/Region";
                worksheet.Cells[3, 5].Value = "Classification";
                worksheet.Cells[3, 6].Value = "Price";

                using (var range = worksheet.Cells[3, 1, 3, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                int row = 4;
                foreach (var receipt in approvedReceipts)
                {
                    worksheet.Cells[row, 1].Value = receipt.UserId;
                    worksheet.Cells[row, 2].Value = receipt.Name;
                    worksheet.Cells[row, 3].Value = receipt.Affiliation;
                    worksheet.Cells[row, 4].Value = receipt.CountryRegion;
                    worksheet.Cells[row, 5].Value = receipt.Classification;
                    worksheet.Cells[row, 6].Value = receipt.Price;
                    row++;
                }

                // Add total revenue at the bottom
                worksheet.Cells[row + 1, 5].Value = "Total Revenue:";
                worksheet.Cells[row + 1, 6].Value = totalRevenue;

                // Apply total revenue styles
                using (var range = worksheet.Cells[row + 1, 5, row + 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                // Autofit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return the file as a memory stream
                var stream = new MemoryStream(package.GetAsByteArray());
                return stream;
            }
        }

        public MemoryStream GenerateResearchPapersReport(string researchEventId)
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Retrieve the research event details
            var researchEvent = _context.ResearchEvent
                .FirstOrDefault(e => e.ResearchEventId == researchEventId);

            // Fetch all papers for the event
            var researchPapers = _context.UploadPapers
                .Where(p => p.ResearchEventId == researchEventId)
                .Select(p => new ResearchPaperDto
                {
                    UserId = p.UserId,
                    Author = p.Author,
                    CoAuthors = p.Authors,
                    Affiliation = p.Affiliation,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    Status = p.Status
                })
                .ToList();

            // Group papers by status
            var generalList = researchPapers;
            var pendingList = researchPapers.Where(p => p.Status == "Pending").ToList();
            var approvedList = researchPapers.Where(p => p.Status == "Approved").ToList();
            var rejectedList = researchPapers.Where(p => p.Status == "Rejected").ToList();

            using (var package = new ExcelPackage())
            {
                // Add sheets for each list
                AddResearchPapersSheet(package, "General List", generalList, researchEvent);
                AddResearchPapersSheet(package, "Pending List", pendingList, researchEvent);
                AddResearchPapersSheet(package, "Approved List", approvedList, researchEvent);
                AddResearchPapersSheet(package, "Rejected List", rejectedList, researchEvent);

                // Return the file as a memory stream
                var stream = new MemoryStream(package.GetAsByteArray());
                return stream;
            }
        }

        // Helper method to add a sheet
        private void AddResearchPapersSheet(ExcelPackage package, string sheetName, List<ResearchPaperDto> papers, ResearchEvent researchEvent)
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Add header with event details
            worksheet.Cells[1, 1].Value = $"{researchEvent?.ResearchEventId ?? "Unknown"} : {researchEvent?.EventName ?? "Unknown"}";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1, 1, 7].Merge = true; // Merge cells A1:G1
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            // Add table headers
            worksheet.Cells[3, 1].Value = "UserId";
            worksheet.Cells[3, 2].Value = "Author";
            worksheet.Cells[3, 3].Value = "Co-Authors";
            worksheet.Cells[3, 4].Value = "Affiliation";
            worksheet.Cells[3, 5].Value = "Title";
            worksheet.Cells[3, 6].Value = "Abstract";
            worksheet.Cells[3, 7].Value = "Status";

            // Apply styles to headers
            using (var range = worksheet.Cells[3, 1, 3, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // Add data rows
            int row = 4;
            foreach (var paper in papers)
            {
                worksheet.Cells[row, 1].Value = paper.UserId;
                worksheet.Cells[row, 2].Value = paper.Author;
                worksheet.Cells[row, 3].Value = paper.CoAuthors;
                worksheet.Cells[row, 4].Value = paper.Affiliation;
                worksheet.Cells[row, 5].Value = paper.Title;
                worksheet.Cells[row, 6].Value = paper.Abstract;
                worksheet.Cells[row, 7].Value = paper.Status;
                row++;
            }

            // Autofit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

    }
}