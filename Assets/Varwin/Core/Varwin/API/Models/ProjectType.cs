using System;
using Varwin.Data;

namespace Varwin.WWW.Models
{
    public class ProjectType : IJsonSerializable
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsPc => Id == 1;
        public bool IsMobile => Id == 2;
    }
}