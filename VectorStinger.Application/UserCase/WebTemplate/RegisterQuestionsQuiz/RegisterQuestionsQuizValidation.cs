using FluentValidation;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    public class RegisterQuestionsQuizValidation : UseCaseValidation<RegisterQuestionsQuizInput>
    {
        public RegisterQuestionsQuizValidation()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("El UserId debe ser mayor a 0");

            RuleFor(x => x.Answers)
                .NotNull()
                .WithMessage("La lista de respuestas no puede ser nula")
                .NotEmpty()
                .WithMessage("Debe proporcionar al menos una respuesta");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .GreaterThan(0)
                    .WithMessage("El QuestionId debe ser mayor a 0");

                answer.RuleFor(a => a.QuestionType)
                    .NotEmpty()
                    .WithMessage("El QuestionType es obligatorio")
                    .Must(type => type.Equals("open", StringComparison.OrdinalIgnoreCase) || 
                                  type.Equals("close", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("El QuestionType debe ser 'open' o 'close'");

                // Validar que preguntas "open" tengan AnswerText
                answer.RuleFor(a => a.AnswerText)
                    .NotEmpty()
                    .When(a => a.QuestionType.Equals("open", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Las preguntas abiertas requieren un texto de respuesta (AnswerText)");

                // Validar que preguntas "close" tengan OptionId
                answer.RuleFor(a => a.OptionId)
                    .NotNull()
                    .GreaterThan(0)
                    .When(a => a.QuestionType.Equals("close", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Las preguntas cerradas requieren un OptionId válido");
            });
        }
    }
}
