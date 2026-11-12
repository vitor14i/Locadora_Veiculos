using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class RevisarReservaView : UserControl
{
    private readonly VeiculoItem _veiculo;
    private readonly DateTime _retirada;
    private readonly DateTime _devolucao;

    public RevisarReservaView(VeiculoItem veiculo, DateTime retirada, DateTime devolucao)
    {
        InitializeComponent();
        (_veiculo, _retirada, _devolucao) = (veiculo, retirada, devolucao);
        Resumo.DataContext = ResumoReservaItem.Criar(veiculo, retirada, devolucao, "Valor estimado");
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new BuscaVeiculosView(_retirada, _devolucao));

    private void AgendarRetirada_Click(object sender, RoutedEventArgs e)
    {
        // TODO: trocar pelo controller, que confere de novo a disponibilidade e salva a reserva.
        DadosDeExemplo.Agendar(_veiculo, _retirada, _devolucao);
        Navegador.IrPara(new MinhasReservasView(
            $"Retirada agendada para {FormatoHelper.DiaEHora(_retirada)}."));
    }
}