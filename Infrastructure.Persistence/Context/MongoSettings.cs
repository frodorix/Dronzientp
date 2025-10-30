namespace Infrastructure.Persistence.Context
{
    /// <summary>
    /// Configuration settings for MongoDB connection.
    /// </summary>
    public class MongoSettings
    {
        /// <summary>
        /// Gets or sets the MongoDB connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the MongoDB database name.
        /// </summary>
        public string Database { get; set; } = string.Empty;
    }
}
