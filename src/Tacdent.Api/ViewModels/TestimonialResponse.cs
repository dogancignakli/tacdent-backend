namespace Tacdent.Api.ViewModels;

public record TestimonialResponse(
    int Id,
    string AuthorName,
    string QuoteTr,
    string? QuoteEn,
    int? Rating,
    int DisplayOrder
);

public record AdminTestimonialResponse(
    int Id,
    string AuthorName,
    string QuoteTr,
    string? QuoteEn,
    int? Rating,
    bool IsActive,
    int DisplayOrder
);
