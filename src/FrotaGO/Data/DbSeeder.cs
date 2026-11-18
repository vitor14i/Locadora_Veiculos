using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Data;

/// <summary>Popula o banco com dados iniciais. Cada etapa só roda quando a parte dela ainda está vazia.</summary>
public static class DbSeeder
{
    // Segue a regra de senha (mínimo de 8 caracteres).
    private const string SenhaPadrao = "Frotago@123";

    private const string ObservacaoDevolucao = "Devolvido sem avarias.";

    public static void Seed(FrotaGODbContext db)
    {
        SeedUsuariosEVeiculos(db);
        SeedLocacoes(db);
    }

    private static void SeedUsuariosEVeiculos(FrotaGODbContext db)
    {
        if (db.Usuarios.Any()) return;

        var hash = PasswordHasher.Hash(SenhaPadrao);

        db.Usuarios.AddRange(
            new Gerente { Nome = "Carlos Gerente", Email = "gerente@frotago.com", SenhaHash = hash, Matricula = "G001" },
            new Atendente { Nome = "Ana Atendente", Email = "ana@frotago.com", SenhaHash = hash, Matricula = "A001" },
            new Atendente { Nome = "Bruno Atendente", Email = "bruno@frotago.com", SenhaHash = hash, Matricula = "A002" },
            new PessoaFisica
            {
                Nome = "Maria Silva", Email = "maria@email.com", SenhaHash = hash, Telefone = "(27) 99999-0001",
                Endereco = "Rua das Flores, 100 - Vitória/ES", Cpf = "11111111111",
                DataNascimento = new DateTime(1990, 5, 10), NumeroCnh = "12345678900"
            },
            new PessoaFisica
            {
                Nome = "João Souza", Email = "joao@email.com", SenhaHash = hash, Telefone = "(27) 99999-0002",
                Endereco = "Av. Central, 200 - Vila Velha/ES", Cpf = "22222222222",
                DataNascimento = new DateTime(1985, 8, 22), NumeroCnh = "98765432100"
            },
            new PessoaJuridica
            {
                Nome = "Transportes Capixaba", Email = "contato@capixaba.com", SenhaHash = hash,
                Telefone = "(27) 3333-0003", Endereco = "Rod. BR-101, km 5 - Serra/ES",
                Cnpj = "11111111000111", RazaoSocial = "Transportes Capixaba LTDA"
            });

        db.Veiculos.AddRange(
            new Carro { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Mobi", Ano = 2023, Quilometragem = 12000, ValorDiaria = 120, NumeroPortas = 4, Cambio = TipoCambio.Manual, Categoria = "Econômico" },
            new Carro { Placa = "DEF2E34", Marca = "Chevrolet", Modelo = "Onix", Ano = 2024, Quilometragem = 8000, ValorDiaria = 150, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "Compacto" },
            new Carro { Placa = "GHI3F45", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Quilometragem = 20000, ValorDiaria = 250, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "Sedan" },
            new Carro { Placa = "JKL4G56", Marca = "Jeep", Modelo = "Compass", Ano = 2024, Quilometragem = 5000, ValorDiaria = 320, NumeroPortas = 4, Cambio = TipoCambio.Automatico, Categoria = "SUV" },
            new Moto { Placa = "MNO5H67", Marca = "Honda", Modelo = "CG 160", Ano = 2023, Quilometragem = 9000, ValorDiaria = 80, Cilindradas = 160 },
            new Moto { Placa = "PQR6I78", Marca = "Yamaha", Modelo = "MT-03", Ano = 2024, Quilometragem = 3000, ValorDiaria = 130, Cilindradas = 321 });

        db.SaveChanges();
    }

    /// <summary>
    /// Locações dos últimos 30 dias, para o relatório ter conteúdo. Roda também em um banco já
    /// populado, desde que ele ainda não tenha contratos. As datas contam a partir do dia em que roda.
    /// Grava tudo com um único SaveChanges().
    /// </summary>
    private static void SeedLocacoes(FrotaGODbContext db)
    {
        if (db.Contratos.Any()) return;

        // Clientes e veículos do SeedUsuariosEVeiculos. Rastreados, porque o km e o status mudam.
        var maria = db.Clientes.FirstOrDefault(cliente => cliente.Email == "maria@email.com");
        var joao = db.Clientes.FirstOrDefault(cliente => cliente.Email == "joao@email.com");
        var capixaba = db.Clientes.FirstOrDefault(cliente => cliente.Email == "contato@capixaba.com");
        var onix = db.Veiculos.FirstOrDefault(veiculo => veiculo.Placa == "DEF2E34");
        var corolla = db.Veiculos.FirstOrDefault(veiculo => veiculo.Placa == "GHI3F45");
        var cg160 = db.Veiculos.FirstOrDefault(veiculo => veiculo.Placa == "MNO5H67");
        var compass = db.Veiculos.FirstOrDefault(veiculo => veiculo.Placa == "JKL4G56");

        // Se algum deles foi apagado, ou se o veículo da locação ativa não está livre, não cria nenhuma locação.
        if (maria is null || joao is null || capixaba is null ||
            onix is null || corolla is null || cg160 is null || compass is null ||
            compass.Status != StatusVeiculo.Disponivel)
        {
            return;
        }

        var hoje = DateTime.Today;

        // Em ordem cronológica: cada locação sai com o km deixado pela anterior do mesmo veículo.
        // Os períodos de um mesmo veículo não se sobrepõem.
        var locacao1 = CriarLocacao(db, maria, onix, hoje.AddDays(-28).AddHours(10), dias: 3, NovoCartao("Visa", "4821"));
        Devolver(locacao1, hoje.AddDays(-25).AddHours(9).AddMinutes(30), kmRodados: 450);

        var locacao2 = CriarLocacao(db, capixaba, corolla, hoje.AddDays(-24).AddHours(9), dias: 4, NovoPix("PIX-SEED-0001"));
        Devolver(locacao2, hoje.AddDays(-20).AddHours(8).AddMinutes(45), kmRodados: 780);

        // Devolvida 1 dia depois do previsto: gera a multa de atraso paga, e o faturamento inclui multas.
        var locacao3 = CriarLocacao(db, joao, cg160, hoje.AddDays(-19).AddHours(14), dias: 2, NovoPix("PIX-SEED-0002"));
        Devolver(locacao3, hoje.AddDays(-16).AddHours(11), kmRodados: 380, pagamentoMulta: NovoPix("PIX-SEED-0003"));

        var locacao4 = CriarLocacao(db, capixaba, onix, hoje.AddDays(-14).AddHours(8), dias: 4, NovoCartao("Mastercard", "5590"));
        Devolver(locacao4, hoje.AddDays(-10).AddHours(7).AddMinutes(40), kmRodados: 750);

        var locacao5 = CriarLocacao(db, maria, corolla, hoje.AddDays(-8).AddHours(10), dias: 3, NovoPix("PIX-SEED-0004"));
        Devolver(locacao5, hoje.AddDays(-5).AddHours(9).AddMinutes(50), kmRodados: 540);

        // Ativa: começou há 2 dias e termina daqui a 2, então aparece Em dia e pode ser devolvida.
        // O status muda direto, como no LocacaoController, porque o model não tem método para isso.
        CriarLocacao(db, maria, compass, hoje.AddDays(-2).AddHours(10), dias: 4, NovoCartao("Visa", "7310"));
        compass.Status = StatusVeiculo.Alugado;

        db.SaveChanges();
    }

    /// <summary>
    /// Reserva convertida com o contrato e o pagamento das diárias, como no LocacaoController:
    /// o item grava a diária com a regra do tipo de veículo, e o valor pago é o total do contrato.
    /// </summary>
    private static Contrato CriarLocacao(FrotaGODbContext db, Cliente cliente, Veiculo veiculo,
                                         DateTime retirada, int dias, Pagamento pagamento)
    {
        var reserva = new Reserva
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            DataCriacao = retirada.AddDays(-2),
            DataInicio = retirada,
            DataFim = retirada.AddDays(dias)
        };
        reserva.Confirmar();

        var contrato = new Contrato { Reserva = reserva, DataGeracao = retirada };
        contrato.AdicionarItem(veiculo, reserva.CalcularDias());

        pagamento.Valor = contrato.ValorTotal;
        pagamento.DataPagamento = contrato.DataGeracao;
        contrato.Pagamentos.Add(pagamento);

        db.Contratos.Add(contrato);
        return contrato;
    }

