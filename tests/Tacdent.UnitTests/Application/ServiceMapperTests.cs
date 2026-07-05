using Tacdent.Application.Mapping;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class ServiceMapperTests
{
    private readonly ServiceMapper _sut = new();

    [Fact]
    public void ToDto_MapsPublicFields()
    {
        var entity = TestData.SampleService();

        var dto = _sut.ToDto(entity);

        dto.Id.ShouldBe(entity.Id);
        dto.NameTr.ShouldBe(entity.NameTr);
        dto.NameEn.ShouldBe(entity.NameEn);
        dto.DescriptionTr.ShouldBe(entity.DescriptionTr);
        dto.DescriptionEn.ShouldBe(entity.DescriptionEn);
        dto.Icon.ShouldBe(entity.Icon);
        dto.PriceFromTry.ShouldBe(entity.PriceFromTry);
        dto.PriceFromEur.ShouldBe(entity.PriceFromEur);
        dto.DurationMinutes.ShouldBe(entity.DurationMinutes);
        dto.DisplayOrder.ShouldBe(entity.DisplayOrder);
        dto.IsActive.ShouldBe(entity.IsActive);
    }

    [Fact]
    public void ToDtoList_MapsAllEntities()
    {
        var entities = new List<Tacdent.Core.Entities.DentalService> { TestData.SampleService() }.AsReadOnly();

        var dtos = _sut.ToDtoList(entities);

        dtos.ShouldHaveSingleItem();
        dtos[0].NameTr.ShouldBe(entities[0].NameTr);
    }
}
