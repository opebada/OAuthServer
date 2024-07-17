using Core.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Parameters
{
    public interface IParameterValidator
    {
        Task<ParameterValidationResult> ValidateClientId(string clientId);
        Task<ParameterValidationResult> ValidateRedirectUrl(string clientId, string redirectUrl);
        Task<ParameterValidationResult> ValidateResponseType(string clientId, string responseType);
        Task<ParameterValidationResult> ValidateScope(string scope);
    }
}
