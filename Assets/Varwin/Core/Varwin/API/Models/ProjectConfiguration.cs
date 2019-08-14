using System;
using Varwin.Data;

namespace Varwin.WWW.Models
{
    public class ProjectConfiguration : IJsonSerializable
    {
        public int Id { get; set; }
        public string Sid { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public int StartSceneId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}