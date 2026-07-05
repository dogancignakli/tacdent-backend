using Moq;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services;
using Tacdent.Core.Entities;
using Tacdent.Data.Repositories.Interfaces;
using Tacdent.UnitTests.Infrastructure;

namespace Tacdent.UnitTests.Application;

public class ServiceCatalogServiceTests
{
    [Fact]
    public async Task GetActiveAsync_ReturnsMappedActiveServices()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var serviceRepository = new Mock<IServiceRepository>();
        var service = TestData.SampleService();

        unitOfWork.SetupGet(u => u.Services).Returns(serviceRepository.Object);
        serviceRepository
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DentalService> { service }.AsReadOnly());

        var sut = new ServiceCatalogService(unitOfWork.Object, new ServiceMapper());

        var result = await sut.GetActiveAsync();

        result.ShouldHaveSingleItem();
        result[0].Id.ShouldBe(service.Id);
        result[0].NameTr.ShouldBe(service.NameTr);
        result[0].DescriptionTr.ShouldBe(service.DescriptionTr);
    }
}
