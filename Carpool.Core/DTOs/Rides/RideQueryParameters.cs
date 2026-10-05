using System.ComponentModel.DataAnnotations;

namespace Carpool.Core.Dtos.Rides;

/// <summary>
/// Filtering, sorting and paging options for <c>GET /api/rides</c> (spec §20),
/// bound with <c>[FromQuery]</c> (spec §37). Filtering/sorting/paging are all
/// applied in the database via LINQ + <c>Skip</c>/<c>Take</c> (spec §65).
/// </summary>
public class RideQueryParameters
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public int? TagId { get; set; }

    [Range(0, 100000)]
    public decimal? MinPrice { get; set; }

    [Range(0, 100000)]
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// When true, return only rides that are still bookable: status Scheduled,
    /// departure in the future and <c>AvailableSeats &gt; 0</c> (full rides are hidden, spec §19).
    /// </summary>
    public bool AvailableOnly { get; set; }

    /// <summary>One of: <c>departureTime</c>, <c>price</c>, <c>availableSeats</c>. Defaults to departure time.</summary>
    public string? SortBy { get; set; }

    /// <summary><c>asc</c> or <c>desc</c>. Defaults to ascending.</summary>
    public string? SortDirection { get; set; }
}
