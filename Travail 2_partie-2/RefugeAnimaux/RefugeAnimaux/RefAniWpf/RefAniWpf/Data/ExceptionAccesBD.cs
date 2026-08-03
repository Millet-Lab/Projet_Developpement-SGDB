using System;

namespace RefAniWpf.Data
{
    public class ExceptionAccesBD : Exception
    {
        public string Details { get; }

        public ExceptionAccesBD(string message, string details) : base(message)
        {
            Details = details;
        }
    }
}