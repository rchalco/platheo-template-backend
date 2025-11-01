using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Application.UserCase.Security.ValidateToken
{
    public class ValidateTokenInput: IUseCaseInput
    {
        public string Token { get; set; } = string.Empty;
    }
}
