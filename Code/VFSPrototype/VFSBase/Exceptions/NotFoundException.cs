using System;
using System.Runtime.Serialization;

namespace VFSBase.Exceptions
{
    [Serializable]
    public class NotFoundException : VFSException
    {
        // ReSharper disable UnusedMember.Global
        // See: http://se.inf.ethz.ch/courses/2013a_spring/JavaCSharp/code_analysis_tools.html
        // and http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(CA1032);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.5);k(DevLang-csharp)&rd=true

        public NotFoundException() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
        protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        // ReSharper restore UnusedMember.Global
    }
}