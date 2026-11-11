using System.Windows.Controls;

namespace FrotaGO.Views;

/// <summary>Troca a tela exibida na janela principal.</summary>
public static class Navegador
{
    private static MainWindow? _janela;

    public static void Iniciar(MainWindow janela) => _janela = janela;

    public static void IrPara(UserControl tela, bool mostrarCabecalho = true)
    {
        if (_janela is null)
        {
            throw new InvalidOperationException("A janela principal ainda não foi iniciada.");
        }

        _janela.ExibirTela(tela, mostrarCabecalho);
    }
}