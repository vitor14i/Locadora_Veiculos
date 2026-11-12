using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.Exemplos;

/// <summary>
/// TEMPORÁRIO: dados fixos para testar as telas enquanto os controllers não ficam prontos.
/// Os veículos seguem o DbSeeder. Apagar esta pasta ao ligar as telas aos controllers.
/// </summary>
public static class DadosDeExemplo
{
    // A ordem das listas importa: cada uma usa as que foram declaradas antes.
    private static readonly DateTime Hoje = DateTime.Today;

    public static List<ClienteExemplo> Clientes { get; } =
    [
        new(1, "Maria Silva", "CPF 111.111.111-11", "maria@email.com", "(27) 99999-1111"),
        new(2, "João Souza", "CPF 222.222.222-22", "joao@email.com", "(27) 99999-2222"),
        new(3, "Transportes Capixaba Ltda", "CNPJ 11.222.333/0001-44", "contato@capixaba.com", "(27) 3333-4444")
    ];

    /// <summary>Cliente usado pelo login temporário de cliente.</summary>
    public static ClienteExemplo ClienteAtual => Clientes[0];

    public static List<Veiculo> Frota { get; } =
    [
        new Carro { Id = 1, Placa = "ABC1D23", Marca = "Fiat", Modelo = "Mobi", Ano = 2023, Quilometragem = 12000,
                    ValorDiaria = 120, NumeroPortas = 4, Cambio = TipoCambio.Manual, Categoria = "Econômico" },
        new Carro { Id = 2, Placa = "DEF2E34", Marca = "Chevrolet", Modelo = "Onix", Ano = 2024, Quilometragem = 8000,
                    ValorDiaria = 150, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "Compacto",
                    Status = StatusVeiculo.Alugado },
        new Carro { Id = 3, Placa = "GHI3F45", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Quilometragem = 20000,
                    ValorDiaria = 250, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "Sedan",
                    Status = StatusVeiculo.Alugado },
        new Carro { Id = 4, Placa = "JKL4G56", Marca = "Jeep", Modelo = "Compass", Ano = 2024, Quilometragem = 5000,
                    ValorDiaria = 320, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "SUV" },
        new Moto { Id = 5, Placa = "MNO5H67", Marca = "Honda", Modelo = "CG 160", Ano = 2023, Quilometragem = 9000,
                   ValorDiaria = 80, Cilindradas = 160 },
        new Moto { Id = 6, Placa = "PQR6I78", Marca = "Yamaha", Modelo = "MT-03", Ano = 2024, Quilometragem = 3000,
                   ValorDiaria = 130, Cilindradas = 321 }
    ];

    public static List<Atendente> Atendentes { get; } =
    [
        new() { Id = 2, Nome = "Ana Costa", Email = "ana@frotago.com", Matricula = "ATD-001" },
        new() { Id = 3, Nome = "Bruno Lima", Email = "bruno@frotago.com", Matricula = "ATD-002" }
    ];

