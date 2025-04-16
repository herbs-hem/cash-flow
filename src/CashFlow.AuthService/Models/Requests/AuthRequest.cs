using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Requests;

public record AuthRequest(
    
    [Required(ErrorMessage = "Por favor informe a senha")]
    [StringLength(10, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 10 caracteres")]
    string Password,

    [Required(ErrorMessage = "Por favor informe o usuario")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "O usuario deve ter entre 3 e 30 caracteres")]
    string Username);