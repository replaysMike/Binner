using Binner.Common.StorageProviders;

namespace Binner.Common.IO
{
    /// <summary>
    /// Database builder
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// A database builder
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        T Build(IBinnerDb db);
    }
}
