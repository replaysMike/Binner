using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Binner.Services.MappingProfiles
{
    public class BaseCollectionMapperAction<TSource, TDestination> : IMappingAction<TSource, TDestination>
    {
        /// <summary>
        /// Map a sub-collection without replacing proxied collections
        /// </summary>
        /// <typeparam name="TCollectionSource"></typeparam>
        /// <typeparam name="TCollectionDestination"></typeparam>
        /// <param name="sourceCollection"></param>
        /// <param name="destCollection"></param>
        /// <param name="predicate"></param>
        /// <param name="context"></param>
        public void MapCollection<TCollectionSource, TCollectionDestination>(IEnumerable<TCollectionSource> sourceCollection, IEnumerable<TCollectionDestination> destCollection, Func<TCollectionSource, TCollectionDestination, bool> predicate, ResolutionContext context)
        {
            MapCollection(sourceCollection.ToList(), destCollection.ToList(), predicate, context);
        }

        /// <summary>
        /// Map a sub-collection without replacing proxied collections
        /// </summary>
        /// <typeparam name="TCollectionSource"></typeparam>
        /// <typeparam name="TCollectionDestination"></typeparam>
        /// <param name="sourceCollection"></param>
        /// <param name="destCollection"></param>
        /// <param name="context"></param>
        public void MapCollection<TCollectionSource, TCollectionDestination>(IEnumerable<TCollectionSource> sourceCollection, IEnumerable<TCollectionDestination> destCollection, ResolutionContext context)
        {
            MapCollection(sourceCollection.ToList(), destCollection.ToList(), context);
        }

        /// <summary>
        /// Map a sub-collection without replacing proxied collections
        /// </summary>
        /// <typeparam name="TCollectionSource"></typeparam>
        /// <typeparam name="TCollectionDestination"></typeparam>
        /// <param name="sourceList"></param>
        /// <param name="destList"></param>
        /// <param name="predicate"></param>
        /// <param name="context"></param>
        public void MapCollection<TCollectionSource, TCollectionDestination>(IList<TCollectionSource> sourceList, IList<TCollectionDestination> destList, Func<TCollectionSource, TCollectionDestination, bool> predicate, ResolutionContext context)
        {
            for (var sourceIndex = 0; sourceIndex < sourceList.Count; sourceIndex++)
            {
                for (var destIndex = 0; sourceIndex < destList.Count; destIndex++)
                {
                    var result = predicate(sourceList[sourceIndex], destList[destIndex]);
                    if (result)
                    {
                        destList[destIndex] = context.Mapper.Map(sourceList[sourceIndex], destList[destIndex]);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Map a sub-collection without replacing proxied collections
        /// </summary>
        /// <typeparam name="TCollectionSource"></typeparam>
        /// <typeparam name="TCollectionDestination"></typeparam>
        /// <param name="sourceList"></param>
        /// <param name="destList"></param>
        /// <param name="context"></param>
        public void MapCollection<TCollectionSource, TCollectionDestination>(IList<TCollectionSource> sourceList, IList<TCollectionDestination> destList, ResolutionContext context)
        {
            // assume both lists are the same size and order, since no predicate match was specified
            for (var sourceIndex = 0; sourceIndex < sourceList.Count; sourceIndex++)
            {
                destList[sourceIndex] = context.Mapper.Map(sourceList[sourceIndex], destList[sourceIndex]);
            }
        }

        public virtual void Process(TSource source, TDestination destination, ResolutionContext context)
        {
            throw new NotImplementedException("You must provide a mapping implementation!");
        }
    }
}
