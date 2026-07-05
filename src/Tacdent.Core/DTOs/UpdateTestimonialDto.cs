namespace Tacdent.Core.DTOs;

public record UpdateTestimonialDto(
    string AuthorName,
    string QuoteTr,
    string? QuoteEn,
    int? Rating,
    bool IsActive,
    int DisplayOrder
);
