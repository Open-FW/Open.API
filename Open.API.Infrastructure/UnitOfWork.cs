using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Open.API.Domain;
using Open.API.Domain.Modules.AuditModule;
using Open.API.Domain.Modules.AuditModule.Attributes;
using Open.API.Domain.Modules.SoftDeleteModule;
using Open.API.Infrastructure.Extensions;

namespace Open.API.Infrastructure
{
    public class UnitOfWork : DbContext, IUnitOfWork
    {
        public DbSet<Audit> Audits { get; set; } = null!;

        public UnitOfWork([NotNull] DbContextOptions<UnitOfWork> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);

                if (entityType.ClrType is ISoftDeleteEntity)
                {
                    modelBuilder.Entity(entityType.ClrType).AddQueryFilter<ISoftDeleteEntity>(f => !f.IsDeleted);
                }
            }
        }

        public async Task<int> SaveChangesAsync([NotNull] string actionBy, CancellationToken cancellationToken)
        {
            var auditEntries = this.AuditBeforeSaveChanges(actionBy);

            var result = await this.SaveChangesAsync(cancellationToken);

            await this.AuditAfterSaveChanges(auditEntries, cancellationToken);

            return result;
        }

        private IEnumerable<AuditEntry> AuditBeforeSaveChanges([NotNull] string actionBy)
        {
            ChangeTracker.DetectChanges();

            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                    continue;

                if (entry.Entity is IAuditableEntity auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditableEntity.Created = DateTime.Now;
                            auditableEntity.CreatedBy = actionBy;
                            break;
                        case EntityState.Modified:
                            auditableEntity.Edited = DateTime.Now;
                            auditableEntity.EditedBy = actionBy;
                            break;
                    }
                }

                var tmpSoftDelete = false;
                if (entry.Entity is ISoftDeleteEntity softDeleteEntity)
                {
                    if (entry.State == EntityState.Deleted)
                    {
                        tmpSoftDelete = true;

                        softDeleteEntity.IsDeleted = true;
                        softDeleteEntity.Deleted = DateTime.Now;
                        softDeleteEntity.DeletedBy = actionBy;

                        entry.State = EntityState.Modified;
                    }
                }

                if (entry.Entity.GetType().GetCustomAttribute<IgnoreAuditAttribute>() == null)
                {
                    var auditEntry = new AuditEntry(entry)
                    {
                        ActionBy = actionBy
                    };

                    auditEntries.Add(auditEntry);

                    foreach (var property in entry.Properties)
                    {

                        if (property.IsTemporary)
                        {
                            auditEntry.TemporaryProperties.Add(property);
                            continue;
                        }

                        string propertyName = property.Metadata.Name;
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        if (property.Metadata.PropertyInfo.GetCustomAttribute<IgnoreAuditAttribute>() == null)
                        {
                            switch (entry.State)
                            {
                                case EntityState.Added:
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                    auditEntry.AuditType = AuditType.Create;
                                    break;
                                case EntityState.Modified:
                                    if (property.IsModified)
                                    {
                                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                                        auditEntry.NewValues[propertyName] = property.CurrentValue;

                                        auditEntry.AuditType = tmpSoftDelete ? AuditType.Delete : AuditType.Update;

                                        auditEntry.ChangedColumns.Add(propertyName);
                                    }
                                    break;
                                case EntityState.Deleted:
                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                                    auditEntry.AuditType = AuditType.Delete;
                                    break;
                            }
                        }
                    }
                }
            }

            this.Audits.AddRange(auditEntries.Where(w => !w.HasTemporaryProperties).Select(s => s.ToAudit()));
            return auditEntries.Where(w => w.HasTemporaryProperties);
        }

        private async Task AuditAfterSaveChanges(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (!auditEntries.Any())
                return;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var property in auditEntry.TemporaryProperties)
                {
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[property.Metadata.Name] = property.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[property.Metadata.Name] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(property.Metadata.Name);
                    }
                }
            }

            this.Audits.AddRange(auditEntries.Select(s => s.ToAudit()));
            await this.SaveChangesAsync(cancellationToken);
        }
    }
}
