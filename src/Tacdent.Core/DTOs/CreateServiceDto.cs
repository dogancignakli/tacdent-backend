namespace Tacdent.Core.DTOs;

public record CreateServiceDto(
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
