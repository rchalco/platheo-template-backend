using FluentResults;
using VectorStinger.Core.Domain.DataBase.DataContext;
using VectorStinger.Core.Domain.DataBase.Models;
using VectorStinger.Core.Domain.DataBase.Querys.StoredProceduresResults;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterTemplateWebHandle;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;

namespace VectorSinger.Modules.WebTemplate.Managers
{
    public class WebTemplateManager : BaseManager<BdPlatheoTemplateContext>, IWebTemplateManager
    {

        public WebTemplateManager(IRepository repository)
            : base(repository)
        {

        }

        public Result<ResoponseRegisterTemplateWebDTO> RegisterTemplateWebHandle(RequestRegisterTemplateWebDTO request)
        {
            throw new NotImplementedException();
        }

        public Result<List<ResponseGetQuestionsQuizDTO>> GetQuestionsQuizHandle()
        {
            var result = _repository.GetDataByProcedure<GetQuestionsResult>("[dbo].[Proc_GetQuestions]");

            if (result?.Count == 0)
            {
                return Result.Fail("Proc_GetQuestions: data not found...");
            }

            var responseGetQuestionsQuizDTOs = result!
                .GroupBy(x => x.QuestionId)
                .Select(g =>
                {
                    var first = g.First();
                    return new ResponseGetQuestionsQuizDTO
                    {
                        QuestionId = g.Key,
                        QuestionText = first.QuestionText,
                        QuestionType = first.QuestionType,
                        IsClosed = first.IsClosed ? 1 : 0,
                        IsActive = first.IsActive ? 1 : 0,
                        Options = g.Select(o => new QuestionOptionDTO
                        {
                            OptionId = o.OptionId,
                            OptionText = o.OptionText
                        }).ToList()
                    };
                })
                .ToList();

            return Result.Ok(responseGetQuestionsQuizDTOs);
        }

        public Result<ResponseRegisterQuestionsQuizDTO> RegisterQuestionsQuizHandle(RequestRegisterQuestionsQuizDTO request)
        {
            // Validar que el usuario existe
            var userExists = _repository.SimpleSelect<User>(x => x.UserId == request.UserId);
            if (userExists == null || userExists.Count == 0)
            {
                return Result.Fail<ResponseRegisterQuestionsQuizDTO>("Usuario no encontrado");
            }

            // Validar que hay respuestas para registrar
            if (request.Answers == null || request.Answers.Count == 0)
            {
                return Result.Fail<ResponseRegisterQuestionsQuizDTO>("No hay respuestas para registrar");
            }

            int answersRegistered = 0;
            var validationErrors = new List<string>();

            // Registrar cada respuesta
            foreach (var answer in request.Answers)
            {
                // Validar que la pregunta existe
                var questionExists = _repository.SimpleSelect<Question>(x => x.QuestionId == answer.QuestionId);
                if (questionExists == null || questionExists.Count == 0)
                {
                    validationErrors.Add($"Pregunta con ID {answer.QuestionId} no encontrada");
                    continue;
                }

                var question = questionExists.First();

                // Validar según el tipo de pregunta
                if (answer.QuestionType.Equals("open", StringComparison.OrdinalIgnoreCase))
                {
                    // Para preguntas abiertas, debe tener AnswerText
                    if (string.IsNullOrWhiteSpace(answer.AnswerText))
                    {
                        validationErrors.Add($"La pregunta abierta {answer.QuestionId} requiere un texto de respuesta");
                        continue;
                    }

                    // Crear la respuesta del usuario para pregunta abierta
                    var userAnswer = new UserAnswer
                    {
                        UserId = request.UserId,
                        QuestionId = answer.QuestionId,
                        AnswerText = answer.AnswerText,
                        OptionId = null, // No aplica para preguntas abiertas
                        AnsweredAt = DateTime.Now,
                        AnswerId = 0
                    };

                    _repository.SaveObject(new Entity<UserAnswer>
                    {
                        EntityDB = userAnswer,
                        stateEntity = StateEntity.add
                    });

                    answersRegistered++;
                }
                else if (answer.QuestionType.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    // Para preguntas cerradas, debe tener OptionId
                    if (!answer.OptionId.HasValue)
                    {
                        validationErrors.Add($"La pregunta cerrada {answer.QuestionId} requiere un OptionId");
                        continue;
                    }

                    // Validar que la opción existe y pertenece a la pregunta
                    var optionExists = _repository.SimpleSelect<QuestionOption>(x => 
                        x.OptionId == answer.OptionId.Value && 
                        x.QuestionId == answer.QuestionId);
                    
                    if (optionExists == null || optionExists.Count == 0)
                    {
                        validationErrors.Add($"La opción {answer.OptionId.Value} no existe o no pertenece a la pregunta {answer.QuestionId}");
                        continue;
                    }

                    // Crear la respuesta del usuario para pregunta cerrada
                    var userAnswer = new UserAnswer
                    {
                        UserId = request.UserId,
                        QuestionId = answer.QuestionId,
                        AnswerText = null, // No aplica para preguntas cerradas
                        OptionId = answer.OptionId.Value,
                        AnsweredAt = DateTime.Now,
                        AnswerId = 0
                    };

                    _repository.SaveObject(new Entity<UserAnswer>
                    {
                        EntityDB = userAnswer,
                        stateEntity = StateEntity.add
                    });

                    answersRegistered++;
                }
                else
                {
                    validationErrors.Add($"Tipo de pregunta '{answer.QuestionType}' no válido para pregunta {answer.QuestionId}. Debe ser 'open' o 'close'");
                }
            }

            // Validar que se registró al menos una respuesta
            if (answersRegistered == 0)
            {
                var errorMessage = validationErrors.Count > 0 
                    ? $"No se pudo registrar ninguna respuesta válida. Errores: {string.Join("; ", validationErrors)}"
                    : "No se pudo registrar ninguna respuesta válida";
                
                return Result.Fail<ResponseRegisterQuestionsQuizDTO>(errorMessage);
            }

            var message = answersRegistered == request.Answers.Count
                ? $"Se registraron {answersRegistered} respuestas correctamente"
                : $"Se registraron {answersRegistered} de {request.Answers.Count} respuestas. Errores: {string.Join("; ", validationErrors)}";

            var response = new ResponseRegisterQuestionsQuizDTO
            {
                IsSuccess = true,
                Message = message,
                TotalAnswersRegistered = answersRegistered
            };

            return Result.Ok(response);
        }
    }
}
