using Tacdent.Core.DTOs;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface ITestimonialService
{
    Task<IReadOnlyList<TestimonialDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestimonialDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<TestimonialDto>> CreateAsync(CreateTestimonialDto dto, CancellationToken cancellationToken = default);

    Task<Result<TestimonialDto>> UpdateAsync(int id, UpdateTestimonialDto dto, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
