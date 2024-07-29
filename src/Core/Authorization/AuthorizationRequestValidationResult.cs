using Core.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Authorization
{
    public class AuthorizationRequestValidationResult(AuthorizationRequest authorizationRequest, bool isSuccess, bool shouldRedirectToClient, ErrorResponse? errorResponse = null)
    {

        public AuthorizationRequest AuthorizationRequest { get; } = authorizationRequest;

        public bool IsSuccess { get; } = isSuccess;
        public bool ShouldRedirectToClient { get; } = shouldRedirectToClient;
        public ErrorResponse? ErrorResponse { get; } = errorResponse;
    }
}
