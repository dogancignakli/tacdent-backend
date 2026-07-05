using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class ServiceErrors
{
    public static Error NotFound(int id) =>
        new("Service.NotFound", $"Service with id '{id}' was not found.", ErrorType.NotFound);

    public static readonly Error InUse =
        new("Service.InUse", "Service cannot be deleted because it is linked to appointments.", ErrorType.Conflict);
}
