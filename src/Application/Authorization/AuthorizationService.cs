using System;
using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Core;
using Core.Authorization.ParameterValidation;
using Core.Client;
using Core.Authorization;

namespace Application.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationParameterValidator _authorizationParameterValidator;

    public AuthorizationService(IAuthorizationParameterValidator authorizationParameterValidator)
    {
        _authorizationParameterValidator = authorizationParameterValidator;
    }

    public async Task<AuthorizationResult> ValidateRequest(AuthorizationRequest authorizationRequest)
    {
        ArgumentNullException.ThrowIfNull(authorizationRequest);

        Result<ClientApplication> clientValidationResult = await _authorizationParameterValidator.ValidateClientId(authorizationRequest.ClientId);
        
        if (clientValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, clientValidationResult.ErrorResponse);

        ClientApplication clientApplication = clientValidationResult.Value;

        Result<bool> redirectUrlValidationResult = _authorizationParameterValidator.ValidateRedirectUrl(authorizationRequest.RedirectUri, clientApplication);

        if (redirectUrlValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, redirectUrlValidationResult.ErrorResponse);

        Result<bool> responseTypeValidationResult = _authorizationParameterValidator.ValidateResponseType(authorizationRequest.ResponseType, clientApplication);

        if (responseTypeValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, responseTypeValidationResult.ErrorResponse);

        Result<bool> scopeValidationResult = await _authorizationParameterValidator.ValidateScope(authorizationRequest.Scope);

        if (scopeValidationResult.IsFailure)
            return new AuthorizationResult(authorizationRequest, scopeValidationResult.ErrorResponse);
            
        return new AuthorizationResult(authorizationRequest);
    }
}
