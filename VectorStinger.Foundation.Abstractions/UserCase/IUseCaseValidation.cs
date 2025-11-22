using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Abstractions.UserCase
{
    public interface IUseCaseValidation
    {
    }
    public abstract class UseCaseValidation<T> : AbstractValidator<T>, IUseCaseValidation
        where T : IUseCaseInput
    {

    }
}
