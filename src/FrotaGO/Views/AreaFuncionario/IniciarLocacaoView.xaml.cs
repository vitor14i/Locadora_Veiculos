using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaFuncionario;

public partial class IniciarLocacaoView : UserControl
{
    private readonly ReservaController _reservaController;
    private readonly Reserva? _reserva;

    public IniciarLocacaoView(int reservaId)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();

        // A reserva vem do controller, com cliente, veículo e contrato.
        _reserva = _reservaController.Obter(reservaId);
        if (_reserva is null)
        {
            MostrarErro("Reserva não encontrada.");
            return;
        }

        var veiculo = MapeadorDeTela.CriarItem(_reserva.Veiculo!).ParaPeriodo(_reserva.DataInicio, _reserva.DataFim);
        var cliente = MapeadorDeTela.CriarClienteItem(_reserva.Cliente!);

        var resumo = ResumoReservaItem.Criar(veiculo, _reserva.DataInicio, _reserva.DataFim, "Valor total");
        resumo.Linhas.InsertRange(0, [new("Cliente", cliente.Nome),
                                      new("Documento", cliente.Documento)]);
        Resumo.DataContext = resumo;
        SecaoPagamento.Valor = veiculo.Total;
    }

    private void IniciarLocacao_Click(object sender, RoutedEventArgs e)
    {
        if (_reserva is null)
        {
            MostrarErro("Reserva não encontrada.");
            return;
        }

        var erro = Validar(out _);
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // TODO (POO-18): trocar pelo controller: cria o Contrato, o ItemContrato e o Pagamento,
        // converte a reserva e marca o veículo como alugado.
        MessageBox.Show("O início da locação ainda não está disponível.", "Iniciar locação",
                        MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private string? Validar(out decimal kmSaida)
    {
        if (!FormatoHelper.TentarLerQuilometragem(CampoKmSaida.Text, out kmSaida))
        {
            return "Informe a quilometragem de saída.";
        }

        if (CampoDocumentos.IsChecked != true)
        {
            return "Confira o documento com foto e a CNH do condutor.";
        }

        return SecaoPagamento.Validar();
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) => Navegador.IrParaHome();
}
