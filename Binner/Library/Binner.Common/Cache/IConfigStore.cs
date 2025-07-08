namespace Binner.Common.Cache
{
    public interface IConfigStore
    {
        void AddOrUpdate<TConfig>(TConfig value) where TConfig : class;
        TConfig FirstOrDefault<TConfig>() where TConfig : class;
    }
}
