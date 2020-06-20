using System;
using System.ComponentModel.DataAnnotations;

namespace Open.API.Domain
{
    public interface IEntity
    {
        [Key] public Guid Id { get; set; }
    }
}
