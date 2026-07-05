namespace Tacdent.Api.ViewModels;

public class UpdateServiceRequest
{
    public string NameTr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public decimal PriceFromTry { get; set; }
    public decimal PriceFromEur { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
