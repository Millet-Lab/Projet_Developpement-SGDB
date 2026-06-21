using System;

class ExceptionAccesBD (string message, string details) : Exception(message)
{
    public string Details { get; } = details;
}