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
    }
}
