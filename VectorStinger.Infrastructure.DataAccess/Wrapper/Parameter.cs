using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace VectorStinger.Infrastructure.DataAccess.Wrapper
{
    public interface IParameterValue { }

    public enum ParameterType
    {
        In = 1,
        Out = 2,
        InOut = 3
    }

    public class Parameter<T>
    {
        private T _value;
        private readonly ParameterType _parameterType;
        private readonly int _size;
        public static readonly Dictionary<Type, DbType> _typeMap = new()
        {
            { typeof(int), DbType.Int32 },
            { typeof(long), DbType.Int64 },
            { typeof(string), DbType.String },
            { typeof(bool), DbType.Boolean },
            { typeof(DateTime), DbType.DateTime },
            { typeof(byte[]), DbType.Binary },
            { typeof(Decimal), DbType.Decimal}
        };

        public static readonly Dictionary<ParameterType, ParameterDirection> _parameterDirectionMap = new()
        {
            { ParameterType.In , ParameterDirection.Input },
            { ParameterType.Out , ParameterDirection.Output },
            { ParameterType.InOut , ParameterDirection.InputOutput }
        };

        public Parameter(T value, ParameterType parameterType = ParameterType.Out, int size = 0)
        {
            if (!_typeMap.Keys.Contains(typeof(T)) && !((typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))))
            {
                throw new ArgumentException($"El tipo {typeof(T)} no es compatible.");
            }

            _value = value;
            _parameterType = parameterType;
            _size = size;
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public ParameterType ParameterType => _parameterType;
        public int Size => _size;
        public DbType GetDbType()
        {
            if (_typeMap.TryGetValue(typeof(T), out var dbType))
            {
                return dbType;
            }
            throw new ArgumentException($"El tipo {typeof(T)} no es compatible.");
        }

        public ParameterDirection GetDbParameterDirection()
        {
            if (_parameterDirectionMap.TryGetValue(_parameterType, out var parameterDirection))
            {
                return parameterDirection;
            }
            throw new ArgumentException($"El tipo {_parameterType} no es compatible.");
        }
    }
}


