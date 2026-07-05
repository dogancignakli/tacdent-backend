namespace Tacdent.Core.DTOs;

public record CreateTestimonialDto(
    string AuthorName,
    string QuoteTr,
    string? QuoteEn,
    int? Rating,
    bool IsActive,
    int DisplayOrder
);
