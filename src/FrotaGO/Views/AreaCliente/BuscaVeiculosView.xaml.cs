using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaCliente;

public partial class BuscaVeiculosView : UserControl
{
    private readonly ReservaController _reservaController;
    private readonly ContaController _contaController;

    private DateTime _retirada;
    private DateTime _devolucao;
    private readonly bool _modoFuncionario;

    /// <summary>Se o período for informado, por exemplo ao voltar da revisão, a busca é refeita.</summary>
    public BuscaVeiculosView(DateTime? retirada = null, DateTime? devolucao = null, bool modoFuncionario = false, ClienteItem? cliente = null)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        _contaController = FabricaDeControllers.CriarContaController();
        _modoFuncionario = modoFuncionario;

        if (modoFuncionario)
        {
            ConfigurarModoFuncionario(cliente);
        }

        // No balcão, a retirada é logo em seguida. O cliente reserva a partir do dia seguinte.
        var retiradaPadrao = modoFuncionario
            ? ProximaMeiaHora(DateTime.Now)
            : DateTime.Today.AddDays(1).AddHours(10);

        CampoRetirada.Text = FormatoHelper.DataHoraCampo(retirada ?? retiradaPadrao);
        CampoDevolucao.Text = FormatoHelper.DataHoraCampo(devolucao ?? retiradaPadrao.AddDays(3));

        if (retirada.HasValue && devolucao.HasValue)
        {
            Buscar();
        }
    }

    private ClienteItem? ClienteSelecionado =>
        _modoFuncionario ? CampoCliente.SelectedItem as ClienteItem : null;

    private void ConfigurarModoFuncionario(ClienteItem? cliente)
    {
        TituloTela.Text = "Nova reserva";
        SubtituloTela.Text = "Escolha o cliente, o período e o veículo.";
        PainelCliente.Visibility = Visibility.Visible;

        CampoCliente.ItemsSource = _contaController.ListarClientesAtivos()
            .Select(MapeadorDeTela.CriarClienteItem)
            .OrderBy(item => item.Nome)
            .ToList();
        CampoCliente.SelectedItem = cliente;
    }

    private static DateTime ProximaMeiaHora(DateTime data) =>
        data.Date.AddHours(data.Hour).AddMinutes(data.Minute < 30 ? 30 : 60);

    private string? TipoSelecionado =>
        OpcaoCarro.IsChecked == true ? "Carro" :
        OpcaoMoto.IsChecked == true ? "Moto" : null;

    private void Buscar_Click(object sender, RoutedEventArgs e) => Buscar();

    private void Buscar()
    {
        var erro = LerPeriodo();
        if (erro is not null)
        {
            MostrarErro(erro);
            ListaVeiculos.ItemsSource = null;
            return;
        }

        // As regras do período (retirada futura e devolução depois da retirada) ficam no controller.
        var resultado = _reservaController.BuscarDisponiveis(_retirada, _devolucao, TipoSelecionado);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            ListaVeiculos.ItemsSource = null;
            return;
        }

        TextoMensagem.Visibility = Visibility.Collapsed;

        var veiculos = resultado.Valor
            .Select(MapeadorDeTela.CriarItem)
            .Select(veiculo => veiculo.ParaPeriodo(_retirada, _devolucao))
            .ToList();

        ListaVeiculos.ItemsSource = veiculos;
        TextoResultado.Text = veiculos.Count == 0
            ? "Nenhum veículo disponível nesse período."
            : $"{FormatoHelper.Quantidade(veiculos.Count, "veículo disponível", "veículos disponíveis")} · " +
              FormatoHelper.Quantidade(veiculos[0].Diarias, "diária", "diárias");
    }

    private string? LerPeriodo()
    {
        if (!FormatoHelper.TentarLerDataHora(CampoRetirada.Text, out _retirada) ||
            !FormatoHelper.TentarLerDataHora(CampoDevolucao.Text, out _devolucao))
        {
            return "Informe as datas no formato dd/mm/aaaa hh:mm.";
        }

        return null;
    }

    private void ListaVeiculos_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: VeiculoItem veiculo })
        {
            // No balcão, a reserva precisa de um cliente escolhido.
            if (_modoFuncionario && ClienteSelecionado is null)
            {
                MostrarErro("Escolha o cliente.");
                return;
            }

            Navegador.IrPara(new RevisarReservaView(veiculo, _retirada, _devolucao, ClienteSelecionado));
        }
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}
