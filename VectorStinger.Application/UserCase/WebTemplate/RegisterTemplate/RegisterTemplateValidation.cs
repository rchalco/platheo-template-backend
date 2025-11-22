using FluentValidation;
using VectorSinger.Modules.WebTemplate.UseCases.RegisterTemplate;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    public class RegisterTemplateValidation : UseCaseValidation<RegisterTemplateInput>
    {
        public RegisterTemplateValidation()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("El UserId debe ser mayor a 0");

        }
    }
}
