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

    public static List<ClienteExemplo> Clientes { get; } =
    [
        new(1, "Maria Silva", "CPF 111.111.111-11"),
        new(2, "João Souza", "CPF 222.222.222-22"),
        new(3, "Transportes Capixaba Ltda", "CNPJ 11.222.333/0001-44")
    ];

    /// <summary>Cliente usado pelo login temporário de cliente.</summary>
    public static ClienteExemplo ClienteAtual => Clientes[0];

    public static List<VeiculoItem> Veiculos { get; } =
    [
        new() { VeiculoId = 1, Tipo = "Carro", Categoria = "Econômico", Nome = "Fiat Mobi · 2023", Diaria = 120m },
        new() { VeiculoId = 2, Tipo = "Carro", Categoria = "Compacto", Nome = "Chevrolet Onix · 2024", Diaria = 165m },
        new() { VeiculoId = 3, Tipo = "Carro", Categoria = "Sedan", Nome = "Toyota Corolla · 2023", Diaria = 275m },
        new() { VeiculoId = 4, Tipo = "Carro", Categoria = "SUV", Nome = "Jeep Compass · 2024", Diaria = 352m },
        new() { VeiculoId = 5, Tipo = "Moto", Categoria = "160 cc", Nome = "Honda CG 160 · 2023", Diaria = 80m },
        new() { VeiculoId = 6, Tipo = "Moto", Categoria = "321 cc", Nome = "Yamaha MT-03 · 2024", Diaria = 130m }
    ];

    // Parâmetros: id, status, cliente, veículo, retirada, devolução prevista.
    public static List<ReservaExemplo> Reservas { get; } =
    [
        Criar(1024, "Ativa", 1, 2, Hoje.AddDays(-1).AddHours(14), Hoje.AddDays(2).AddHours(14), contrato: 1024, kmSaida: 8000),
        Criar(1025, "Agendada", 1, 4, Hoje.AddDays(3).AddHours(15), Hoje.AddDays(5).AddHours(15)),
        Criar(1010, "Concluída", 1, 1, Hoje.AddDays(-20).AddHours(9), Hoje.AddDays(-17).AddHours(9), contrato: 1010, kmSaida: 11500),
        Criar(1012, "Cancelada", 1, 6, Hoje.AddDays(-10).AddHours(10), Hoje.AddDays(-8).AddHours(10)),
        Criar(1020, "Ativa", 2, 3, Hoje.AddDays(-4).AddHours(9), Hoje.AddDays(-1).AddHours(9), contrato: 1020, kmSaida: 19500),
        Criar(1026, "Agendada", 2, 5, Hoje.AddHours(10).AddMinutes(30), Hoje.AddDays(2).AddHours(10).AddMinutes(30)),
        Criar(1027, "Agendada", 3, 6, Hoje.AddHours(14), Hoje.AddDays(3).AddHours(14))
    ];

    public static ReservaExemplo Buscar(int reservaId) =>
        Reservas.First(reserva => reserva.Id == reservaId);

    public static IEnumerable<ReservaExemplo> ReservasDoClienteAtual() =>
        Reservas.Where(reserva => reserva.Cliente == ClienteAtual);

    public static ReservaExemplo Agendar(ClienteExemplo cliente, VeiculoItem veiculo,
                                         DateTime retirada, DateTime devolucao)
    {
        var reserva = new ReservaExemplo(Reservas.Max(item => item.Id) + 1, "Agendada", cliente,
                                         veiculo.ParaPeriodo(retirada, devolucao), retirada, devolucao);
        Reservas.Add(reserva);
        return reserva;
    }

    public static void Cancelar(int reservaId) =>
        Atualizar(reservaId, reserva => reserva with { Status = "Cancelada" });

    /// <summary>Converte a reserva em locação e retorna o número do contrato.</summary>
    public static int IniciarLocacao(int reservaId, decimal kmSaida)
    {
        var contrato = (Reservas.Max(reserva => reserva.Contrato) ?? 1000) + 1;
        Atualizar(reservaId, reserva => reserva with { Status = "Ativa", Contrato = contrato, KmSaida = kmSaida });
        return contrato;
    }

    public static void ConcluirDevolucao(int reservaId, DateTime dataDevolucao) =>
        Atualizar(reservaId, reserva => reserva with { Status = "Concluída", Devolucao = dataDevolucao });

    public static List<VisitaItem> VisitasDeHoje() => Reservas
        .Where(reserva => reserva.Status == "Agendada" && reserva.Retirada.Date == DateTime.Today)
        .OrderBy(reserva => reserva.Retirada)
        .Select(reserva => new VisitaItem
        {
            ReservaId = reserva.Id,
            Horario = reserva.Retirada.ToString("HH:mm"),
            Cliente = reserva.Cliente.Nome,
            Veiculo = reserva.Veiculo.Nome,
            Status = reserva.Status
        })
        .ToList();

    public static List<LocacaoItem> LocacoesAtivas() => Reservas
        .Where(reserva => reserva.Status == "Ativa")
        .OrderBy(reserva => reserva.Devolucao)
        .Select(reserva => new LocacaoItem
        {
            ReservaId = reserva.Id,
            Contrato = $"nº {reserva.Contrato}",
            Cliente = reserva.Cliente.Nome,
            Veiculo = reserva.Veiculo.Nome,
            DevolucaoPrevista = FormatoHelper.DiaEHora(reserva.Devolucao),
            Status = reserva.Devolucao < DateTime.Now ? "Atrasada" : "Em dia"
        })
        .ToList();

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

    private static ReservaExemplo Criar(int id, string status, int clienteId, int veiculoId,
                                        DateTime retirada, DateTime devolucao,
                                        int? contrato = null, decimal? kmSaida = null)
    {
        var cliente = Clientes.First(item => item.Id == clienteId);
        var veiculo = Veiculos.First(item => item.VeiculoId == veiculoId).ParaPeriodo(retirada, devolucao);
        return new ReservaExemplo(id, status, cliente, veiculo, retirada, devolucao, contrato, kmSaida);
    }

    private static void Atualizar(int reservaId, Func<ReservaExemplo, ReservaExemplo> alterar)
    {
        var indice = Reservas.FindIndex(reserva => reserva.Id == reservaId);
        if (indice >= 0)
        {
            Reservas[indice] = alterar(Reservas[indice]);
        }
    }
}

/// <summary>TEMPORÁRIO: cliente de exemplo.</summary>
public record ClienteExemplo(int Id, string Nome, string Documento)
{
    public string Descricao => $"{Nome} · {Documento}";
}

/// <summary>TEMPORÁRIO: reserva ou locação de exemplo.</summary>
public record ReservaExemplo(int Id, string Status, ClienteExemplo Cliente, VeiculoItem Veiculo,
                             DateTime Retirada, DateTime Devolucao,
                             int? Contrato = null, decimal? KmSaida = null);