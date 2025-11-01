using Mapster;
using System;
using System.Collections.Generic;

namespace VectorStinger.Foundation.Utilities.Mapper
{
    /// <summary>
    /// Utilidad de mapeo basada en Mapster para convertir entre tipos de objetos.
    /// </summary>
    public static class MapUtil
    {
        /// <summary>
        /// Mapea un objeto de tipo TSource a un nuevo objeto de tipo TDestination.
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(TSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Adapt<TDestination>();
        }

        /// <summary>
        /// Mapea una lista de objetos de tipo TSource a una lista de objetos de tipo TDestination.
        /// </summary>
        public static List<TDestination> MapToList<TSource, TDestination>(IEnumerable<TSource> sourceList)
        {
            if (sourceList == null) throw new ArgumentNullException(nameof(sourceList));
            return sourceList.Adapt<List<TDestination>>();
        }

        /// <summary>
        /// Mapea un objeto de tipo TSource a una instancia existente de tipo TDestination.
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            return source.Adapt(destination);
        }
    }
}
