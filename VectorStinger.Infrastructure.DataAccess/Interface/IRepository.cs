using VectorStinger.Infrastructure.DataAccess.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace VectorStinger.Infrastructure.DataAccess.Interface
{
    public interface IRepository
    {
        Task<bool> SaveObjectAsync<T>(Entity<T> entity, CancellationToken cancellationToken = default) where T : class, new();
        Task<bool> CallProcedureAsync<T>(string nameProcedure, params object[] parameters) where T : class, new();
        Task<List<T>> GetDataByProcedureAsync<T>(string nameProcedure, params object[] parameters) where T : class, new();
        Task<List<T>> SimpleSelectAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, new();
        Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class, new();
        Task<T> GetByIdAsync<T>(params object[] keyValues) where T : class, new();
        Task<bool> CommitAsync(CancellationToken cancellationToken = default);
        Task<bool> RollbackAsync(CancellationToken cancellationToken = default);
    }
}
