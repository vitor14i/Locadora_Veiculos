using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class AtendentesView : UserControl
{
    public AtendentesView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        MostrarMensagem(mensagemSucesso);
        Carregar();
    }

    private void Carregar()
    {
        // TODO: trocar pelos atendentes vindos do controller.
        TabelaAtendentes.ItemsSource = DadosDeExemplo.Atendentes
            .OrderBy(atendente => atendente.Nome)
            .Select(atendente => new LinhaAtendente(atendente.Id, atendente.Nome, atendente.Email,
                                                    atendente.Matricula, atendente.Ativo ? "Ativo" : "Inativo"))
            .ToList();
    }

    /// <summary>Desativa ou reativa a conta. Contas desativadas não podem fazer login.</summary>
    private void AlterarSituacao_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: LinhaAtendente linha }) return;

        var atendente = DadosDeExemplo.Atendentes.First(item => item.Id == linha.Id);

        if (atendente.Ativo)
        {
            var resposta = MessageBox.Show(
                $"Desativar a conta de {atendente.Nome}? Ela não poderá mais fazer login.",
                "Desativar atendente", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resposta != MessageBoxResult.Yes) return;
        }

        // TODO: trocar pelo controller.
        atendente.Ativo = !atendente.Ativo;
        MostrarMensagem(atendente.Ativo ? "Atendente reativado." : "Atendente desativado.");
        Carregar();
    }

    private void NovoAtendente_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new AtendenteFormView());

    private void MostrarMensagem(string? texto)
    {
        if (texto is null) return;
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }
}