using System;
using System.Reflection;

namespace Varwin
{
    public static class LogicUtils
    {
        public static void AddEventHandler(this ILogic self, object o, string eventName, string methodName)
        {
            EventInfo eventInfo = o.GetType().GetEvent(eventName);
            
            MethodInfo methodInfo = self.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            
            Delegate delegatedMethod = Delegate.CreateDelegate(eventInfo.EventHandlerType, self, methodInfo);

            eventInfo.AddEventHandler(o, delegatedMethod);
        }
    }
}