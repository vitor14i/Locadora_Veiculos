using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FrotaGO.Controllers;
using FrotaGO.Models;
using FrotaGO.Views.AreaCliente;
using FrotaGO.Views.AreaFuncionario;

namespace FrotaGO.Views.Acesso;

public partial class LoginView : UserControl
{
    private readonly ContaController _contaController;

    public LoginView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        _contaController = FabricaDeControllers.CriarContaController();

        if (mensagemSucesso is not null)
        {
            MostrarMensagem(mensagemSucesso, sucesso: true);
        }
    }

    private void Entrar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CampoEmail.Text) || CampoSenha.Password.Length == 0)
        {
            MostrarMensagem("Preencha o e-mail e a senha.");
            return;
        }

        var resultado = _contaController.Entrar(CampoEmail.Text, CampoSenha.Password);
        if (!resultado.Sucesso)
        {
            MostrarMensagem(resultado.Erro);
            return;
        }

        AbrirHome(resultado.Valor);
    }

    /// <summary>Guarda o usuário na sessão e abre a home do perfil dele.</summary>
    private static void AbrirHome(Usuario usuario)
    {
        var sessao = UsuarioSessao.De(usuario);

        if (sessao.EhCliente)
        {
            Navegador.Entrar(sessao, () => new HomeClienteView(sessao.Nome));
        }
        else
        {
            Navegador.Entrar(sessao, () => new HomeFuncionarioView(sessao.Nome, sessao.EhGerente));
        }
    }

    private void CriarConta_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new CriarContaView(), mostrarCabecalho: false);

    private void MostrarMensagem(string texto, bool sucesso = false)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Foreground = (Brush)FindResource(sucesso ? "CorSucesso" : "CorErro");
        TextoMensagem.Visibility = Visibility.Visible;
    }
}
