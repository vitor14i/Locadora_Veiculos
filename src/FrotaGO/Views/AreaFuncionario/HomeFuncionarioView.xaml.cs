using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Views.AreaCliente;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Gestao;

namespace FrotaGO.Views.AreaFuncionario;

public partial class HomeFuncionarioView : UserControl
{
    private readonly ReservaController _reservaController;

    public HomeFuncionarioView(string nomeFuncionario, bool ehGerente)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeFuncionario)}";
        LinhaGestao.Visibility = ehGerente ? Visibility.Visible : Visibility.Collapsed;

        ExibirVisitas(_reservaController.ListarVisitasDeHoje().Select(MapeadorDeTela.CriarVisita).ToList());
    }

    private void ExibirVisitas(IReadOnlyList<VisitaItem> visitas)
    {
        ListaVisitas.ItemsSource = visitas;
        TextoSemVisitas.Visibility = visitas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TextoResumo.Text = visitas.Count switch
        {
            0 => "Nenhuma visita agendada para hoje.",
            1 => "Você tem 1 visita agendada para hoje.",
            _ => $"Você tem {visitas.Count} visitas agendadas para hoje."
        };
    }
    private void NovaReserva_Click(object sender, RoutedEventArgs e) =>
    Navegador.IrPara(new BuscaVeiculosView(modoFuncionario: true));

    private void RegistrarDevolucao_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LocacoesAtivasView());

    private void ListaVisitas_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: VisitaItem visita })
        {
            Navegador.IrPara(new IniciarLocacaoView(visita.ReservaId));
        }
    }
    private void Veiculos_Click(object sender, RoutedEventArgs e) => Navegador.IrPara(new VeiculosView());

    private void Manutencoes_Click(object sender, RoutedEventArgs e) => Navegador.IrPara(new ManutencoesView());

    private void Atendentes_Click(object sender, RoutedEventArgs e) => Navegador.IrPara(new AtendentesView());

    private void Relatorios_Click(object sender, RoutedEventArgs e) => Navegador.IrPara(new RelatoriosView());

    private void Clientes_Click(object sender, RoutedEventArgs e) => Navegador.IrPara(new ClientesView());
}
