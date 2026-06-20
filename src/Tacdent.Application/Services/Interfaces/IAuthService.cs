using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IAuthService
{
    Result Authenticate(string password);
}
