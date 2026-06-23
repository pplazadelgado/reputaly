using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reputaly.API.Infrastructure.Persistence;

namespace Reputaly.API.Features.Locations;

[ApiController]
public class GetLocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GetLocationsController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet("/locations")]
    public async Task<IActionResult> Handle()
    {
        var locations = await _db.TenantLocations
            .Select( l => new LocationDto(
                l.Id,
                l.Name,
                l.GoogleLocationId,
                l.GoogleAccountEmail,
                l.GoogleAccessToken != null,
                l.IsActive))
            .ToListAsync();

        return Ok(locations);
    }
}

public record LocationDto(
    Guid Id,
    string Name,
    string? GoogleLocationId,
    string? GoogleAccountEmail,
    bool IsGoogleConnected,
    bool IsActive);
