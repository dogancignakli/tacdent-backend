namespace Tacdent.Api.ViewModels;

public class UpdateTestimonialRequest
{
    public string AuthorName { get; set; } = string.Empty;
    public string QuoteTr { get; set; } = string.Empty;
    public string? QuoteEn { get; set; }
    public int? Rating { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
