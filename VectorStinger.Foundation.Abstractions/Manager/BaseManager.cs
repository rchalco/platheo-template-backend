using VectorStinger.Infrastructure.DataAccess.Interface;
using Microsoft.EntityFrameworkCore;
using VectorStinger.Foundation.Utilities.Exceptions;
using VectorStinger.Foundation.Utilities.Wrapper;
using System;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Abstractions.Manager
{
    public interface IManager
    {
        Task<string> ProcessErrorAsync(Exception ex);
        Task<string> ProcessErrorAsync(Exception ex, Response response);
    }
    public abstract class BaseManager<T> : IManager where T : DbContext
    {
        protected readonly IRepository _repository;
        public BaseManager(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> ProcessErrorAsync(Exception ex)
        {
            ManagerException managerException = new ManagerException();
            await (_repository?.RollbackAsync() ?? Task.CompletedTask);
            return managerException.ProcessException(ex);
        }

        public async Task<string> ProcessErrorAsync(Exception ex, Response response)
        {
            ManagerException managerException = new ManagerException();
            response.State = ResponseType.Error;
            response.Message = managerException.ProcessException(ex);
            await (_repository?.RollbackAsync() ?? Task.CompletedTask);
            return response.Message;
        }
    }
}
