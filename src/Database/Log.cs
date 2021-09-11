using System;
using System.ComponentModel.DataAnnotations;

namespace ForSakenBorders.Backend.Database
{
    public class Log
    {
        [Key]
        public Guid Id { get; }
        public Guid Token { get; }
        public string Endpoint { get; }
        public string UserAgent { get; }
        public DateTime DateTime { get; } = DateTime.UtcNow;

        /// <summary>
        /// Empty constructor required by EFCore.
        /// </summary>
        public Log() { }

        public Log(Guid token, string endpoint, string userAgent)
        {
            Id = Guid.NewGuid();
            Token = token;
            Endpoint = endpoint;
            UserAgent = userAgent;
        }
    }
}