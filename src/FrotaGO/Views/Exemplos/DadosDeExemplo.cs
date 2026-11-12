using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.Exemplos;

/// <summary>
/// TEMPORÁRIO: dados fixos para testar as telas enquanto os controllers não ficam prontos.
/// Os veículos seguem o DbSeeder, com a diária já calculada pela regra de tarifa.
/// Apagar esta pasta ao ligar as telas aos controllers.
/// </summary>
public static class DadosDeExemplo
{
    private static readonly DateTime Hoje = DateTime.Today;

    public static List<VeiculoItem> Veiculos { get; } =
    [
        new() { VeiculoId = 1, Tipo = "Carro", Categoria = "Econômico", Nome = "Fiat Mobi · 2023", Diaria = 120m },
        new() { VeiculoId = 2, Tipo = "Carro", Categoria = "Compacto", Nome = "Chevrolet Onix · 2024", Diaria = 165m },
        new() { VeiculoId = 3, Tipo = "Carro", Categoria = "Sedan", Nome = "Toyota Corolla · 2023", Diaria = 275m },
        new() { VeiculoId = 4, Tipo = "Carro", Categoria = "SUV", Nome = "Jeep Compass · 2024", Diaria = 352m },
        new() { VeiculoId = 5, Tipo = "Moto", Categoria = "160 cc", Nome = "Honda CG 160 · 2023", Diaria = 80m },
        new() { VeiculoId = 6, Tipo = "Moto", Categoria = "321 cc", Nome = "Yamaha MT-03 · 2024", Diaria = 130m }
    ];

    public static List<ReservaExemplo> Reservas { get; } =
    [
        Criar(1024, "Ativa", 2, Hoje.AddDays(-1).AddHours(14), Hoje.AddDays(2).AddHours(14), contrato: "1024"),
        Criar(1025, "Agendada", 4, Hoje.AddDays(3).AddHours(15), Hoje.AddDays(5).AddHours(15)),
        Criar(1010, "Concluída", 1, Hoje.AddDays(-20).AddHours(9), Hoje.AddDays(-17).AddHours(9), contrato: "1010"),
        Criar(1012, "Cancelada", 6, Hoje.AddDays(-10).AddHours(10), Hoje.AddDays(-8).AddHours(10))
    ];

    public static ReservaExemplo Agendar(VeiculoItem veiculo, DateTime retirada, DateTime devolucao)
    {
        var reserva = new ReservaExemplo(Reservas.Max(item => item.Id) + 1, "Agendada",
                                         veiculo.ParaPeriodo(retirada, devolucao), retirada, devolucao);
        Reservas.Add(reserva);
        return reserva;
    }

    public static void Cancelar(int reservaId)
    {
        var indice = Reservas.FindIndex(item => item.Id == reservaId);
        if (indice >= 0)
        {
            Reservas[indice] = Reservas[indice] with { Status = "Cancelada" };
        }
    }

    public static CardReservaItem CriarCard(ReservaExemplo reserva) => reserva.Status switch
    {
        "Ativa" => new()
        {
            ReservaId = reserva.Id,
            Titulo = "Locação",
            Status = reserva.Status,
            Veiculo = reserva.Veiculo.Nome,
            Detalhe1 = $"Devolução: {FormatoHelper.DiaEHora(reserva.Devolucao)}",
            Detalhe2 = "Pagamento: confirmado",
            Detalhe3 = $"Contrato: nº {reserva.Contrato}"
        },
        "Concluída" => new()
        {
            ReservaId = reserva.Id,
            Titulo = "Locação",
            Status = reserva.Status,
            Veiculo = reserva.Veiculo.Nome,
            Detalhe1 = $"Devolvido em: {FormatoHelper.DiaEHora(reserva.Devolucao)}",
            Detalhe3 = $"Valor final: {FormatoHelper.Moeda(reserva.Veiculo.Total)}"
        },
        _ => new()
        {
            ReservaId = reserva.Id,
            Titulo = "Reserva",
            Status = reserva.Status,
            Veiculo = reserva.Veiculo.Nome,
            Detalhe1 = $"Retirada: {FormatoHelper.DiaEHora(reserva.Retirada)}",
            Detalhe2 = $"Devolução: {FormatoHelper.DiaEHora(reserva.Devolucao)}",
            Detalhe3 = $"Valor estimado: {FormatoHelper.Moeda(reserva.Veiculo.Total)}"
        }
    };

    private static ReservaExemplo Criar(int id, string status, int veiculoId, DateTime retirada,
                                        DateTime devolucao, string? contrato = null)
    {
        var veiculo = Veiculos.First(item => item.VeiculoId == veiculoId).ParaPeriodo(retirada, devolucao);
        return new ReservaExemplo(id, status, veiculo, retirada, devolucao, contrato);
    }
}

/// <summary>TEMPORÁRIO: reserva de exemplo usada pelas telas do cliente.</summary>
public record ReservaExemplo(int Id, string Status, VeiculoItem Veiculo,
                             DateTime Retirada, DateTime Devolucao, string? Contrato = null);
