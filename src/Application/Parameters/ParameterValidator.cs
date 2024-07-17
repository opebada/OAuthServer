using Application.Authorization;
using Core.Client;
using Core.Error;
using Core.Scope;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Error.Constants;
using static System.Formats.Asn1.AsnWriter;

namespace Application.Parameters
{
    public class ParameterValidator(IClientRepository clientRepository, IScopeRepository scopeRepository, ILogger<ParameterValidator> logger) : IParameterValidator
    {
        private IClientRepository _clientRepository = clientRepository;
        private IScopeRepository _scopeRepository = scopeRepository;
        private ILogger<ParameterValidator> _logger = logger;
        private readonly HashSet<string> _responseTypes = [ResponseType.Code, ResponseType.AccessToken, ResponseType.IdToken, ResponseType.None];

        public async Task<ParameterValidationResult> ValidateClientId(string clientId)
        {
            ClientApplication client = await _clientRepository.GetById(clientId);

            if (client == null)
                return new ParameterValidationResult(false, OAuthError.InvalidRequest);

            return new ParameterValidationResult(true);
        }

        public async Task<ParameterValidationResult> ValidateRedirectUrl(string clientId, string redirectUrl)
        {
            ClientApplication client = await _clientRepository.GetById(clientId);

            if (string.IsNullOrWhiteSpace(redirectUrl) && client?.RedirectUrls?.Count() == 1)
                redirectUrl = client.RedirectUrls.ToList()[0].Value;

            if (!string.IsNullOrWhiteSpace(redirectUrl) && !client.RedirectUrls.Any(x => x.Value == redirectUrl)) // might need to ensure /
                return new ParameterValidationResult(false, OAuthError.InvalidRequest);

            var redirectUriIsValid = Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? validRedirectUri);

            if (!redirectUriIsValid)
                return new ParameterValidationResult(false, OAuthError.InvalidRequest);

            return new ParameterValidationResult(true);
        }

        public async Task<ParameterValidationResult> ValidateResponseType(string clientId, string responseType)
        {
            ClientApplication client = await _clientRepository.GetById(clientId);

            string[] responseTypes = responseType.Split(' ');

            if (!responseTypes.All(x => _responseTypes.Contains(x)))
                return new ParameterValidationResult(false, OAuthError.InvalidRequest);


            if (client.ClientType == ClientType.Confidential && responseType == ResponseType.AccessToken)
                return new ParameterValidationResult(false, OAuthError.UnauthorizedClient);

            return new ParameterValidationResult(true);
        }

        public async Task<ParameterValidationResult> ValidateScope(string scope)
        {
            string[] scopes = scope.Split(' ');

            if (scopes.Length != (await _scopeRepository.GetMany(scopes)).Count())
                return new ParameterValidationResult(false, OAuthError.InvalidScope);

            return new ParameterValidationResult(true);
        }
    }
}
