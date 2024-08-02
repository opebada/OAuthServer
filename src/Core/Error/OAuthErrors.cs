using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Error
{
    public class OAuthErrors
    {
        public static ErrorResponse InvalidRequest = new ErrorResponse("invalid_request", "The request is invalid. A parameter is missing, addd more than once or invalid");
        public static ErrorResponse AccessDenied = new ErrorResponse("access_denied", "The user denied the request");
        public static ErrorResponse UnauthorizedClient = new ErrorResponse("unauthorized_client", "The client is not allowed to request an authorization code using this method");
        public static ErrorResponse UnsupportedResponseType = new ErrorResponse("unsupported_response_type", "The server does not support obtaining an authorization code using this method");
        public static ErrorResponse InvalidScope = new ErrorResponse("invalid_scope", "The requested scope is invalid or unknown or malformed");
        public static ErrorResponse ServerError = new ErrorResponse("server_error", "Instead of displaying a 500 Internal Server Error page to the user, the server can redirect with this error code");
        public static ErrorResponse TemporarilyUnavailable = new ErrorResponse("temporarily_unavailable", "The server is undergoing maintenance, or is otherwise unavailable");

    }
}
