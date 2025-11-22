using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using VectorStinger.Core.Domain.DataBase.Models;

namespace VectorStinger.Core.Interfaces.Managers.WebTemplate
{
    public interface IFakeDomainManager
    {
        /// <summary>
        /// Crea un nuevo dominio fake
        /// </summary>
        Task<Result<FakeDomain>> CreateFakeDomainAsync(
            int userId,
            int templateId,
            string subportalName,
            string baseDomain);

        /// <summary>
        /// Verifica si un dominio ya existe
        /// </summary>
        Task<bool> CheckDomainExistsAsync(string domainName);

        /// <summary>
        /// Obtiene un dominio fake por ID
        /// </summary>
        Task<Result<FakeDomain>> GetFakeDomainByIdAsync(int domainId);

        /// <summary>
        /// Actualiza la disponibilidad de un dominio
        /// </summary>
        Task<Result> UpdateDomainAvailabilityAsync(int domainId, bool isAvailable);

        /// <summary>
        /// Genera un nombre de subportal válido
        /// </summary>
        string SanitizeSubportalName(string subportalName);
    }
}
