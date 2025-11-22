using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.DataBase.Querys.StoredProceduresResults
{
    /// <summary>
    /// Resul of executing the stored procedure to get questions [dbo].[Proc_GetQuestions].
    /// </summary>
    public class GetQuestionsResult
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}
