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
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IScopeRepository _scopeRepository = scopeRepository;
    private readonly ILogger<AuthorizationParameterValidator> _logger = logger;
    private readonly HashSet<string> _oauthResponseTypes = [ResponseType.Code, ResponseType.AccessToken, ResponseType.IdToken, ResponseType.None];

    public async Task<Result<ClientApplication>> ValidateClientId(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogDebug("The clientId is empty");
            return ErrorResponse.InvalidRequest;
        }

        ClientApplication client = await _clientRepository.GetById(clientId);

        if (client == null)
        {
            _logger.LogDebug("The client with id {clientId} does not exist", clientId);
            return ErrorResponse.InvalidRequest;
        }
        
        return client;
    }

    public Result<bool> ValidateRedirectUrl(string redirectUrl, ClientApplication client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (client.RedirectUrls == null || client.RedirectUrls.Count() == 0)
        {
            _logger.LogDebug("There are no registered redirect urls");
            return ErrorResponse.InvalidRequest;
        }

        if (string.IsNullOrWhiteSpace(redirectUrl) && client.RedirectUrls.Count() == 1)
        {
            _logger.LogInformation("RedirectUrl is empty in request but client has a redirectUrl");
            return true;
        }

        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out Uri? validRedirectUri) || validRedirectUri.Scheme != Uri.UriSchemeHttps)
        {
            _logger.LogDebug("RedirectUrl {RedirectUrl} is not a valid uri.", redirectUrl);
            return ErrorResponse.InvalidRequest;
        }

        bool urlIsRegistered = client.RedirectUrls.Any(x => string.Equals(x.Value.TrimEnd('/'), redirectUrl, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(redirectUrl) && urlIsRegistered)
        {
            _logger.LogInformation("RedirectUrl is valid");
            return true;
        }

        _logger.LogDebug("RedirectUrl is not registered for client");
        return ErrorResponse.InvalidRequest;
    }

    public Result<bool> ValidateResponseType(string requestedResponseType, ClientApplication client)
    {
        if (string.IsNullOrWhiteSpace(requestedResponseType))
        {
            _logger.LogDebug("ResponseType is empty");
            return ErrorResponse.InvalidRequest;
        }

        ArgumentNullException.ThrowIfNull(client);

        IEnumerable<string> responseTypes = requestedResponseType.Trim(' ').Split(' ').Where(x => !string.IsNullOrWhiteSpace(x));

        if (!_oauthResponseTypes.IsSupersetOf(responseTypes))
        {
            _logger.LogDebug("Invalid response type {ResponseTypes}", responseTypes);
            return ErrorResponse.InvalidRequest;
        }

        if (client.ClientType == ClientType.Confidential && responseTypes.Contains(ResponseType.AccessToken))
        {
            _logger.LogDebug("Client is unauthorized to request 'token' response type");
            return ErrorResponse.UnauthorizedClient;
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

        HashSet<string> validScopes = (await _scopeRepository.GetScopes(scopes.ToArray())).Select(x => x.Name).ToHashSet();

        foreach (var scopeItem in scopes)
        {
            if (!validScopes.Contains(scopeItem))
            {
                _logger.LogDebug("Scope {ScopeItem} is invalid", scopeItem);
                return ErrorResponse.InvalidScope;
            }   
        }

        _logger.LogInformation("Scope is valid");
        return true;
    }
}