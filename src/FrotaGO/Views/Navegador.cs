using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Acesso;

namespace FrotaGO.Views;

public static class Navegador
{
    private static MainWindow? _janela;
    private static Func<UserControl>? _criarHome;

    public static void Iniciar(MainWindow janela) => _janela = janela;

    public static void IrPara(UserControl tela, bool mostrarCabecalho = true) =>
        Janela.ExibirTela(tela, mostrarCabecalho);

    public static UsuarioSessao? Usuario { get; private set; }

    public static void Entrar(UsuarioSessao usuario, Func<UserControl> criarHome)
    {
        _criarHome = criarHome;
        AtualizarUsuario(usuario);
        IrParaHome();
    }

    public static void AtualizarUsuario(UsuarioSessao usuario)
    {
        Usuario = usuario;
        Janela.DefinirIniciais(NomeHelper.Iniciais(usuario.Nome));
    }

    public static void IrParaHome()
    {
        if (_criarHome is not null)
        {
            IrPara(_criarHome());
        }
    }

    public static void Sair()
    {
        Usuario = null;
        _criarHome = null;
        IrPara(new LoginView(), mostrarCabecalho: false);
    }

    private static MainWindow Janela =>
        _janela ?? throw new InvalidOperationException("A janela principal ainda não foi iniciada.");
}