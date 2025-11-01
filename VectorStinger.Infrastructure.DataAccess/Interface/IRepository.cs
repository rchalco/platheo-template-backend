using VectorStinger.Infrastructure.DataAccess.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VectorStinger.Infrastructure.DataAccess.Interface
{
    public interface IRepository
    {
        bool SaveObject<T>(Entity<T> entity) where T : class, new();
        bool CallProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new();
        List<T> GetDataByProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new();
        List<T> SimpleSelect<T>(Expression<Func<T, bool>> predicate) where T : class, new();
        List<T> GetAll<T>() where T : class, new();
        bool Commit();
        bool Rollback();

    }
}
