using System.Linq;

using HotChocolate;

using Microsoft.Extensions.DependencyInjection;

using Open.API.Common.Extensions;

namespace Open.API.Core.GraphQL.Extensions
{
    public static class IServiceCollectionExt
    {
        public static IServiceCollection RegisterGraphQL(this IServiceCollection services)
        {
            services
                .AddGraphQL(SchemaBuilder.New()
                    .AddQueryType<GraphQLQuery>()
                    .AddMutationType<GraphQLMutation>()
                    .AddTypes(typeof(IGraphQLType).Assembly.GetTypes().Where(w => w.IsImplementedFrom(typeof(IGraphQLType))).ToArray())
                    .AddAuthorizeDirectiveType()
                    .Create()
                );

            services.AddDataLoaderRegistry();

            return services;
        }
    }
}
