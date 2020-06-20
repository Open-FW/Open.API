using System;

namespace Open.API.Domain.Modules.SoftDeleteModule
{
    public interface ISoftDeleteEntity
    {
        bool IsDeleted { get; set; }

        DateTime? Deleted { get; set; }
        string? DeletedBy { get; set; }
    }
}
