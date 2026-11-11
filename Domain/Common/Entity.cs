namespace Domain.Common
{
    /// <summary>
    /// Entity base class
    /// </summary>
    public abstract class Entity : IEntity<long>
    {
        /// <summary>
        /// Unique identifier to identify specific instance
        /// </summary>
        public long Id { get; set; }
    }
}
