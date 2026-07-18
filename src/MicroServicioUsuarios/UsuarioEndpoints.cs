using FluentValidation;
using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioUsuarios
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuariosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/usuario");

            group.MapPost("/", MapCreateAsync);         // (Crear Usuario)
            group.MapPut("/{id}", MapPutAsync);         // (Modificar un Usuario)
            group.MapDelete("/{id}", MapDeleteAsync);   // (Borrar un Usuario)
            group.MapGet("/", MapGetAllAsync);          // (Obtener todos los Usuarios)
            group.MapGet("/{id}", MapGetByID);          // (Obtener un solo Usuario)
        }

        // POST (Crear Usuario)
        public static async Task<IResult> MapCreateAsync(
            [FromHeader(Name = "Authorization")] string token,
            [FromBody] UsuarioRequest usuario,
            IUsuarioService usuarioService)
        {
            return await usuarioService.CreateUsuarioServiceAsync(usuario, token);
        }

        /* Aún no implementados */
        // PUT (Modificar un Usuario)
        public static async Task<IResult> MapPutAsync()
        {
            return Results.Json(new { message = "OK" }, statusCode: 200); // Devuelve lo actualizado con un código de estado 200 (OK) investigar si está bien
        }

        // DELETE (Borrar un Usuario)
        public static async Task<IResult> MapDeleteAsync()
        {
            return Results.Json(new { message = "OK" }, statusCode: 200); // Devuelve lo eliminado con un código de estado 200 (OK) investigar si está bien
        }

        // GET (ALL) (Obtener todos los Usuarios)
        public static async Task<IResult> MapGetAllAsync()
        {
            return Results.Json(new { message = "OK" }, statusCode: 200); // Devuelve todos los usuarios con un código de estado 200 (OK) investigar si está bien
        }

        // GET (BY ID) (Obtener un solo Usuario)
        public static async Task<IResult> MapGetByID()
        {
            return Results.Json(new { message = "OK" }, statusCode: 200); // Devuelve el usuario con un código de estado 200 (OK) investigar si está bien
        }

    }
}
