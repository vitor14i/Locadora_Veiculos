using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Acesso;

namespace FrotaGO.Views;

/// <summary>Troca a tela exibida na janela principal.</summary>
public static class Navegador
{
    private static MainWindow? _janela;
    private static Func<UserControl>? _criarHome;

    public static void Iniciar(MainWindow janela) => _janela = janela;

    public static void IrPara(UserControl tela, bool mostrarCabecalho = true) =>
        Janela.ExibirTela(tela, mostrarCabecalho);

    /// <summary>Registra a home do usuário logado e abre essa tela.</summary>
    public static void Entrar(string nomeUsuario, Func<UserControl> criarHome)
    {
        _criarHome = criarHome;
        Janela.DefinirIniciais(NomeHelper.Iniciais(nomeUsuario));
        IrParaHome();
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
        _criarHome = null;
        IrPara(new LoginView(), mostrarCabecalho: false);
    }

    private static MainWindow Janela =>
        _janela ?? throw new InvalidOperationException("A janela principal ainda não foi iniciada.");
}