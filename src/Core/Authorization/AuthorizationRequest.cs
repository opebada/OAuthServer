using System;
using System.Collections.Specialized;
using System.Linq;

namespace Core.Authorization
{
    public sealed class AuthorizationRequest
    {
        public required string ClientId { get; set; }
        public required string RedirectUri { get; set; }
        public required string ResponseType { get; set; }
        public string? Scope { get; set; }
        public required string State { get; set; }
        public string? CodeChallenge { get; set; }
        public string? CodeChallengeMethod { get; set; }
    }
}
