using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaFuncionario;

public partial class LocacoesAtivasView : UserControl
{
    public LocacoesAtivasView()
    {
        InitializeComponent();

        // TODO: trocar pelas locações ativas vindas do controller.
        var locacoes = DadosDeExemplo.LocacoesAtivas();
        ListaLocacoes.ItemsSource = locacoes;
        TextoVazio.Visibility = locacoes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ListaLocacoes_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: LocacaoItem locacao })
        {
            Navegador.IrPara(new RegistrarDevolucaoView(locacao.ReservaId));
        }
    }
}