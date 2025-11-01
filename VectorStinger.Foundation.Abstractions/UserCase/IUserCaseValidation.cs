using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Abstractions.UserCase
{
    public interface IUserCaseValidation
    {
    }
    public abstract class UserCaseValidation<T> : AbstractValidator<T>, IUserCaseValidation
        where T : IUseCaseInput
    {

    }
}
