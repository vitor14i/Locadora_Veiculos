using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.Gestao;

public partial class ManutencoesView : UserControl
{
    private readonly ManutencaoController _manutencaoController;

    public ManutencoesView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        _manutencaoController = FabricaDeControllers.CriarManutencaoController();
        MostrarMensagem(mensagemSucesso);
        Carregar();
    }

    private void Carregar()
    {
        // A ordem (em andamento primeiro, depois as mais recentes) vem do repositório.
        TabelaManutencoes.ItemsSource = _manutencaoController.Listar().Select(CriarLinha).ToList();
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

        // O gerente é o usuário logado. Sem usuário, o id 0 não existe e o controller recusa.
        var resultado = _manutencaoController.Finalizar(linha.Id, Navegador.Usuario?.Id ?? 0);
        MostrarMensagem(resultado.Sucesso ? "Manutenção finalizada. O veículo está disponível." : resultado.Erro);
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