namespace Varwin.Errors
{
    public static class ErrorCode
    {
        public const int ServerNoConnectionError = 101;
        public const int LicenseKeyError = 102;
        public const int RabbitNoArgsError = 103;
        public const int LocationSaveError = 104;
        public const int RabbitCannotReadArgsError = 105;

        public const int PhotonServerDisconnectError = 201;
        
        public const int LogicExecuteError = 301;
        public const int LogicInitError = 302;

        public const int CompileCodeError = 401;
        public const int RuntimeCodeError = 402;
        
        public const int ReadStartupArgsError = 501;
        public const int LoadObjectError = 502;
        public const int LoadSceneError = 503;
        public const int SpawnPointNotFoundError = 504;
        public const int LoadWorldConfigError = 505;
        public const int EnvironmentNotFoundError = 506;
        public const int ProjectConfigNullError = 507;

        public const int ExceptionInObject = 601;
        public const int CannotPreview = 602;
        public const int CannotDeleteObjectLogic = 603;
        public const int NotForCommercialUse = 604;
        
        public const int UnknownError = 900;
    }
}