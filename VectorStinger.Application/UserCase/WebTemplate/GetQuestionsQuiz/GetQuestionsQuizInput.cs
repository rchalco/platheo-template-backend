using VectorStinger.Foundation.Abstractions.UserCase;
using System.Runtime.Serialization;

namespace VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz
{
    [DataContract]
    public class GetQuestionsQuizInput : IUseCaseInput
    {
        // Este input está vacío pero es necesario para el mapeo automático
        // Se puede agregar filtros o parámetros en el futuro si es necesario
    }
}
