using System.Collections.Specialized;
using System.Threading.Tasks;
using Core;
using Core.Authorization;

namespace Application.Authorization
{
    public interface IAuthorizationService
    {
        Task<AuthorizationResult> ValidateRequest(NameValueCollection requestParameters);
    }
}
