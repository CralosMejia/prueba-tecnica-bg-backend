using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Application.Authentication;

public sealed record LoginRequest(
    [Required(
        ErrorMessage = "El correo electrónico es obligatorio."
    )]
    [EmailAddress(
        ErrorMessage = "El correo electrónico no es válido."
    )]
    string Email,

    [Required(
        ErrorMessage = "La contraseña es obligatoria."
    )]
    [MinLength(
        8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres."
    )]
    string Password
);