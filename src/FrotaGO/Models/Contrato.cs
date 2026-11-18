using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public class Contrato
{
    public int Id { get; set; }
    public DateTime DataGeracao { get; set; } = DateTime.Now;
    public decimal ValorTotal { get; set; }
    public decimal Desconto { get; set; }
    public StatusContrato Status { get; set; } = StatusContrato.Ativo;

    public int ReservaId { get; set; }
    public Reserva? Reserva { get; set; }
    public List<ItemContrato> Itens { get; set; } = new();

    public List<Veiculo> Veiculos { get; set; } = new();
    public List<Pagamento> Pagamentos { get; set; } = new();
    public Devolucao? Devolucao { get; set; }

    public ItemContrato AdicionarItem(Veiculo veiculo, int quantidadeDias)
    {
        var item = new ItemContrato
        {
            Veiculo = veiculo,
            VeiculoId = veiculo.Id,
            QuantidadeDias = quantidadeDias,
            ValorDiaria = veiculo.Calcular(1),
            KmInicial = veiculo.Quilometragem
        };

        Itens.Add(item);
        ValorTotal = Itens.Sum(i => i.CalcularSubtotal());
        return item;
    }

    public void AplicarDesconto(decimal valor) => Desconto = valor;

    public decimal CalcularSaldoDevedor()
    {
        var multas = Devolucao?.Multas
            .Where(multa => multa.Status != StatusMulta.Cancelada)
            .Sum(multa => multa.Valor) ?? 0;

        return ValorTotal + multas - Desconto - Pagamentos.Sum(pagamento => pagamento.Valor);
    }

    public void Encerrar() => Status = StatusContrato.Encerrado;
}
