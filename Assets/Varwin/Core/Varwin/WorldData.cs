using System;

namespace Varwin
{
    /// <summary>
    /// Backward compatibility
    /// </summary>
    [Obsolete("This will be removed in the future. Use ProjectData instead")]
    public static class WorldData
    {
        [Obsolete("This will be removed in the future. Use ProjectData.ProjectId instead")]
        public static int WorldId => ProjectData.ProjectId;
        [Obsolete("This will be removed in the future. Use ProjectData.SceneId instead")]
        public static int WorldLocationId => ProjectData.SceneId;
        [Obsolete("This will be removed in the future. Use ProjectData.LocationId instead")]
        public static int LocationId => ProjectData.LocationId;
        
        [Obsolete("This will be removed in the future. Use ProjectData.ProjectConfigurationId instead")]
        public static int WorldConfigurationId => ProjectData.ProjectConfigurationId;
        [Obsolete("This will be removed in the future. Use ProjectData.GameMode instead")]
        public static GameMode GameMode => ProjectData.GameMode;

        
    }
}