using VectorStinger.Infrastructure.DataAccess.Interface;
using Microsoft.EntityFrameworkCore;
using VectorStinger.Foundation.Utilities.Exceptions;
using VectorStinger.Foundation.Utilities.Wrapper;

namespace VectorStinger.Foundation.Abstractions.Manager
{
    public interface IManager
    {
        string ProcessError(Exception ex);
        string ProcessError(Exception ex, Response response);
    }
    public abstract class BaseManager<T> : IManager where T : DbContext
    {
        protected readonly IRepository _repository;
        public BaseManager(IRepository repository)
        {
            _repository = repository;
        }

        public string ProcessError(Exception ex)
        {
            ManagerException managerException = new ManagerException();
            _repository?.Rollback();
            return managerException.ProcessException(ex);
        }

        public string ProcessError(Exception ex, Response response)
        {
            ManagerException managerException = new ManagerException();
            response.State = ResponseType.Error;
            response.Message = managerException.ProcessException(ex);
            _repository?.Rollback();
            return response.Message;
        }
    }
}
