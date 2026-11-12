using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.AreaFuncionario;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class RevisarReservaView : UserControl
{
    private readonly VeiculoItem _veiculo;
    private readonly DateTime _retirada;
    private readonly DateTime _devolucao;

    /// <summary>Preenchido quando um funcionário faz a reserva para um cliente na loja.</summary>
    private readonly ClienteExemplo? _cliente;

    public RevisarReservaView(VeiculoItem veiculo, DateTime retirada, DateTime devolucao,
                              ClienteExemplo? cliente = null)
    {
        InitializeComponent();
        (_veiculo, _retirada, _devolucao, _cliente) = (veiculo, retirada, devolucao, cliente);

        var resumo = ResumoReservaItem.Criar(veiculo, retirada, devolucao, "Valor estimado");

        if (cliente is not null)
        {
            resumo.Linhas.InsertRange(0, [new("Cliente", cliente.Nome), new("Documento", cliente.Documento)]);
            SubtituloTela.Text = "Confira os dados com o cliente antes de iniciar a locação.";
            TextoAviso.Text = "Confira um documento com foto e a CNH do condutor.";
            BotaoConfirmar.Content = "Iniciar locação agora";
        }

        Resumo.DataContext = resumo;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new BuscaVeiculosView(_retirada, _devolucao,
                                               modoFuncionario: _cliente is not null, cliente: _cliente));

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        // TODO: trocar pelo controller, que confere de novo a disponibilidade e salva a reserva.
        var reserva = DadosDeExemplo.Agendar(_cliente ?? DadosDeExemplo.ClienteAtual,
                                             _veiculo, _retirada, _devolucao);

        if (_cliente is not null)
        {
            Navegador.IrPara(new IniciarLocacaoView(reserva.Id));
            return;
        }

        Navegador.IrPara(new MinhasReservasView(
            $"Retirada agendada para {FormatoHelper.DiaEHora(_retirada)}."));
    }
}