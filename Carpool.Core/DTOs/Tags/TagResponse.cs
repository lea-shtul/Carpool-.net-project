namespace Carpool.Core.Dtos.Tags;

/// <summary>Public projection of a <see cref="Entities.Tag"/>.</summary>
public class TagResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <c>false</c> for a global/seed tag visible to everyone; <c>true</c> for a private
    /// tag the caller created for themselves. Not part of the base spec — see
    /// <see cref="Entities.Tag"/>.
    /// </summary>
    public bool IsPrivate { get; set; }
}
