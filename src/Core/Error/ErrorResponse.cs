using System;

namespace Core.Error
{
    public class ErrorResponse(string code, string description, string uri = "") : IEquatable<ErrorResponse>
    {
        public static ErrorResponse None = new(string.Empty, string.Empty);
        public string Code { get;} = code;
        public string Description { get; } = description;
        public string Uri { get; } = uri;

        public bool Equals(ErrorResponse? other) => Code == other?.Code && Description == other.Description;
    
        public static ErrorResponse InvalidRequest = new ErrorResponse(OAuthErrors.Code.InvalidRequest, "The request is invalid. A parameter is missing, addd more than once or invalid");
        public static ErrorResponse AccessDenied = new ErrorResponse(OAuthErrors.Code.AccessDenied, "The user denied the request");
        public static ErrorResponse UnauthorizedClient = new ErrorResponse(OAuthErrors.Code.UnauthorizedClient, "The client is not allowed to request an authorization code using this method");
        public static ErrorResponse UnsupportedResponseType = new ErrorResponse(OAuthErrors.Code.UnsupportedResponseType, "The server does not support obtaining an authorization code using this method");
        public static ErrorResponse InvalidScope = new ErrorResponse(OAuthErrors.Code.InvalidScope, "The requested scope is invalid or unknown or malformed");
        public static ErrorResponse ServerError = new ErrorResponse(OAuthErrors.Code.ServerError, "Instead of displaying a 500 Internal Server Error page to the user, the server can redirect with this error code");
        public static ErrorResponse TemporarilyUnavailable = new ErrorResponse(OAuthErrors.Code.TemporarilyUnavailable, "The server is undergoing maintenance, or is otherwise unavailable");

    }
}
