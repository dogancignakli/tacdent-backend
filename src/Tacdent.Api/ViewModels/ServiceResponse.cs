namespace Tacdent.Api.ViewModels;

public record ServiceResponse(
    int Id,
    string Name,
    string Description,
    string? Icon,
    decimal PriceFrom,
    int DurationMinutes
);
