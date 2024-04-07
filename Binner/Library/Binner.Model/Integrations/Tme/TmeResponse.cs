namespace Binner.Model.Integrations.Tme
{
    public class TmeResponse<T>
    {
        /// <summary>
        /// Response status. "OK" indicates that the action was successful
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Action response data
        /// </summary>
        public T? Data { get; set; }
    }
}
