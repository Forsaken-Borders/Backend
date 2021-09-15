using System;
using System.ComponentModel.DataAnnotations;

namespace ForSakenBorders.Backend.Database
{
    public class Log
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Token { get; set; }
        public string Endpoint { get; set; }
        public string UserAgent { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

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