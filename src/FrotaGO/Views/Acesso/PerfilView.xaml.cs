using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Acesso;

public partial class PerfilView : UserControl
{
    private const int TamanhoMinimoSenha = 8;

    private readonly UsuarioSessao _usuario;

    public PerfilView()
    {
        InitializeComponent();
        _usuario = Navegador.Usuario ?? throw new InvalidOperationException("Nenhum usuário logado.");

        CampoNome.Text = _usuario.Nome;
        CampoEmail.Text = _usuario.Email;

        // TODO: buscar os dados completos do usuário pelo controller.
        if (_usuario.EhCliente)
        {
            CampoTelefone.Text = DadosDeExemplo.ClienteAtual.Telefone;
            RotuloInfo.Text = "Documento";
            TextoInfo.Text = DadosDeExemplo.ClienteAtual.Documento;
        }
        else
        {
            PainelTelefone.Visibility = Visibility.Collapsed;
            RotuloInfo.Text = "Perfil";
            TextoInfo.Text = _usuario.Perfil;
        }
    }

    private void SalvarDados_Click(object sender, RoutedEventArgs e)
    {
        var email = CampoEmail.Text.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(CampoNome.Text) || !email.Contains('@'))
        {
            Mostrar(MensagemDados, "Informe o nome e um e-mail válido.");
            return;
        }

        // TODO: salvar pelo controller, usando Usuario.AlterarDados.
        Navegador.AtualizarUsuario(_usuario with { Nome = CampoNome.Text.Trim(), Email = email });
        Mostrar(MensagemDados, "Dados atualizados com sucesso.", sucesso: true);
    }

    private void AlterarSenha_Click(object sender, RoutedEventArgs e)
    {
        var erro = ValidarSenha();
        if (erro is not null)
        {
            Mostrar(MensagemSenha, erro);
            return;
        }

        // TODO: trocar pelo controller, que confere a senha atual com Usuario.ValidarSenha.
        CampoSenhaAtual.Clear();
        CampoNovaSenha.Clear();
        CampoConfirmarSenha.Clear();
        Mostrar(MensagemSenha, "Senha alterada com sucesso.", sucesso: true);
    }

    private string? ValidarSenha()
    {
        if (CampoSenhaAtual.Password.Length == 0 || CampoNovaSenha.Password.Length == 0)
        {
            return "Preencha a senha atual e a nova senha.";
        }

        if (CampoNovaSenha.Password.Length < TamanhoMinimoSenha)
        {
            return $"A nova senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.";
        }

        if (CampoNovaSenha.Password != CampoConfirmarSenha.Password)
        {
            return "As senhas não coincidem.";
        }

        if (CampoNovaSenha.Password == CampoSenhaAtual.Password)
        {
            return "A nova senha deve ser diferente da atual.";
        }

        return null;
    }

    private void Mostrar(TextBlock alvo, string texto, bool sucesso = false)
    {
        alvo.Text = texto;
        alvo.Foreground = (Brush)FindResource(sucesso ? "CorSucesso" : "CorErro");
        alvo.Visibility = Visibility.Visible;
    }
}