using Core.Client;
using Core.Error;
using Core.Scope;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Authorization.ParameterValidation;

public class AuthorizationParameterValidator(IClientRepository clientRepository, IScopeRepository scopeRepository, ILogger<AuthorizationParameterValidator> logger) : IAuthorizationParameterValidator
{
    private IClientRepository _clientRepository = clientRepository;
    private IScopeRepository _scopeRepository = scopeRepository;
    private ILogger<AuthorizationParameterValidator> _logger = logger;
    private readonly HashSet<string> _oauthResponseTypes = [ResponseType.Code, ResponseType.AccessToken, ResponseType.IdToken, ResponseType.None];

    public async Task<Result<bool>> ValidateClientId(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogDebug("The clientId is empty");
            return OAuthErrors.InvalidRequest;
        }

        ClientApplication client = await _clientRepository.GetById(clientId);

        if (client == null)
        {
            _logger.LogDebug("The client with id {clientId} does not exist", clientId);
            return OAuthErrors.InvalidRequest;
        }
        
        return true;
    }

    public Result<bool> ValidateRedirectUrl(string redirectUrl, ClientApplication client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (client.RedirectUrls == null || client.RedirectUrls.Count() == 0)
        {
            _logger.LogDebug("There are no registered redirect urls");
            return OAuthErrors.InvalidRequest;
        }

        if (string.IsNullOrWhiteSpace(redirectUrl) && client.RedirectUrls.Count() == 1)
        {
            _logger.LogInformation("RedirectUrl is empty in request but client has a redirectUrl");
            return true;
        }

        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? validRedirectUri) || validRedirectUri.Scheme != Uri.UriSchemeHttps)
        {
            _logger.LogDebug("RedirectUrl {RedirectUrl} is not a valid uri.", redirectUrl);
            return OAuthErrors.InvalidRequest;
        }

        bool urlIsRegistered = client.RedirectUrls.Any(x => string.Equals(x.Value.TrimEnd('/'), redirectUrl, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(redirectUrl) && urlIsRegistered)
        {
            _logger.LogInformation("RedirectUrl is valid");
            return true;
        }

        _logger.LogDebug("RedirectUrl is not registered for client");
        return OAuthErrors.InvalidRequest;
    }

    public Result<bool> ValidateResponseType(string requestedResponseType, ClientApplication client)
    {
        if (string.IsNullOrWhiteSpace(requestedResponseType))
        {
            _logger.LogDebug("ResponseType is empty");
            return OAuthErrors.InvalidRequest;
        }

        ArgumentNullException.ThrowIfNull(client);

        IEnumerable<string> responseTypes = requestedResponseType.Trim(' ').Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));

        if (!_oauthResponseTypes.IsSupersetOf(responseTypes))
        {
            _logger.LogDebug("Invalid response type {ResponseTypes}", responseTypes);
            return OAuthErrors.InvalidRequest;
        }

        if (client.ClientType == ClientType.Confidential && responseTypes.Contains(ResponseType.AccessToken))
        {
            _logger.LogDebug("Client is unauthorized to request 'token' response type");
            return OAuthErrors.UnauthorizedClient;
        }

        _logger.LogInformation("Response type is valid");
        return true;
    }

    public async Task<Result<bool>> ValidateScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            _logger.LogInformation("Scope is empty but allows basic information in token");
            return true;
        }

        IEnumerable<string> scopes = scope.Split(' ').Select(x => x.Trim(' '));

        HashSet<string> result = (await _scopeRepository.GetScopes(scopes.ToArray())).Select(x => x.Name).ToHashSet();

        foreach (var scopeItem in scopes)
        {
            if (!result.Contains(scopeItem))
            {
                _logger.LogDebug("Scope {ScopeItem} is invalid", scopeItem);
                return OAuthErrors.InvalidScope;
            }   
        }

        _logger.LogInformation("Scope is valid");
        return true;
    }
}