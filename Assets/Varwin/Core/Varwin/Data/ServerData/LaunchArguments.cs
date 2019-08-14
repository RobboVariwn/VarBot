namespace Varwin.Data.ServerData
{
    public class Api
    {
        public string baseUrl { get; set; }
    }

    public class Web
    {
        public string baseUrl { get; set; }
    }

    public class Rabbitmq : IJsonSerializable
    {
        public string stompUrl { get; set; }
        public string host { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public string key { get; set; }
    }

    public class Photon
    {
        public string host { get; set; }
    }

    public class LaunchArguments : IJsonSerializable
    {
        public int gm { get; set; }
        public int platformMode { get; set; }
        public string lang { get; set; }
        public int sceneId { get; set; }
        public int projectId { get; set; }
        public int projectConfigurationId { get; set; }

        public Api api { get; set; }
        public Api web { get; set; }
        public Rabbitmq rabbitmq { get; set; }
        public Photon photon { get; set; }
        public bool onboarding { get; set; }
        public string extraArgs { get; set; }
    }
}
