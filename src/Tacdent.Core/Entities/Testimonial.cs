using Tacdent.Core.Entities.Common;

namespace Tacdent.Core.Entities;

public class Testimonial : AuditableEntity
{
    public int Id { get; set; }
    public required string AuthorName { get; set; }
    public required string QuoteTr { get; set; }
    public string? QuoteEn { get; set; }
    public int? Rating { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
