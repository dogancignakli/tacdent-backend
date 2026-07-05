using Tacdent.Core.Entities.Common;

namespace Tacdent.Core.Entities;

public class DentalService : AuditableEntity
{
    public int Id { get; set; }
    public required string NameTr { get; set; }
    public required string NameEn { get; set; }
    public required string DescriptionTr { get; set; }
    public required string DescriptionEn { get; set; }
    public string? Icon { get; set; }
    public decimal PriceFromTry { get; set; }
    public decimal PriceFromEur { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
