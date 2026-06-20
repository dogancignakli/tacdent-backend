using Tacdent.Core.Entities.Common;

namespace Tacdent.Core.Entities;

public class DentalService : AuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Icon { get; set; }
    public decimal PriceFrom { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}
