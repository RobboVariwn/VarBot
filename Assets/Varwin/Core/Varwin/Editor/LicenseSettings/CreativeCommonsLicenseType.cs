namespace Varwin.Editor
{
    public class CreativeCommonsLicenseType : LicenseType
    {
        private static readonly string[] AvailableVersions = {"4.0", "3.0", "2.5", "2.0", "1.0"};
        
        public CreativeCommonsLicenseType(string code, string name) : base(code, name, AvailableVersions, $"{LicenseSettings.CreativeCommonsLink}{code.Replace("cc-", "")}/{{0}}/")
        {
        }
    }
}