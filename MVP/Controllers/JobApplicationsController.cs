using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.JobApplications;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobApplicationResponse>>> GetAll([FromQuery] string? status)
    {
        var userId = User.GetUserId();
        var query = _db.JobApplications
            .Include(j => j.Company)
            .Where(j => j.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(j => j.Status == status);
        }

        var applications = await query
            .OrderByDescending(j => j.ApplicationDate)
            .ToListAsync();

        return Ok(applications.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var application = await _db.JobApplications
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        return application is null ? NotFound() : Ok(ToResponse(application));
    }


    [HttpPost("with-company")]
    public async Task<ActionResult<JobApplicationResponse>> CreateWithCompany(JobApplicationWithCompanyRequest request)
    {
        var userId = User.GetUserId();
        var now = DateTime.UtcNow;
        var companyName = request.CompanyName.Trim();

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == companyName.ToLower());

        if (company is null)
        {
            company = new Company
            {
                UserId = userId,
                Name = companyName,
                Website = request.Website,
                Location = request.Location,
                ContactName = request.ContactName,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                Notes = request.CompanyNotes,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.Companies.Add(company);
        }
        else
        {
            company.Website = Merge(company.Website, request.Website);
            company.Location = Merge(company.Location, request.Location);
            company.ContactName = Merge(company.ContactName, request.ContactName);
            company.ContactEmail = Merge(company.ContactEmail, request.ContactEmail);
            company.ContactPhone = Merge(company.ContactPhone, request.ContactPhone);
            company.Notes = Merge(company.Notes, request.CompanyNotes);
            company.UpdatedAt = now;
        }

        var application = new JobApplication
        {
            UserId = userId,
            Company = company,
            PositionTitle = request.PositionTitle.Trim(),
            ApplicationDate = request.ApplicationDate,
            Status = request.Status,
            JobAdUrl = request.JobAdUrl,
            SalaryRange = request.SalaryRange,
            InterviewDateTime = request.InterviewDateTime,
            ExperienceNotes = request.ExperienceNotes,
            NextStep = request.NextStep,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, ToResponse(application));
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationResponse>> Create(JobApplicationRequest request)
    {
        var userId = User.GetUserId();
        var companyExists = await _db.Companies.AnyAsync(c => c.Id == request.CompanyId && c.UserId == userId);
        if (!companyExists) return BadRequest("A megadott cég nem található ennél a felhasználónál.");

        var application = new JobApplication
        {
            UserId = userId,
            CompanyId = request.CompanyId,
            PositionTitle = request.PositionTitle.Trim(),
            ApplicationDate = request.ApplicationDate,
            Status = request.Status,
            JobAdUrl = request.JobAdUrl,
            SalaryRange = request.SalaryRange,
            InterviewDateTime = request.InterviewDateTime,
            ExperienceNotes = request.ExperienceNotes,
            NextStep = request.NextStep
        };

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();

        application.Company = await _db.Companies.FindAsync(application.CompanyId);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, ToResponse(application));
    }


    [HttpPut("{id:int}/with-company")]
    public async Task<ActionResult<JobApplicationResponse>> UpdateWithCompany(int id, JobApplicationWithCompanyRequest request)
    {
        var userId = User.GetUserId();
        var application = await _db.JobApplications
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (application is null) return NotFound();

        var now = DateTime.UtcNow;
        var companyName = request.CompanyName.Trim();

        var company = application.Company;
        if (company is null || !company.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase))
        {
            company = await _db.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == companyName.ToLower());

            if (company is null)
            {
                company = new Company
                {
                    UserId = userId,
                    Name = companyName,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.Companies.Add(company);
            }

            application.Company = company;
        }

        company.Name = companyName;
        company.Website = request.Website;
        company.Location = request.Location;
        company.ContactName = request.ContactName;
        company.ContactEmail = request.ContactEmail;
        company.ContactPhone = request.ContactPhone;
        company.Notes = request.CompanyNotes;
        company.UpdatedAt = now;

        application.PositionTitle = request.PositionTitle.Trim();
        application.ApplicationDate = request.ApplicationDate;
        application.Status = request.Status;
        application.JobAdUrl = request.JobAdUrl;
        application.SalaryRange = request.SalaryRange;
        application.InterviewDateTime = request.InterviewDateTime;
        application.ExperienceNotes = request.ExperienceNotes;
        application.NextStep = request.NextStep;
        application.UpdatedAt = now;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(application));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> Update(int id, JobApplicationRequest request)
    {
        var userId = User.GetUserId();
        var application = await _db.JobApplications
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (application is null) return NotFound();

        var companyExists = await _db.Companies.AnyAsync(c => c.Id == request.CompanyId && c.UserId == userId);
        if (!companyExists) return BadRequest("A megadott cég nem található ennél a felhasználónál.");

        application.CompanyId = request.CompanyId;
        application.PositionTitle = request.PositionTitle.Trim();
        application.ApplicationDate = request.ApplicationDate;
        application.Status = request.Status;
        application.JobAdUrl = request.JobAdUrl;
        application.SalaryRange = request.SalaryRange;
        application.InterviewDateTime = request.InterviewDateTime;
        application.ExperienceNotes = request.ExperienceNotes;
        application.NextStep = request.NextStep;

        await _db.SaveChangesAsync();
        application.Company = await _db.Companies.FindAsync(application.CompanyId);

        return Ok(ToResponse(application));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var application = await _db.JobApplications.FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);
        if (application is null) return NotFound();

        _db.JobApplications.Remove(application);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string? Merge(string? currentValue, string? newValue) =>
        string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;

    private static JobApplicationResponse ToResponse(JobApplication j) => new(
        j.Id,
        j.CompanyId,
        j.Company?.Name,
        j.Company?.Website,
        j.Company?.Location,
        j.Company?.ContactName,
        j.Company?.ContactEmail,
        j.Company?.ContactPhone,
        j.Company?.Notes,
        j.PositionTitle,
        j.ApplicationDate,
        j.Status,
        j.JobAdUrl,
        j.SalaryRange,
        j.InterviewDateTime,
        j.ExperienceNotes,
        j.NextStep,
        j.CreatedAt,
        j.UpdatedAt
    );
}
