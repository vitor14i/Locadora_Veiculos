using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Views.AreaFuncionario;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaCliente;

public partial class RevisarReservaView : UserControl
{
    private readonly ReservaController _reservaController;

    private readonly VeiculoItem _veiculo;
    private readonly DateTime _retirada;
    private readonly DateTime _devolucao;

    /// <summary>Preenchido quando um funcionário faz a reserva para um cliente na loja.</summary>
    private readonly ClienteItem? _cliente;

    public RevisarReservaView(VeiculoItem veiculo, DateTime retirada, DateTime devolucao,
                              ClienteItem? cliente = null)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
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
        // O controller confere de novo a disponibilidade e salva a reserva.
        // Na loja, a reserva é do cliente escolhido. No app, é do cliente logado.
        var clienteId = _cliente?.Id ?? Navegador.Usuario!.Id;
        var resultado = _reservaController.Criar(clienteId, _veiculo.VeiculoId, _retirada, _devolucao);

        if (!resultado.Sucesso)
        {
            MessageBox.Show(resultado.Erro, "Não foi possível reservar",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_cliente is not null)
        {
            Navegador.IrPara(new IniciarLocacaoView(resultado.Valor.Id));
            return;
        }

        Navegador.IrPara(new MinhasReservasView(
            $"Retirada agendada para {FormatoHelper.DiaEHora(_retirada)}."));
    }
}
