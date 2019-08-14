using System;
using System.Collections.Generic;
using Varwin.Data;

namespace Varwin.WWW.Models
{
    public class ProjectItem : IJsonSerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SceneCount { get; set; }
        public List<ProjectConfiguration> Configurations { get; set; }
        public ProjectType ProjectType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsPc => ProjectType?.IsPc ?? false;
        public bool IsMobile => ProjectType?.IsMobile ?? false;
    }
}