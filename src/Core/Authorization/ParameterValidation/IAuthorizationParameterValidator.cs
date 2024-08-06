using Core.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Authorization.ParameterValidation;

public interface IAuthorizationParameterValidator
{
    Task<Result<bool>> ValidateClientId(string clientId);
    Result<bool> ValidateRedirectUrl(string redirectUrl, IEnumerable<RedirectUrl> registeredRedirectUrls);
    Result<bool> ValidateResponseType(string responseType, ClientType clientType);
    Task<Result<bool>> ValidateScope(string scope);
}