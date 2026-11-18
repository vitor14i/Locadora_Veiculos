using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Repositories.Resultados;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.Gestao;

public partial class RelatoriosView : UserControl
{
    private const int DiasPadrao = 30;

    private readonly RelatorioController _relatorioController;

    public RelatoriosView()
    {
        InitializeComponent();
        _relatorioController = FabricaDeControllers.CriarRelatorioController();
        CampoInicio.Text = FormatoHelper.DataCampo(DateTime.Today.AddDays(-DiasPadrao));
        CampoFim.Text = FormatoHelper.DataCampo(DateTime.Today);
        Atualizar();
    }

    private void Atualizar_Click(object sender, RoutedEventArgs e) => Atualizar();

    private void Atualizar()
    {
        // A tela confere só o formato. A regra do período fica no controller.
        if (!FormatoHelper.TentarLerData(CampoInicio.Text, out var inicio) ||
            !FormatoHelper.TentarLerData(CampoFim.Text, out var fim))
        {
            MostrarErro("Informe as datas no formato dd/mm/aaaa.");
            return;
        }

        var resultado = _relatorioController.Gerar(inicio, fim);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        TextoMensagem.Visibility = Visibility.Collapsed;

        var relatorio = resultado.Valor;
        TextoFaturamento.Text = FormatoHelper.Moeda(relatorio.Resumo.Faturamento);
        TextoLocacoes.Text = relatorio.Resumo.Locacoes.ToString();
        TextoValorMedio.Text = FormatoHelper.Moeda(relatorio.ValorMedio);

        // O ranking já vem ordenado do SQL. A posição é o índice na lista.
        var ranking = relatorio.Ranking
            .Select((veiculo, indice) => CriarLinha(veiculo, indice + 1))
            .ToList();

        TabelaRanking.ItemsSource = ranking;
        TextoVazio.Visibility = ranking.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Converte o resultado do SQL na linha da tabela, com o mesmo nome de veículo das outras telas.</summary>
    private static LinhaRanking CriarLinha(VeiculoMaisLocado veiculo, int posicao) =>
        new($"{posicao}º", $"{veiculo.Marca} {veiculo.Modelo} · {veiculo.Ano}", veiculo.Locacoes,
            FormatoHelper.Moeda(veiculo.Faturamento));

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}