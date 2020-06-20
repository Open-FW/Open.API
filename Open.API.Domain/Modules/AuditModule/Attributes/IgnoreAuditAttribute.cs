using System;

namespace Open.API.Domain.Modules.AuditModule.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreAuditAttribute : Attribute
    {
    }
}
