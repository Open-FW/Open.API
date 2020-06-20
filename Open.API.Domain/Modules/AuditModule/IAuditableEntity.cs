using System;

namespace Open.API.Domain.Modules.AuditModule
{
    public interface IAuditableEntity
    {
        DateTime? Edited { get; set; }
        string? EditedBy { get; set; }

        DateTime Created { get; set; }
        string CreatedBy { get; set; }
    }
}
