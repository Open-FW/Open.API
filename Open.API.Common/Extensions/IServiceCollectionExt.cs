
using System.Reflection;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Open.API.Common.Extensions
{
    public static class IServiceCollectionExt
    {
        public static IServiceCollection RegisterMediatR(this IServiceCollection service)
        {
            service.AddMediatR(Assembly.Load("Open.API.Common"));

            return service;
        }
    }
}
