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
        dto.Name.ShouldBe(entity.Name);
        dto.Description.ShouldBe(entity.Description);
        dto.Icon.ShouldBe(entity.Icon);
        dto.PriceFrom.ShouldBe(entity.PriceFrom);
        dto.DurationMinutes.ShouldBe(entity.DurationMinutes);
    }

    [Fact]
    public void ToDtoList_MapsAllEntities()
    {
        var entities = new List<Tacdent.Core.Entities.DentalService> { TestData.SampleService() }.AsReadOnly();

        var dtos = _sut.ToDtoList(entities);

        dtos.ShouldHaveSingleItem();
        dtos[0].Name.ShouldBe(entities[0].Name);
    }
}
