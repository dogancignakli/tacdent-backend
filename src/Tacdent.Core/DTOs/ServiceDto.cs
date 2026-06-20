namespace Tacdent.Core.DTOs;

/// <summary>Output contract returned by the Application layer for a dental service.</summary>
public record ServiceDto(
    int Id,
    string Name,
    string Description,
    string? Icon,
    decimal PriceFrom,
    int DurationMinutes
);