    public static List<Manutencao> Manutencoes { get; } =
    [
        new() { Id = 1, VeiculoId = 5, Veiculo = Frota[4], Descricao = "Troca de óleo e revisão",
                DataInicio = Hoje.AddDays(-15).AddHours(9), DataFim = Hoje.AddDays(-14).AddHours(17) }
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

    // ---------- Veículos ----------

    /// <summary>Veículos que podem aparecer na busca, ou seja, fora de manutenção.</summary>
    public static List<VeiculoItem> Veiculos => Frota
        .Where(veiculo => veiculo.Status != StatusVeiculo.EmManutencao)
        .Select(CriarItem)
        .ToList();

    /// <summary>Converte o model em item da busca, usando a regra de tarifa do próprio model.</summary>
    public static VeiculoItem CriarItem(Veiculo veiculo) => new()
    {
        VeiculoId = veiculo.Id,
        Tipo = veiculo is Moto ? "Moto" : "Carro",
        Categoria = veiculo switch
        {
            Carro carro => carro.Categoria,
            Moto moto => $"{moto.Cilindradas} cc",
            _ => string.Empty
        },
        Nome = $"{veiculo.Marca} {veiculo.Modelo} · {veiculo.Ano}",
        Diaria = veiculo.Calcular(1)
    };

    public static Veiculo VeiculoDaFrota(int veiculoId) => Frota.First(veiculo => veiculo.Id == veiculoId);

    public static void SalvarVeiculo(Veiculo veiculo)
    {
        if (veiculo.Id != 0) return; // Na edição, o objeto da lista já foi alterado.

        veiculo.Id = ProximoId(Frota.Select(item => item.Id));
        Frota.Add(veiculo);
    }

    // ---------- Manutenções ----------

    public static void IniciarManutencao(Veiculo veiculo, string descricao)
    {
        Manutencoes.Add(new Manutencao
        {
            Id = ProximoId(Manutencoes.Select(item => item.Id)),
            Veiculo = veiculo,
            VeiculoId = veiculo.Id,
            Descricao = descricao,
            DataInicio = DateTime.Now
        });
        veiculo.Status = StatusVeiculo.EmManutencao;
    }

    public static void FinalizarManutencao(Manutencao manutencao)
    {
        manutencao.Finalizar(DateTime.Now);
        if (manutencao.Veiculo is not null)
        {
            manutencao.Veiculo.Status = StatusVeiculo.Disponivel;
        }
    }

    // ---------- Pessoas ----------

    public static void AdicionarAtendente(Atendente atendente)
    {
        atendente.Id = ProximoId(Atendentes.Select(item => item.Id));
        Atendentes.Add(atendente);
    }

    public static void AdicionarCliente(string nome, string documento, string email, string telefone) =>
        Clientes.Add(new ClienteExemplo(ProximoId(Clientes.Select(item => item.Id)), nome, documento, email, telefone));

    // ---------- Reservas e locações ----------

    public static ReservaExemplo Buscar(int reservaId) =>
        Reservas.First(reserva => reserva.Id == reservaId);

    public static IEnumerable<ReservaExemplo> ReservasDoClienteAtual() =>
        Reservas.Where(reserva => reserva.Cliente == ClienteAtual);

    public static ReservaExemplo Agendar(ClienteExemplo cliente, VeiculoItem veiculo,
                                         DateTime retirada, DateTime devolucao)
    {
        var reserva = new ReservaExemplo(ProximoId(Reservas.Select(item => item.Id)), "Agendada", cliente,
                                         veiculo.ParaPeriodo(retirada, devolucao), retirada, devolucao);
        Reservas.Add(reserva);
        return reserva;
    }

    public static void Cancelar(int reservaId) =>
        Atualizar(reservaId, reserva => reserva with { Status = "Cancelada" });

    /// <summary>Converte a reserva em locação, marca o veículo como alugado e retorna o número do contrato.</summary>
    public static int IniciarLocacao(int reservaId, decimal kmSaida)
    {
        var contrato = (Reservas.Max(reserva => reserva.Contrato) ?? 1000) + 1;
        VeiculoDaFrota(Buscar(reservaId).Veiculo.VeiculoId).Status = StatusVeiculo.Alugado;
        Atualizar(reservaId, reserva => reserva with { Status = "Ativa", Contrato = contrato, KmSaida = kmSaida });
        return contrato;
    }

    /// <summary>Encerra a locação e libera o veículo ou o envia para manutenção.</summary>
    public static void ConcluirDevolucao(int reservaId, DateTime dataDevolucao, bool enviarParaManutencao)
    {
        var veiculo = VeiculoDaFrota(Buscar(reservaId).Veiculo.VeiculoId);
        if (enviarParaManutencao)
        {
            IniciarManutencao(veiculo, "Revisão após devolução");
        }
        else
        {
            veiculo.Status = StatusVeiculo.Disponivel;
        }

        Atualizar(reservaId, reserva => reserva with { Status = "Concluída", Devolucao = dataDevolucao });
    }

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

    /// <summary>Locações, ativas ou concluídas, com retirada dentro do período.</summary>
    public static List<ReservaExemplo> LocacoesNoPeriodo(DateTime inicio, DateTime fim) => Reservas
        .Where(reserva => reserva.Contrato is not null &&
                          reserva.Retirada.Date >= inicio.Date && reserva.Retirada.Date <= fim.Date)
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

    // ---------- Auxiliares ----------

    private static ReservaExemplo Criar(int id, string status, int clienteId, int veiculoId,
                                        DateTime retirada, DateTime devolucao,
                                        int? contrato = null, decimal? kmSaida = null)
    {
        var cliente = Clientes.First(item => item.Id == clienteId);
        var veiculo = CriarItem(VeiculoDaFrota(veiculoId)).ParaPeriodo(retirada, devolucao);
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

    private static int ProximoId(IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;
}

/// <summary>TEMPORÁRIO: cliente de exemplo.</summary>
public record ClienteExemplo(int Id, string Nome, string Documento, string Email = "", string Telefone = "")
{
    public string Descricao => $"{Nome} · {Documento}";
}

/// <summary>TEMPORÁRIO: reserva ou locação de exemplo.</summary>
public record ReservaExemplo(int Id, string Status, ClienteExemplo Cliente, VeiculoItem Veiculo,
                             DateTime Retirada, DateTime Devolucao,
                             int? Contrato = null, decimal? KmSaida = null);