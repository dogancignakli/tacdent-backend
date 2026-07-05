namespace Tacdent.Core.DTOs;

/// <summary>Output contract returned by the Application layer for a dental service.</summary>
public record ServiceDto(
    int Id,
    string NameTr,
    string NameEn,
    string DescriptionTr,
    string DescriptionEn,
    string? Icon,
    decimal PriceFromTry,
    decimal PriceFromEur,
    int DurationMinutes,
    int DisplayOrder,
    bool IsActive
);
