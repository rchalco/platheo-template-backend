using FluentValidation;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz
{
    public class GetQuestionsQuizValidation : UseCaseValidation<GetQuestionsQuizInput>
    {
        public GetQuestionsQuizValidation()
        {
            // No hay validaciones específicas ya que no recibe parámetros
            // Pero la clase es necesaria para la arquitectura del caso de uso
        }
    }
}
