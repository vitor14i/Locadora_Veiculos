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
    private readonly LocacaoController _locacaoController; // Novo na POO-18
    private readonly Reserva? _reserva;

    public IniciarLocacaoView(int reservaId)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        _locacaoController = FabricaDeControllers.CriarLocacaoController(); // Novo na POO-18

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

    // ---------- Alterado na POO-18 ----------

    private void IniciarLocacao_Click(object sender, RoutedEventArgs e)
    {
        if (_reserva is null)
        {
            MostrarErro("Reserva não encontrada.");
            return;
        }

        var erro = Validar(out var kmSaida);
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // As regras de negócio, inclusive a dos documentos, ficam no controller.
        var resultado = _locacaoController.IniciarLocacao(_reserva.Id, kmSaida, CampoDocumentos.IsChecked == true,
                                                          SecaoPagamento.CriarPagamento());
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        MessageBox.Show($"Locação iniciada. Contrato nº {resultado.Valor.Id}.", "Iniciar locação",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        Navegador.IrParaHome();
    }

    /// <summary>Só o preenchimento: o formato da quilometragem e os dados do pagamento.</summary>
    private string? Validar(out decimal kmSaida)
    {
        if (!FormatoHelper.TentarLerQuilometragem(CampoKmSaida.Text, out kmSaida))
        {
            return "Informe a quilometragem de saída.";
        }

        return SecaoPagamento.Validar();
    }

    // ---------- Fim do trecho alterado ----------

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) => Navegador.IrParaHome();
}
