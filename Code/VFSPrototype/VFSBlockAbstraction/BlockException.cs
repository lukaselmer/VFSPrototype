using System;
using System.Runtime.Serialization;

namespace VFSBlockAbstraction
{
    [Serializable]
    public class BlockException : Exception
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable MemberCanBeProtected.Global
        // See: http://se.inf.ethz.ch/courses/2013a_spring/JavaCSharp/code_analysis_tools.html
        // and http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA1032);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true

        public BlockException() { }
        public BlockException(string message) : base(message) { }
        public BlockException(string message, Exception innerException) : base(message, innerException) { }
        protected BlockException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore UnusedMember.Global
    }
}