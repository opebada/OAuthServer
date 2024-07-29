using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Authorization
{
    public interface IAuthorizationService
    {
        Task<AuthorizationRequestValidationResult> VerifyRequest(AuthorizationRequest authorizationRequest);
    }
}
