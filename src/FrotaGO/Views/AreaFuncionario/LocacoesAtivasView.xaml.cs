using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaFuncionario;

public partial class LocacoesAtivasView : UserControl
{
    private readonly LocacaoController _locacaoController;

    public LocacoesAtivasView()
    {
        InitializeComponent();
        _locacaoController = FabricaDeControllers.CriarLocacaoController();

        // Os contratos ativos vêm do banco, e a situação é calculada no MapeadorDeTela.
        var locacoes = _locacaoController.ListarAtivos().Select(MapeadorDeTela.CriarLocacao).ToList();
        ListaLocacoes.ItemsSource = locacoes;
        TextoVazio.Visibility = locacoes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ListaLocacoes_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: LocacaoItem locacao })
        {
            Navegador.IrPara(new RegistrarDevolucaoView(locacao.ContratoId));
        }
    }
}