using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class RelatoriosView : UserControl
{
    private const int DiasPadrao = 30;

    public RelatoriosView()
    {
        InitializeComponent();
        CampoInicio.Text = FormatoHelper.DataCampo(DateTime.Today.AddDays(-DiasPadrao));
        CampoFim.Text = FormatoHelper.DataCampo(DateTime.Today);
        Atualizar();
    }

    private void Atualizar_Click(object sender, RoutedEventArgs e) => Atualizar();

    private void Atualizar()
    {
        if (!FormatoHelper.TentarLerData(CampoInicio.Text, out var inicio) ||
            !FormatoHelper.TentarLerData(CampoFim.Text, out var fim))
        {
            MostrarErro("Informe as datas no formato dd/mm/aaaa.");
            return;
        }

        if (fim < inicio)
        {
            MostrarErro("A data final deve ser igual ou posterior à inicial.");
            return;
        }

        TextoMensagem.Visibility = Visibility.Collapsed;

        // TODO: trocar pelo controller, que busca estes dados com SQL no RelatorioRepository.
        var locacoes = DadosDeExemplo.LocacoesNoPeriodo(inicio, fim);
        var faturamento = locacoes.Sum(locacao => locacao.Veiculo.Total);

        TextoFaturamento.Text = FormatoHelper.Moeda(faturamento);
        TextoLocacoes.Text = locacoes.Count.ToString();
        TextoValorMedio.Text = FormatoHelper.Moeda(locacoes.Count == 0 ? 0 : faturamento / locacoes.Count);

        var ranking = locacoes
            .GroupBy(locacao => locacao.Veiculo.Nome)
            .Select(grupo => new { Veiculo = grupo.Key, Locacoes = grupo.Count(), Total = grupo.Sum(item => item.Veiculo.Total) })
            .OrderByDescending(item => item.Locacoes)
            .ThenByDescending(item => item.Total)
            .Select((item, indice) => new LinhaRanking($"{indice + 1}º", item.Veiculo, item.Locacoes,
                                                        FormatoHelper.Moeda(item.Total)))
            .ToList();

        TabelaRanking.ItemsSource = ranking;
        TextoVazio.Visibility = ranking.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}