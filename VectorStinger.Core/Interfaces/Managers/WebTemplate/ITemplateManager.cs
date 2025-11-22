using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using VectorStinger.Core.Domain.DataBase.Models;

namespace VectorStinger.Core.Interfaces.Managers.WebTemplate
{
    public interface ITemplateManager
    {
        /// <summary>
        /// Crea un nuevo template en la base de datos
        /// </summary>
        Task<Result<Template>> CreateTemplateAsync(
            int userId,
            string templateFileUrl,
            string templateFileKey,
            bool isPublished);

        /// <summary>
        /// Obtiene un template por ID
        /// </summary>
        Task<Result<Template>> GetTemplateByIdAsync(int templateId);

        /// <summary>
        /// Actualiza el estado de publicación de un template
        /// </summary>
        Task<Result> UpdatePublishStatusAsync(int templateId, bool isActive);
    }
}
