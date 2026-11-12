using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class AtendenteFormView : UserControl
{
    private const int TamanhoMinimoSenha = 8;

    public AtendenteFormView()
    {
        InitializeComponent();
    }

    private void Cadastrar_Click(object sender, RoutedEventArgs e)
    {
        var erro = ValidarFormulario();
        if (erro is not null)
        {
            TextoMensagem.Text = erro;
            TextoMensagem.Visibility = Visibility.Visible;
            return;
        }

        // TODO: trocar pelo controller.
        DadosDeExemplo.AdicionarAtendente(new Atendente
        {
            Nome = CampoNome.Text.Trim(),
            Email = CampoEmail.Text.Trim().ToLowerInvariant(),
            Matricula = CampoMatricula.Text.Trim().ToUpperInvariant(),
            SenhaHash = PasswordHasher.Hash(CampoSenha.Password)
        });

        Navegador.IrPara(new AtendentesView("Atendente cadastrado com sucesso."));
    }

    private string? ValidarFormulario()
    {
        TextBox[] obrigatorios = [CampoNome, CampoEmail, CampoMatricula];
        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)) || CampoSenha.Password.Length == 0)
        {
            return "Preencha todos os campos.";
        }

        var email = CampoEmail.Text.Trim().ToLowerInvariant();
        if (!email.Contains('@'))
        {
            return "Informe um e-mail válido.";
        }

        if (DadosDeExemplo.Atendentes.Any(atendente => atendente.Email == email))
        {
            return "Este e-mail já está cadastrado.";
        }

        if (CampoSenha.Password.Length < TamanhoMinimoSenha)
        {
            return $"A senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.";
        }

        if (CampoSenha.Password != CampoConfirmarSenha.Password)
        {
            return "As senhas não coincidem.";
        }

        return null;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new AtendentesView());
}