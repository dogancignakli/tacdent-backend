using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public interface ITestimonialFactory
{
    TestimonialResponse ToResponse(TestimonialDto dto);

    AdminTestimonialResponse ToAdminResponse(TestimonialDto dto);

    CreateTestimonialDto ToCreateDto(CreateTestimonialRequest request);

    UpdateTestimonialDto ToUpdateDto(UpdateTestimonialRequest request);
}
