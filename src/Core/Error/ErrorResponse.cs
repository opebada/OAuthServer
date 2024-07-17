using System;
using System.Reflection.Metadata.Ecma335;

namespace Core.Error
{
    public class ErrorResponse(string error, string errorDescription, string errorUri = "") : IEquatable<ErrorResponse>
    {
        public string Error { get;} = error;
        public string ErrorDescription { get; } = errorDescription;
        public string ErrorUri { get; } = errorUri;

        public bool Equals(ErrorResponse? other) => (Error == other?.Error && ErrorDescription == other.ErrorDescription) ? true : false;
    }
}
