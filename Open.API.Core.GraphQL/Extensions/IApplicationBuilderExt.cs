
using HotChocolate.AspNetCore;

using Microsoft.AspNetCore.Builder;

namespace Open.API.Core.GraphQL.Extensions
{
    public static class IApplicationBuilderExt
    {
        public static IApplicationBuilder EnableGraphQL(this IApplicationBuilder builder)
        {
            builder
                .UseGraphQL("/graphql")
                .UseGraphQLHttpPost()
                .UsePlayground("/graphql", "/playground");

            return builder;
        }
    }
}
