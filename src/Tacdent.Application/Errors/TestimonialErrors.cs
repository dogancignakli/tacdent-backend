using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class TestimonialErrors
{
    public static Error NotFound(int id) =>
        new("Testimonial.NotFound", $"Testimonial with id '{id}' was not found.", ErrorType.NotFound);
}
