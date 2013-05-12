using System;
using System.Runtime.Serialization;

namespace VFSWCFService.Persistence
{
    [Serializable]
    public class PersistenceException : Exception
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable MemberCanBeProtected.Global
        // See: http://se.inf.ethz.ch/courses/2013a_spring/JavaCSharp/code_analysis_tools.html
        // and http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA1032);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true

        public PersistenceException() { }
        public PersistenceException(string message) : base(message) { }
        public PersistenceException(string message, Exception innerException) : base(message, innerException) { }
        protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore UnusedMember.Global
    }
}