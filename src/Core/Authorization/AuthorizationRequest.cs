using System;
using System.Collections.Specialized;
using System.Linq;

namespace Application.Authorization
{
    public sealed class AuthorizationRequest
    {
        private const string ClientIdConstant = "client_id";
        private NameValueCollection _requestParameters;


        public AuthorizationRequest(NameValueCollection requestParameters) 
        { 
            _requestParameters = requestParameters;

            SetProperties(requestParameters);
        }

        public string ClientId { get; set; }
        public required string RedirectUri { get; set; }
        public required string ResponseType { get; set; }
        public string? Scope { get; set; }
        public required string State { get; set; }
        public string? CodeChallenge { get; set; }
        public string? CodeChallengeMethod { get; set; }

        private void SetProperties(NameValueCollection requestParameters)
        {
            if (requestParameters.GetValues(ClientIdConstant)?.Count() == 1)
                ClientId = requestParameters[ClientIdConstant];

            if (requestParameters.GetValues("redirect_uri")?.Count() == 1)
                RedirectUri = requestParameters["redirect_uri"];

            if (requestParameters.GetValues("response_type")?.Count() == 1)
                ResponseType = requestParameters["response_type"];            
            
            if (requestParameters.GetValues("scope")?.Count() == 1)
                Scope = requestParameters["scope"];
            
            if (requestParameters.GetValues("state")?.Count() == 1)
                State = requestParameters["state"];
            
            if (requestParameters.GetValues("code_challenge")?.Count() == 1)
                CodeChallenge = requestParameters["code_challenge"];
            
            if (requestParameters.GetValues("code_challenge_method")?.Count() == 1)
                CodeChallenge = requestParameters["code_challenge_method"];
        }
    }
}
