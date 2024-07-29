using Core.Client;
using Core.Error;
using Core.Scope;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Authorization.ParameterValidation
{
    public class AuthorizationParameterValidator(IClientRepository clientRepository, IScopeRepository scopeRepository, ILogger<AuthorizationParameterValidator> logger) : IAuthorizationParameterValidator
    {
        private IClientRepository _clientRepository = clientRepository;
        private IScopeRepository _scopeRepository = scopeRepository;
        private ILogger<AuthorizationParameterValidator> _logger = logger;
        private readonly HashSet<string> _responseTypes = [ResponseType.Code, ResponseType.AccessToken, ResponseType.IdToken, ResponseType.None];

        public async Task<AuthorizationParameterValidationResult> ValidateClientId(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return OAuthErrors.InvalidRequest;

            ClientApplication client = await _clientRepository.GetById(clientId);

            if (client == null)
                return OAuthErrors.InvalidRequest;

            return AuthorizationParameterValidationResult.Success();
        }

        public AuthorizationParameterValidationResult ValidateRedirectUrl(string redirectUrl, IEnumerable<RedirectUrl> registeredRedirectUrls)
        {
            if (registeredRedirectUrls == null || registeredRedirectUrls.Count() == 0)
                return OAuthErrors.InvalidRequest;

            if (string.IsNullOrWhiteSpace(redirectUrl) && registeredRedirectUrls?.Count() == 1)
                return AuthorizationParameterValidationResult.Success();

            if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? validRedirectUri) || validRedirectUri.Scheme != Uri.UriSchemeHttps)
                return OAuthErrors.InvalidRequest;

            bool urlIsRegistered = registeredRedirectUrls.Any(x => x.Value.TrimEnd('/').Equals(redirectUrl, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(redirectUrl) && urlIsRegistered)
                return AuthorizationParameterValidationResult.Success();

            return OAuthErrors.InvalidRequest;
        }

        public AuthorizationParameterValidationResult ValidateResponseType(string responseType, ClientType clientType)
        {
            if (string.IsNullOrWhiteSpace(responseType))
                return OAuthErrors.InvalidRequest;

            IEnumerable<string> responseTypes = responseType.Trim(' ').Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));

            if (!_responseTypes.IsSupersetOf(responseTypes))
                return OAuthErrors.InvalidRequest;

            if (clientType == ClientType.Confidential && responseTypes.Contains(ResponseType.AccessToken)) // check this out
                return OAuthErrors.UnauthorizedClient;

            return AuthorizationParameterValidationResult.Success();
        }

        public async Task<AuthorizationParameterValidationResult> ValidateScope(string scope)
        {
            scope ??= string.Empty;

            IEnumerable<string> scopes = scope.Split(' ').Select(x => x.Trim(' '));

            var result = await _scopeRepository.GetScopes(scopes.ToArray());

            if (scopes.Count() != result.Count())
                return OAuthErrors.InvalidScope;

            return AuthorizationParameterValidationResult.Success();
        }
    }
}
