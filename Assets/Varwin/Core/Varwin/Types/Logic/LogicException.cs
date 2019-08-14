using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using Varwin.Errors;
using Varwin.UI.VRErrorManager;
using Varwin.WWW;

namespace Varwin.Types
{
    public class LogicException
    {
        public LogicInstance Logic { get; }
        public MethodBase Method { get; }
        public Exception Exception { get; }

        public string MethodName { get; }
        public string DeclaringTypeFullName { get; }

        public string Message { get; }

        private StackFrame _frame;
        public int Line => _frame?.GetFileLineNumber() ?? 0;
        public int Column => _frame?.GetFileColumnNumber() ?? 0;
            
        public LogicException(LogicInstance logic, Exception exception)
        {
            Logic = logic;
            Exception = exception;

            _frame = GetStackTraceFrame(exception);
            
            Method = _frame.GetMethod();
            if (Method != null)
            {
                MethodName = Method.Name;
                if (Method.DeclaringType != null)
                {
                    DeclaringTypeFullName = Method.DeclaringType.FullName;
                }
            }

            if (!string.IsNullOrEmpty(DeclaringTypeFullName) && !DeclaringTypeFullName.Contains("Varwin.LogicOfLocation"))
            {
                string objectType = DeclaringTypeFullName.Split('_')[0];
                Message = $"Object {objectType} has error in method {MethodName}. Exception = {exception.Message}";
            }
            else
            {
                Message = exception.Message;
            }
        }

        public string GetStackFrameString()
        {
            return $"location Id = {Logic.WorldLocationId}; Line: {Line}; Column: {Column}; Message:{Exception.Message}";
        }

        public static StackFrame GetStackTraceFrame(Exception exception)
        {
            var st = new StackTrace(exception, true);
            return st.GetFrame(0);
        }
    }
}