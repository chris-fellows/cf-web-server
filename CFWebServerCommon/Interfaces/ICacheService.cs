namespace CFWebServer.Interfaces
{
    /// <summary>
    /// Cache service
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets item by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetItem<T>(string key);

        /// <summary>
        /// Returns all item keys
        /// </summary>
        /// <param name="startsWith">Starts with (Optional)</param>
        /// <returns></returns>
        List<string> GetKeys(string? startsWith);

        /// <summary>
        /// Adds item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="expiry"></param>
        void AddItem<T>(string key, T item, TimeSpan expiry);

        /// <summary>
        /// Removes item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        void RemoveItem<T>(string key);

        /// <summary>
        /// Updates item. If expiry not set then uses existing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="expiry">Expiry (Optional)</param>
        void UpdateItem<T>(string key, T item, TimeSpan? expiry);
    }
}
