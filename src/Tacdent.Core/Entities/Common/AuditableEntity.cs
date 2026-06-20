namespace Tacdent.Core.Entities.Common;

/// <summary>
/// Base type for entities that track when they were created and last changed.
/// The values are populated centrally by the audit save-changes interceptor in the Data layer.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
