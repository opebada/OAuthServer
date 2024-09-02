using Core.Error;

namespace Core.Authorization
{
    public class AuthorizationResult(AuthorizationRequest authorizationRequest, ErrorResponse? errorResponse = null)
    {

        public AuthorizationRequest AuthorizationRequest { get; } = authorizationRequest;

        public bool IsValid { get; } = errorResponse == null;
        public ErrorResponse? ErrorResponse { get; } = errorResponse;
    }
}
