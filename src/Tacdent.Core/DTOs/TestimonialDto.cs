namespace Tacdent.Core.DTOs;

public record TestimonialDto(
    int Id,
    string AuthorName,
    string QuoteTr,
    string? QuoteEn,
    int? Rating,
    bool IsActive,
    int DisplayOrder
);
