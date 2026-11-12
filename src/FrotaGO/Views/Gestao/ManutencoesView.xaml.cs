using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class ManutencoesView : UserControl
{
    public ManutencoesView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        MostrarMensagem(mensagemSucesso);
        Carregar();
    }

    private void Carregar()
    {
        // TODO: trocar pelas manutenções vindas do controller.
        TabelaManutencoes.ItemsSource = DadosDeExemplo.Manutencoes
            .OrderByDescending(manutencao => manutencao.EstaEmAndamento())
            .ThenByDescending(manutencao => manutencao.DataInicio)
            .Select(CriarLinha)
            .ToList();
    }

    private static LinhaManutencao CriarLinha(Manutencao manutencao) => new(
        manutencao.Id,
        $"{manutencao.Veiculo?.Modelo} · {manutencao.Veiculo?.Placa}",
        manutencao.Descricao,
        FormatoHelper.DiaEHora(manutencao.DataInicio),
        manutencao.DataFim is DateTime fim ? FormatoHelper.DiaEHora(fim) : "—",
        manutencao.EstaEmAndamento() ? "Em andamento" : "Finalizada",
        manutencao.EstaEmAndamento());

    private void Finalizar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: LinhaManutencao linha }) return;

        var resposta = MessageBox.Show(
            $"Finalizar a manutenção de {linha.Veiculo}? O veículo voltará a ficar disponível.",
            "Finalizar manutenção", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (resposta != MessageBoxResult.Yes) return;

        // TODO: trocar pelo controller.
        DadosDeExemplo.FinalizarManutencao(DadosDeExemplo.Manutencoes.First(item => item.Id == linha.Id));
        MostrarMensagem("Manutenção finalizada. O veículo está disponível.");
        Carregar();
    }

    private void NovaManutencao_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new ManutencaoFormView());

    private void MostrarMensagem(string? texto)
    {
        if (texto is null) return;
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}