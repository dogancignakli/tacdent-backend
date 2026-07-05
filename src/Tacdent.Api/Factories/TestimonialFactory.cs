using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public class TestimonialFactory : ITestimonialFactory
{
    public TestimonialResponse ToResponse(TestimonialDto dto) =>
        new(dto.Id, dto.AuthorName, dto.QuoteTr, dto.QuoteEn, dto.Rating, dto.DisplayOrder);

    public AdminTestimonialResponse ToAdminResponse(TestimonialDto dto) =>
        new(dto.Id, dto.AuthorName, dto.QuoteTr, dto.QuoteEn, dto.Rating, dto.IsActive, dto.DisplayOrder);

    public CreateTestimonialDto ToCreateDto(CreateTestimonialRequest request) =>
        new(
            request.AuthorName.Trim(),
            request.QuoteTr.Trim(),
            string.IsNullOrWhiteSpace(request.QuoteEn) ? null : request.QuoteEn.Trim(),
            request.Rating,
            request.IsActive,
            request.DisplayOrder);

    public UpdateTestimonialDto ToUpdateDto(UpdateTestimonialRequest request) =>
        new(
            request.AuthorName.Trim(),
            request.QuoteTr.Trim(),
            string.IsNullOrWhiteSpace(request.QuoteEn) ? null : request.QuoteEn.Trim(),
            request.Rating,
            request.IsActive,
            request.DisplayOrder);
}
