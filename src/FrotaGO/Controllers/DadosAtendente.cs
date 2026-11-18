namespace FrotaGO.Controllers;

/// <summary>Campos do formulário de novo atendente, como foram digitados na tela.</summary>
public record DadosAtendente(string Nome, string Email, string Matricula, string Senha);
