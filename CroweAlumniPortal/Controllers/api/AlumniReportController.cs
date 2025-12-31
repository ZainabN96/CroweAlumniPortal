using CroweAlumniPortal.Data;
using CroweAlumniPortal.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Excel
using ClosedXML.Excel;

// PDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlumniReportController : ControllerBase
    {
        private readonly ApplicationDbContext _dc;

        public AlumniReportController(ApplicationDbContext dc)
        {
            _dc = dc;
        }

        // ✅ List for report table
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] AlumniReportFilterDto f)
        {
            var q = _dc.Users.AsNoTracking().Where(x => x.UserType == "Alumni" && x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var s = f.Search.Trim().ToLower();
                q = q.Where(u =>
                    ((u.FirstName ?? "").ToLower().Contains(s)) ||
                    ((u.LastName ?? "").ToLower().Contains(s)) ||
                    ((u.EmailAddress ?? "").ToLower().Contains(s)) ||
                    ((u.City ?? "").ToLower().Contains(s)) ||
                    ((u.Country ?? "").ToLower().Contains(s)) ||
                    ((u.Industry ?? "").ToLower().Contains(s)) ||
                    ((u.EmployerOrganization ?? "").ToLower().Contains(s)) ||
                    ((u.JobTitle ?? "").ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(f.Country) && f.Country != "all")
                q = q.Where(u => u.Country == f.Country);

            if (!string.IsNullOrWhiteSpace(f.City) && f.City != "all")
                q = q.Where(u => u.City == f.City);

            if (!string.IsNullOrWhiteSpace(f.Industry) && f.Industry != "all")
                q = q.Where(u => u.Industry == f.Industry);

            if (f.JoinYear.HasValue)
                q = q.Where(u => u.JoiningDate.Year == f.JoinYear.Value);
           
            if (f.LeaveYear.HasValue)
                q = q.Where(u => u.LeavingDate.Year == f.LeaveYear.Value);

            var list = await q
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .Select(u => new AlumniReportRowDto
                {
                    Id = u.Id,
                    FullName = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                    Email = u.EmailAddress ?? "",
                    City = u.City ?? "",
                    Country = u.Country ?? "",
                    Industry = u.Industry ?? "",
                    Organization = u.EmployerOrganization ?? "",
                    JobTitle = u.JobTitle ?? "",
                    JoiningDate = u.JoiningDate,
                    LeavingDate = u.LeavingDate,
                    MemberStatus = u.MemberStatus ?? ""
                })
                .ToListAsync();

            // ✅ Add Sr here
            for (int i = 0; i < list.Count; i++) list[i].Sr = i + 1;

            return Ok(list);
        }

        // ✅ View icon 
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var u = await _dc.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.UserType == "Alumni");
            if (u == null) return NotFound();

            return Ok(new
            {
                u.Id,
                u.Title,
                u.FirstName,
                u.LastName,
                u.EmailAddress,
                u.LoginId,
                u.MemberStatus,
                u.Qualification,
                u.DOB,
                u.CNIC,
                u.MobileNumber,
                u.Address,
                u.City,
                u.Country,
                u.LinkedIn,
                u.ProfilePicturePath,

                // ✅ IMPORTANT
                u.EmploymentStatus,

                // ✅ Current Employer
                u.Industry,
                u.EmployerOrganization,
                u.JobTitle,
                u.EmployerCountry,
                u.EmployerCity,
                u.EmployerLandline1,
                u.EmployerFaxNumber,
                u.EmployerAddress,

                // ✅ Crowe History
                u.StaffCode,
                u.LastPosition,
                u.Department,
                u.HistoryCity,
                u.JoiningDate,
                u.LeavingDate
            });
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] AlumniReportFilterDto f)
        {
            // reuse List query logic:
            var data = await GetFiltered(f);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Crowe Alumni Report");

            // headers
            var headers = new[]
            {
                "Sr", "Full Name", "Email", "Phone Number", "City", "Country", "Industry", "Organization", "Job Title", "Joining Date", "Leaving Date", "Member"
            };

            for (int c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];

            // rows
            for (int r = 0; r < data.Count; r++)
            {
                var x = data[r];
                ws.Cell(r + 2, 1).Value = x.Sr;
                ws.Cell(r + 2, 2).Value = x.FullName;
                ws.Cell(r + 2, 3).Value = x.Email;
                ws.Cell(r + 2, 3).Value = x.MobileNumber;
                ws.Cell(r + 2, 4).Value = x.City;
                ws.Cell(r + 2, 5).Value = x.Country;
                ws.Cell(r + 2, 6).Value = x.Industry;
                ws.Cell(r + 2, 7).Value = x.Organization;
                ws.Cell(r + 2, 8).Value = x.JobTitle;
                ws.Cell(r + 2, 9).Value = x.JoiningDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(r + 2, 10).Value = x.LeavingDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(r + 2, 11).Value = x.MemberStatus;
            }

            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            var bytes = ms.ToArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"CroweAlumniReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        // ✅ PDF export
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] AlumniReportFilterDto f)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetFiltered(f);

            var doc = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(r =>
                    {
                        r.RelativeItem().Text("Crowe Alumni Report").FontSize(18).SemiBold();
                        r.ConstantItem(220).AlignRight().Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });

                    page.Content().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30);   // Sr
                            c.RelativeColumn(2);    // Name
                            c.RelativeColumn(2);    // Email
                            c.RelativeColumn(2);    // MobileNumber
                            c.RelativeColumn(1);    // City
                            c.RelativeColumn(1);    // Country
                            c.RelativeColumn(1);    // Industry
                            c.RelativeColumn(2);    // Org
                            c.RelativeColumn(1);    // Job
                            c.RelativeColumn(2);    // MemberStatus
                        });

                        t.Header(h =>
                        {
                            void H(string s) => h.Cell().Padding(4).Background(Colors.Grey.Lighten3).Text(s).SemiBold();
                            H("Sr"); H("Name"); H("Email"); H("MobileNumber"); H("City"); H("Country"); H("Industry"); H("Organization"); H("Job"); H("Member");
                        });

                        foreach (var x in data)
                        {
                            void C(string s) => t.Cell().Padding(4).Text(s ?? "");
                            C(x.Sr.ToString());
                            C(x.FullName);
                            C(x.Email);
                            C(x.MobileNumber);
                            C(x.City);
                            C(x.Country);
                            C(x.Industry);
                            C(x.Organization);
                            C(x.JobTitle);
                            C(x.MemberStatus);
                        }
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generated by Crowe Alumni Portal • ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });

            var bytes = doc.GeneratePdf();

            return File(bytes, "application/pdf", $"CroweAlumniReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        // -------- helper (same filters) --------
        private async Task<List<AlumniReportRowDto>> GetFiltered(AlumniReportFilterDto f)
        {
            var q = _dc.Users.AsNoTracking().Where(x => x.UserType == "Alumni" && x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var s = f.Search.Trim().ToLower();
                q = q.Where(u =>
                    ((u.FirstName ?? "").ToLower().Contains(s)) ||
                    ((u.LastName ?? "").ToLower().Contains(s)) ||
                    ((u.EmailAddress ?? "").ToLower().Contains(s)) ||
                    ((u.City ?? "").ToLower().Contains(s)) ||
                    ((u.Country ?? "").ToLower().Contains(s)) ||
                    ((u.Industry ?? "").ToLower().Contains(s)) ||
                    ((u.EmployerOrganization ?? "").ToLower().Contains(s)) ||
                    ((u.JobTitle ?? "").ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(f.Country) && f.Country != "all")
                q = q.Where(u => u.Country == f.Country);

            if (!string.IsNullOrWhiteSpace(f.City) && f.City != "all")
                q = q.Where(u => u.City == f.City);

            if (!string.IsNullOrWhiteSpace(f.Industry) && f.Industry != "all")
                q = q.Where(u => u.Industry == f.Industry);

            if (f.JoinYear.HasValue)
                q = q.Where(u => u.JoiningDate.Year == f.JoinYear.Value);

            if (f.LeaveYear.HasValue)
                q = q.Where(u => u.LeavingDate.Year == f.LeaveYear.Value);

            //if (f.JoinYear.HasValue)
            //    q = q.Where(u => u.JoiningDate.HasValue && u.JoiningDate.Value.Year == f.JoinYear.Value);

            //if (f.LeaveYear.HasValue)
            //    q = q.Where(u => u.LeavingDate.HasValue && u.LeavingDate.Value.Year == f.LeaveYear.Value);

            var list = await q
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .Select(u => new AlumniReportRowDto
                {
                    Id = u.Id,
                    FullName = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                    Email = u.EmailAddress ?? "",
                    MobileNumber = u.MobileNumber ?? "",
                    City = u.City ?? "",
                    Country = u.Country ?? "",
                    Industry = u.Industry ?? "",
                    Organization = u.EmployerOrganization ?? "",
                    JobTitle = u.JobTitle ?? "",
                    JoiningDate = u.JoiningDate,
                    LeavingDate = u.LeavingDate,
                    MemberStatus = u.MemberStatus ?? ""
                })
                /*.Select(u => new AlumniReportRowDto
                {
                    Id = u.Id,
                    FullName = string.Join(" ", new[] { u.FirstName, u.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                    Email = u.EmailAddress ?? "",
                    City = u.City ?? "",
                    Country = u.Country ?? "",
                    Industry = u.Industry ?? "",
                    Organization = u.EmployerOrganization ?? "",
                    JobTitle = u.JobTitle ?? "",
                    JoiningDate = u.JoiningDate,
                    LeavingDate = u.LeavingDate,
                    MemberStatus = u.MemberStatus ?? ""
                })*/
                .ToListAsync();

            for (int i = 0; i < list.Count; i++) list[i].Sr = i + 1;
            return list;
        }
    }
}

/*using System.Reflection.Metadata;
using ClosedXML.Excel;
using CroweAlumniPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlumniReportController : ControllerBase
    {
        private readonly ApplicationDbContext _dc;

        public AlumniReportController(ApplicationDbContext dc)
        {
            _dc = dc;
        }

        // ====== LIST (filters + paging) ======
        // GET: /api/AlumniReport?search=&city=&country=&qualification=&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? search,
            [FromQuery] string? city,
            [FromQuery] string? country,
            [FromQuery] string? qualification,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 5) pageSize = 5;

            var q = BuildQuery(search, city, country, qualification);

            var total = await q.CountAsync();

            var items = await q
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.EmailAddress,
                    x.LoginId,
                    x.Qualification,
                    x.City,
                    x.Country,
                    EmployerOrganization = x.EmployerOrganization,
                    JobTitle = x.JobTitle,
                    x.ProfilePicturePath
                })
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }

        // ====== EXPORT EXCEL ======
        // GET: /api/AlumniReport/export/excel?search=&city=&country=&qualification=
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] string? search,
            [FromQuery] string? city,
            [FromQuery] string? country,
            [FromQuery] string? qualification)
        {
            var list = await BuildQuery(search, city, country, qualification)
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Alumni");

            // Header
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Login ID";
            ws.Cell(1, 5).Value = "Qualification";
            ws.Cell(1, 6).Value = "City";
            ws.Cell(1, 7).Value = "Country";
            ws.Cell(1, 8).Value = "Organization";
            ws.Cell(1, 9).Value = "Job Title";

            ws.Range(1, 1, 1, 9).Style.Font.Bold = true;

            int r = 2;
            foreach (var x in list)
            {
                ws.Cell(r, 1).Value = x.Id;
                ws.Cell(r, 2).Value = $"{x.FirstName} {x.LastName}".Trim();
                ws.Cell(r, 3).Value = x.EmailAddress;
                ws.Cell(r, 4).Value = x.LoginId;
                ws.Cell(r, 5).Value = x.Qualification;
                ws.Cell(r, 6).Value = x.City;
                ws.Cell(r, 7).Value = x.Country;
                ws.Cell(r, 8).Value = x.EmployerOrganization;
                ws.Cell(r, 9).Value = x.JobTitle;
                r++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var fileName = $"AlumniReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // ====== EXPORT PDF ======
        // GET: /api/AlumniReport/export/pdf?search=&city=&country=&qualification=
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf(
            [FromQuery] string? search,
            [FromQuery] string? city,
            [FromQuery] string? country,
            [FromQuery] string? qualification)
        {
            var list = await BuildQuery(search, city, country, qualification)
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .Take(2000) // safety
                .ToListAsync();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Alumni Report").FontSize(18).SemiBold();

                    page.Content().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(40);   // ID
                            c.RelativeColumn(2);    // Name
                            c.RelativeColumn(2);    // Email
                            c.RelativeColumn(1);    // Login
                            c.RelativeColumn(1);    // Qual
                            c.RelativeColumn(1);    // City
                            c.RelativeColumn(1);    // Country
                            c.RelativeColumn(1);    // Org
                            c.RelativeColumn(1);    // Job
                        });

                        // header row
                        t.Header(h =>
                        {
                            void H(string txt) => h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(txt).SemiBold();
                            H("ID"); H("Name"); H("Email"); H("Login ID"); H("Qualification");
                            H("City"); H("Country"); H("Organization"); H("Job Title");
                        });

                        foreach (var x in list)
                        {
                            t.Cell().Padding(4).Text(x.Id.ToString());
                            t.Cell().Padding(4).Text($"{x.FirstName} {x.LastName}".Trim());
                            t.Cell().Padding(4).Text(x.EmailAddress ?? "");
                            t.Cell().Padding(4).Text(x.LoginId ?? "");
                            t.Cell().Padding(4).Text(x.Qualification ?? "");
                            t.Cell().Padding(4).Text(x.City ?? "");
                            t.Cell().Padding(4).Text(x.Country ?? "");
                            t.Cell().Padding(4).Text(x.EmployerOrganization ?? "");
                            t.Cell().Padding(4).Text(x.JobTitle ?? "");
                        }
                    });

                    page.Footer().AlignRight().Text($"Generated: {DateTime.Now:dd-MMM-yyyy hh:mm tt}");
                });
            }).GeneratePdf();

            var fileName = $"AlumniReport_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ====== Query builder ======
        private IQueryable<Models.User> BuildQuery(string? search, string? city, string? country, string? qualification)
        {
            var q = _dc.Users.AsNoTracking()
                .Where(x => x.UserType == "Alumni" && x.IsActive == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(x =>
                    ((x.FirstName ?? "").ToLower().Contains(s)) ||
                    ((x.LastName ?? "").ToLower().Contains(s)) ||
                    ((x.EmailAddress ?? "").ToLower().Contains(s)) ||
                    ((x.LoginId ?? "").ToLower().Contains(s)) ||
                    ((x.City ?? "").ToLower().Contains(s)) ||
                    ((x.Country ?? "").ToLower().Contains(s)) ||
                    ((x.EmployerOrganization ?? "").ToLower().Contains(s))
                );
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = city.Trim().ToLower();
                q = q.Where(x => (x.City ?? "").ToLower() == c);
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                var c = country.Trim().ToLower();
                q = q.Where(x => (x.Country ?? "").ToLower() == c);
            }

            if (!string.IsNullOrWhiteSpace(qualification))
            {
                var ql = qualification.Trim().ToLower();
                q = q.Where(x => (x.Qualification ?? "").ToLower().Contains(ql));
            }

            return q;
        }
    }
}
*/