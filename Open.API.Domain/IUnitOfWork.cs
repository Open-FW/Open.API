using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Open.API.Domain.Modules.AuditModule;

namespace Open.API.Domain
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync([NotNull] string actionBy, CancellationToken cancellationToken);

        DbSet<Audit> Audits { get; set; }
    }
}
