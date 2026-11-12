using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaFuncionario;

public partial class IniciarLocacaoView : UserControl
{
    private readonly ReservaExemplo _reserva;

    public IniciarLocacaoView(int reservaId)
    {
        InitializeComponent();

        // TODO: buscar a reserva pelo controller.
        _reserva = DadosDeExemplo.Buscar(reservaId);

        var resumo = ResumoReservaItem.Criar(_reserva.Veiculo, _reserva.Retirada, _reserva.Devolucao, "Valor total");
        resumo.Linhas.InsertRange(0, [new("Cliente", _reserva.Cliente.Nome),
                                      new("Documento", _reserva.Cliente.Documento)]);
        Resumo.DataContext = resumo;
        SecaoPagamento.Valor = _reserva.Veiculo.Total;
    }

    private void IniciarLocacao_Click(object sender, RoutedEventArgs e)
    {
        var erro = Validar(out var kmSaida);
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // TODO: trocar pelo controller: cria o Contrato, o ItemContrato e o Pagamento,
        // converte a reserva e marca o veículo como alugado.
        var contrato = DadosDeExemplo.IniciarLocacao(_reserva.Id, kmSaida);

        MessageBox.Show(
            $"Locação iniciada. Contrato nº {contrato}.\nPagamento por {SecaoPagamento.Descricao}.",
            "Locação iniciada", MessageBoxButton.OK, MessageBoxImage.Information);
        Navegador.IrParaHome();
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