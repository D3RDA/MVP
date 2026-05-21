using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Companies;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CompaniesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAll()
    {
        var userId = User.GetUserId();
        var companies = await _db.Companies
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(companies.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        return company is null ? NotFound() : Ok(ToResponse(company));
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create(CompanyRequest request)
    {
        var userId = User.GetUserId();
        var company = new Company
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Website = request.Website,
            Location = request.Location,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Notes = request.Notes
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = company.Id }, ToResponse(company));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CompanyResponse>> Update(int id, CompanyRequest request)
    {
        var userId = User.GetUserId();
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (company is null) return NotFound();

        company.Name = request.Name.Trim();
        company.Website = request.Website;
        company.Location = request.Location;
        company.ContactName = request.ContactName;
        company.ContactEmail = request.ContactEmail;
        company.ContactPhone = request.ContactPhone;
        company.Notes = request.Notes;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(company));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (company is null) return NotFound();

        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CompanyResponse ToResponse(Company c) => new(
        c.Id, c.Name, c.Website, c.Location, c.ContactName, c.ContactEmail,
        c.ContactPhone, c.Notes, c.CreatedAt, c.UpdatedAt
    );
}
