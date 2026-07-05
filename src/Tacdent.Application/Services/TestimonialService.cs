using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class TestimonialService(IUnitOfWork unitOfWork, TestimonialMapper mapper) : ITestimonialService
{
    public async Task<IReadOnlyList<TestimonialDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var testimonials = await unitOfWork.Testimonials.GetActiveAsync(cancellationToken);
        return mapper.ToDtoList(testimonials);
    }

    public async Task<IReadOnlyList<TestimonialDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var testimonials = await unitOfWork.Testimonials.GetAllOrderedAsync(cancellationToken);
        return mapper.ToDtoList(testimonials);
    }

    public async Task<Result<TestimonialDto>> CreateAsync(
        CreateTestimonialDto dto,
        CancellationToken cancellationToken = default)
    {
        var testimonial = new Testimonial
        {
            AuthorName = dto.AuthorName.Trim(),
            QuoteTr = dto.QuoteTr.Trim(),
            QuoteEn = string.IsNullOrWhiteSpace(dto.QuoteEn) ? null : dto.QuoteEn.Trim(),
            Rating = dto.Rating,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
        };

        await unitOfWork.Testimonials.AddAsync(testimonial, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(testimonial));
    }

    public async Task<Result<TestimonialDto>> UpdateAsync(
        int id,
        UpdateTestimonialDto dto,
        CancellationToken cancellationToken = default)
    {
        var testimonial = await unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial is null)
        {
            return Result.Failure<TestimonialDto>(TestimonialErrors.NotFound(id));
        }

        testimonial.AuthorName = dto.AuthorName.Trim();
        testimonial.QuoteTr = dto.QuoteTr.Trim();
        testimonial.QuoteEn = string.IsNullOrWhiteSpace(dto.QuoteEn) ? null : dto.QuoteEn.Trim();
        testimonial.Rating = dto.Rating;
        testimonial.IsActive = dto.IsActive;
        testimonial.DisplayOrder = dto.DisplayOrder;

        unitOfWork.Testimonials.Update(testimonial);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(testimonial));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var testimonial = await unitOfWork.Testimonials.GetByIdAsync(id, cancellationToken);
        if (testimonial is null)
        {
            return Result.Failure(TestimonialErrors.NotFound(id));
        }

        unitOfWork.Testimonials.Remove(testimonial);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
