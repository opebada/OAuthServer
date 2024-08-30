using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Error
{
    public class OAuthErrors
    {
        public class Code
        {
            public const string InvalidRequest = "invalid_request";
            public const string AccessDenied = "access_denied";
            public const string UnauthorizedClient = "unauthorized_client";
            public const string UnsupportedResponseType = "unsupported_response_type";
            public const string InvalidScope = "invalid_scope";
            public const string ServerError = "server_error";
            public const string TemporarilyUnavailable = "temporarily_unavailable";
        }
        
        public class Descriptions
        {
            public const string InvalidRequest = "The request is invalid. A parameter is missing, addd more than once or invalid";
            public const string AccessDenied = "The user denied the request";
            public const string UnauthorizedClient = "The client is not allowed to request an authorization code using this method";
            public const string UnsupportedResponseType = "The server does not support obtaining an authorization code using this method";
            public const string InvalidScope = "The requested scope is invalid or unknown or malformed";
            public const string ServerError = "Instead of displaying a 500 Internal Server Error page to the user, the server can redirect with this error code";
            public const string TemporarilyUnavailable = "The server is undergoing maintenance, or is otherwise unavailable";
        }
    }
}
