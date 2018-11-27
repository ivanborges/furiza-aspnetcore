using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class InvalidHiringTypeException : CoreException
    {
        public override string Key => "InvalidHiringType";
        public override string Message => "Invalid hiring type.";

        public InvalidHiringTypeException() : base()
        {
        }

        protected InvalidHiringTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}