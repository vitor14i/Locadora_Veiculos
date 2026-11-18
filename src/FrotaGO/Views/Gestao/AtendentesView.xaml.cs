using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FrotaGO.Controllers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.Gestao;

public partial class AtendentesView : UserControl
{
    private const string SituacaoAtivo = "Ativo";
    private const string SituacaoInativo = "Inativo";

    private readonly AtendenteController _atendenteController;

    public AtendentesView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        _atendenteController = FabricaDeControllers.CriarAtendenteController();
        MostrarMensagem(mensagemSucesso);
        Carregar();
    }

    private void Carregar()
    {
        // A ordem por nome vem do repositório.
        TabelaAtendentes.ItemsSource = _atendenteController.Listar().Select(CriarLinha).ToList();
    }

    private static LinhaAtendente CriarLinha(Atendente atendente) => new(
        atendente.Id,
        atendente.Nome,
        atendente.Email,
        atendente.Matricula,
        atendente.Ativo ? SituacaoAtivo : SituacaoInativo);

    /// <summary>Desativa ou reativa a conta. Contas desativadas não podem fazer login.</summary>
    private void AlterarSituacao_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: LinhaAtendente linha }) return;

        var estavaAtivo = linha.Status == SituacaoAtivo;

        if (estavaAtivo)
        {
            var resposta = MessageBox.Show(
                $"Desativar a conta de {linha.Nome}? Ela não poderá mais fazer login.",
                "Desativar atendente", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resposta != MessageBoxResult.Yes) return;
        }

        // O gerente é o usuário logado. Sem usuário, o id 0 não existe e o controller recusa.
        var resultado = _atendenteController.AlterarSituacao(linha.Id, Navegador.Usuario?.Id ?? 0);

        if (resultado.Sucesso)
        {
            MostrarMensagem(estavaAtivo ? "Atendente desativado." : "Atendente reativado.");
        }
        else
        {
            MostrarMensagem(resultado.Erro, sucesso: false);
        }

        Carregar();
    }

    private void NovoAtendente_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new AtendenteFormView());

    private void MostrarMensagem(string? texto, bool sucesso = true)
    {
        if (texto is null) return;
        TextoMensagem.Text = texto;
        // O .xaml começa com a cor de sucesso; um erro troca para a cor de erro.
        TextoMensagem.Foreground = (Brush)FindResource(sucesso ? "CorSucesso" : "CorErro");
        TextoMensagem.Visibility = Visibility.Visible;
    }
}
