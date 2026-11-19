using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;

namespace FrotaGO.Views.Componentes;

public partial class PagamentoView : UserControl
{
    private const int MaximoParcelas = 12;
    private static readonly string[] Bandeiras = ["Visa", "Mastercard", "Elo", "American Express"];

    private decimal _valor;

    public PagamentoView()
    {
        InitializeComponent();
        CampoBandeira.ItemsSource = Bandeiras;
        CampoBandeira.SelectedIndex = 0;
        AtualizarParcelas();
    }

    /// <summary>Valor a pagar, usado para calcular o valor de cada parcela.</summary>
    public decimal Valor
    {
        get => _valor;
        set
        {
            _valor = value;
            AtualizarParcelas();
        }
    }

    private bool EhCartao => OpcaoCartao.IsChecked == true;

    public string Descricao => EhCartao
        ? $"cartão {CampoBandeira.SelectedItem} final {CampoDigitos.Text} em {CampoParcelas.SelectedIndex + 1}x"
        : "Pix";

    public Pagamento CriarPagamento() => EhCartao
        ? new Cartao
        {
            Valor = _valor,
            DataPagamento = DateTime.Now,
            Bandeira = (string)CampoBandeira.SelectedItem,
            UltimosDigitos = CampoDigitos.Text.Trim(),
            Parcelas = CampoParcelas.SelectedIndex + 1
        }
        : new Pix
        {
            Valor = _valor,
            DataPagamento = DateTime.Now,
            CodigoTransacao = CampoCodigoPix.Text.Trim()
        };

    /// <summary>Usa a validação do próprio model (Cartao ou Pix).</summary>
    public string? Validar()
    {
        var pagamento = CriarPagamento();
        var digitosValidos = pagamento is not Cartao cartao || cartao.UltimosDigitos.All(char.IsDigit);

        if (pagamento.Validar() && digitosValidos) return null;

        return EhCartao
            ? "Informe os 4 últimos dígitos do cartão."
            : "Informe o código da transação Pix.";
    }

    private void Forma_Checked(object sender, RoutedEventArgs e)
    {
        // O evento também dispara durante o InitializeComponent.
        if (!IsInitialized) return;

        PainelCartao.Visibility = EhCartao ? Visibility.Visible : Visibility.Collapsed;
        PainelPix.Visibility = EhCartao ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AtualizarParcelas()
    {
        var selecionada = Math.Max(CampoParcelas.SelectedIndex, 0);
        CampoParcelas.ItemsSource = Enumerable.Range(1, MaximoParcelas)
            .Select(parcelas => $"{parcelas}x de {FormatoHelper.Moeda(_valor / parcelas)}")
            .ToList();
        CampoParcelas.SelectedIndex = selecionada;
    }
}