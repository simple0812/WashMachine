using System;
using WashMachine.Enums;

namespace WashMachine.Models
{
    public sealed class CustomException : Exception
    {
        public string ModuleName { get; set; }
        public ExceptionPriority Priority { get; set; }
        public string Trace { get; set; }

        public string BaseException => GetBaseException().GetType().FullName;

        public CustomException(string msg, string moduleName = "", ExceptionPriority priority = ExceptionPriority.Normal) : base(msg)
        {
            this.ModuleName = moduleName;
            this.Priority = priority;
            this.Trace = StackTrace;
        }
    }
}
