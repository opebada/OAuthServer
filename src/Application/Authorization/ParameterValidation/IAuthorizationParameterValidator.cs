using Core.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Authorization.ParameterValidation
{
    public interface IAuthorizationParameterValidator
    {
        Task<AuthorizationParameterValidationResult> ValidateClientId(string clientId);
        AuthorizationParameterValidationResult ValidateRedirectUrl(string redirectUrl, IEnumerable<RedirectUrl> registeredRedirectUrls);
        AuthorizationParameterValidationResult ValidateResponseType(string responseType, ClientType clientType);
        Task<AuthorizationParameterValidationResult> ValidateScope(string scope);
    }
}
