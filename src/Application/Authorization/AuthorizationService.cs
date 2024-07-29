//using Application.Parameters;
//using Core.Client;
//using Core.Error;
//using Core.Scope;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using static Core.Error.Constants;

//namespace Application.Authorization
//{
//    public sealed class AuthorizationService(IParameterValidator parameterValidator, ILogger<AuthorizationService> logger) : IAuthorizationService
//    {
//        private IParameterValidator _parameterValidator = parameterValidator;
//        private ILogger<AuthorizationService> _logger = logger;
//        private readonly HashSet<string> _responseTypes = ["code", "token"];

//        /// <summary>
//        /// Validates the authorization request's parameters. Prevent open redirector attack by not redirecting to a redirect_uri 
//        /// if there is any error with the redirect_uri parameter. see: https://oauth.net/advisories/2014-1-covert-redirect/
//        /// </summary>
//        /// <param name="authorizationRequest"></param>
//        /// <returns></returns>
//        public async Task<AuthorizationRequestValidationResult> VerifyRequest(AuthorizationRequest authorizationRequest)
//        {
//            ArgumentNullException.ThrowIfNull(authorizationRequest);

//            // validate client_id
//            ClientApplication client = await _clientRepository.GetById(authorizationRequest.ClientId);

//            if (client == null)
//            {
//                var errorResponse = new ErrorResponse(OAuthError.InvalidRequest, OAuthErrorDescription.InvalidRequest);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, false, errorResponse);
//            }

//            // redirect uri
//            string redirectUri = string.Empty;

//            if (string.IsNullOrWhiteSpace(authorizationRequest.RedirectUri) && client.RedirectUrls.Count() == 1)
//                redirectUri = client.RedirectUrls.ToList()[0].Value;

//            if (!string.IsNullOrWhiteSpace(authorizationRequest.RedirectUri) && !client.RedirectUrls.Any(x => x.Value == authorizationRequest.RedirectUri)) // might need to ensure /
//            {
//                var errorResponse = new ErrorResponse(OAuthError.InvalidRequest, OAuthErrorDescription.InvalidRequest);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, false, errorResponse);
//            }

//            var redirectUriIsValid = Uri.TryCreate(authorizationRequest.RedirectUri, UriKind.Absolute, out Uri? validRedirectUri);

//            if (!redirectUriIsValid)
//            {
//                var errorResponse = new ErrorResponse(OAuthError.InvalidRequest, OAuthErrorDescription.InvalidRequest);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, false, errorResponse);
//            }

//            // responsetype
//            if (!_responseTypes.Contains(authorizationRequest.ResponseType))
//            {
//                var errorResponse = new ErrorResponse(OAuthError.InvalidRequest, OAuthErrorDescription.InvalidRequest);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, true, errorResponse);
//            }

//            if (client.ClientType == ClientType.Confidential && authorizationRequest.ResponseType == "token")
//            {
//                var errorResponse = new ErrorResponse(OAuthError.UnauthorizedClient, OAuthErrorDescription.UnauthorizedClient);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, true, errorResponse);
//            }

//            // scope
//            string[] scopes = authorizationRequest.Scope.Split(' ');

//            if (scopes.Length != (await _scopeRepository.GetMany(scopes)).Count())
//            {
//                var errorResponse = new ErrorResponse(OAuthError.InvalidScope, OAuthErrorDescription.InvalidScope);
//                return new AuthorizationRequestValidationResult(authorizationRequest, false, true, errorResponse);
//            }

//            // TODO: abstract each parameter verification into methods in another class
//            // state
//            // TODO: store state

//            // PKCE
//            // TODO: store PKCE
//            // TODO: verfiy PKCE

//            return null;
//        }
//    }
//}
