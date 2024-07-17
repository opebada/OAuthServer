using Application.Authorization;
using Core.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Parameters
{
    public class ParameterValidationResult (bool isSuccess, ErrorResponse? errorResponse = null)
    {
        public bool IsSuccess { get; } = isSuccess;
        public ErrorResponse? ErrorResponse { get; } = errorResponse;
    }
}
