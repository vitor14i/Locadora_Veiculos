using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class BuscaVeiculosView : UserControl
{
    private DateTime _retirada;
    private DateTime _devolucao;

    /// <summary>Se o período for informado, por exemplo ao voltar da revisão, a busca é refeita.</summary>
    public BuscaVeiculosView(DateTime? retirada = null, DateTime? devolucao = null)
    {
        InitializeComponent();

        var retiradaPadrao = DateTime.Today.AddDays(1).AddHours(10);
        CampoRetirada.Text = FormatoHelper.DataHoraCampo(retirada ?? retiradaPadrao);
        CampoDevolucao.Text = FormatoHelper.DataHoraCampo(devolucao ?? retiradaPadrao.AddDays(3));

        if (retirada.HasValue && devolucao.HasValue)
        {
            Buscar();
        }
    }

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

        TextoMensagem.Visibility = Visibility.Collapsed;

        // TODO: trocar pelos veículos disponíveis vindos do controller
        // (sem reserva no período e sem manutenção em andamento).
        var veiculos = DadosDeExemplo.Veiculos
            .Where(veiculo => TipoSelecionado is null || veiculo.Tipo == TipoSelecionado)
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

        if (_retirada < DateTime.Now)
        {
            return "A retirada deve ser em uma data futura.";
        }

        if (_devolucao <= _retirada)
        {
            return "A devolução deve ser depois da retirada.";
        }

        return null;
    }

    private void ListaVeiculos_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: VeiculoItem veiculo })
        {
            Navegador.IrPara(new RevisarReservaView(veiculo, _retirada, _devolucao));
        }
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}