using System;
using System.ComponentModel.DataAnnotations;

namespace ForsakenBorders.Backend.Models
{
    /// <summary>
    /// Contains general non-identifiable information about an http request.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// The generated id of the log.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// If authorization token, if any. Otherwise <see cref="Guid.Empty"/>.
        /// </summary>
        public Guid Token { get; set; } = Guid.Empty;

        /// <summary>
        /// Which endpoint was accessed.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Which User Agent was used, if any.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// When the endpoint was accessed.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// If the request was not successful, this will contain the exception.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Empty constructor required by EFCore.
        /// </summary>
        public Log() { }

        /// <summary>
        /// Creates a new log entry.
        /// </summary>
        /// <param name="token">If authorization token, if any.</param>
        /// <param name="endpoint">Which endpoint was accessed.</param>
        /// <param name="userAgent">Which User Agent was used, if any.</param>
        public Log(Guid token, string endpoint, string userAgent)
        {
            Id = Guid.NewGuid();
            Token = token;
            Endpoint = endpoint;
            UserAgent = userAgent;
        }
    }
}