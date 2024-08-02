using Core.Error;
using System;

namespace Core.Authorization.ParameterValidation
{
    public class AuthorizationParameterValidationResult
    {
        public AuthorizationParameterValidationResult(bool isSuccess, ErrorResponse? errorResponse = null)
        {
            if (isSuccess && errorResponse != ErrorResponse.None || !isSuccess && errorResponse == ErrorResponse.None)
                throw new ArgumentException("Invalid error response", nameof(errorResponse));

            IsSuccess = isSuccess;
            ErrorResponse = errorResponse;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public ErrorResponse? ErrorResponse { get; }

        public static AuthorizationParameterValidationResult Success() => new(true, ErrorResponse.None);
        public static AuthorizationParameterValidationResult Failure(ErrorResponse errorResponse) => new(false, errorResponse);
        public static implicit operator AuthorizationParameterValidationResult(ErrorResponse errorResponse) => Failure(errorResponse);
    }
}