    /// <summary>
    /// Devolução sem avarias, com uma vistoria por item, como no DevolucaoController. Com atraso,
    /// cria a multa já paga e o pagamento dela. No fim, encerra o contrato e atualiza o km do veículo.
    /// </summary>
    private static void Devolver(Contrato contrato, DateTime dataDevolucao, decimal kmRodados,
                                 Pagamento? pagamentoMulta = null)
    {
        var devolucao = new Devolucao { DataDevolucao = dataDevolucao, Observacoes = ObservacaoDevolucao };
        contrato.Devolucao = devolucao;

        foreach (var item in contrato.Itens)
        {
            var kmChegada = item.KmInicial + kmRodados;
            devolucao.Vistorias.Add(new Vistoria
            {
                ItemContrato = item,
                KmFinal = kmChegada,
                TemAvarias = false,
                Observacoes = ObservacaoDevolucao
            });
            item.Veiculo!.Quilometragem = kmChegada;
        }

        // Mesma regra e mesmo formato de motivo do DevolucaoController: uma diária por dia de atraso.
        var diasAtraso = devolucao.CalcularDiasAtraso(contrato.Reserva!.DataFim);
        if (diasAtraso > 0)
        {
            var pagamento = pagamentoMulta
                ?? throw new ArgumentException("Uma devolução com atraso precisa do pagamento da multa.");

            var multa = new Multa
            {
                Motivo = $"Atraso · {FormatoHelper.Quantidade(diasAtraso, "dia", "dias")}",
                Valor = diasAtraso * contrato.Itens.Sum(item => item.ValorDiaria)
            };
            multa.Pagar();
            devolucao.Multas.Add(multa);

            pagamento.Valor = multa.Valor;
            pagamento.DataPagamento = dataDevolucao;
            contrato.Pagamentos.Add(pagamento);
        }

        contrato.Encerrar();
    }

    private static Cartao NovoCartao(string bandeira, string ultimosDigitos) =>
        new() { Bandeira = bandeira, UltimosDigitos = ultimosDigitos, Parcelas = 1 };

    private static Pix NovoPix(string codigoTransacao) => new() { CodigoTransacao = codigoTransacao };
}
