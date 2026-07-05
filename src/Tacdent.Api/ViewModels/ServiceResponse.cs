namespace Tacdent.Api.ViewModels;

public record ServiceResponse(
    int Id,
    string NameTr,
    string NameEn,
    string DescriptionTr,
    string DescriptionEn,
    string? Icon,
    decimal PriceFromTry,
    decimal PriceFromEur,
    int DurationMinutes,
    int DisplayOrder
);

public record AdminServiceResponse(
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
