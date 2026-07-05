using Riok.Mapperly.Abstractions;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Application.Mapping;

[Mapper]
public partial class TestimonialMapper
{
    [MapperIgnoreSource(nameof(Testimonial.CreatedAt))]
    [MapperIgnoreSource(nameof(Testimonial.UpdatedAt))]
    public partial TestimonialDto ToDto(Testimonial entity);

    public partial IReadOnlyList<TestimonialDto> ToDtoList(IReadOnlyList<Testimonial> entities);
}
