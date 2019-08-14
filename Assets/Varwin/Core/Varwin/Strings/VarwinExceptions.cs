using System;

namespace Varwin
{
    /// <summary>
    /// Exception for the case of calling a part of code that uses .NET 4.x instead of .NET Standard 2.0.
    /// </summary>
    public class WrongApiCompatibilityLevelException : Exception
    {
        public WrongApiCompatibilityLevelException() : base(ErrorMessages.DoNotUseNetStandart20)
        {
        }
    }
}