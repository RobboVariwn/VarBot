namespace Varwin.Data
{
    public static class ApiRoutes
    {
        public const string ProjectStructureRequest = "/v1/project-structure/{0}";
        public const string SaveSceneObjectsRequest = "/v1/update-scene-objects";
        public const string GetLicenseRequest = "/v1/get-app-settings";
        public const string CanDeleteObjectRequest = "/v1/can-delete-scene-object/{0}";
        public const string ServerConfig = "/v1/server-config";
        
        public const string Projects = "/v1/projects";
        public const string MobileProjects = "/v1/projects?mobile_ready=true";
        
        public const string Objects = "/v1/objects-library?offset={0}&limit={1}&search={2}";
        public const string MobileObjects = Objects + "&mobile_ready=true";
    }
}