using MicroServicioAuth.Entities;
using MicroServicioAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioAuth
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
        { 
            var group = routes
                .MapGroup("/auth")
                .WithTags("Auth");
        }
    }
}
