using Locadora_Veiculos.Controllers;
using Locadora_Veiculos.Data;
using Locadora_Veiculos.Enums;
using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Locadora_Veiculos.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Locadora_Veiculos.Tests;

public class LocadoraFluxoTests
{
    [Theory]
    [InlineData(7, 840)]
    [InlineData(8, 864)]
    public void Carro_UsaDiariaEAplicaDesconto(int dias, decimal esperado)
    {
        var calculadora = CriarCalculadoraCarro();

        var total = calculadora.Calcular(100m, dias);

        Assert.Equal(esperado, total);
    }

    [Fact]
    public void CalculadoraMoto_IncluiTaxaDeCapacete()
    {
        var calculadora = new CalculadoraTarifaMoto(Options.Create(new TarifasConfig()));

        var total = calculadora.Calcular(100m, 2);

        Assert.Equal(220m, total);
    }

    [Fact]
    public void Carro_RejeitaDiariaOuDuracaoInvalidas()
    {
        var calculadora = CriarCalculadoraCarro();

        Assert.Throws<ArgumentOutOfRangeException>(() => calculadora.Calcular(0m, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => calculadora.Calcular(100m, 0));
    }

    [Fact]
    public async Task LoginAsync_AceitaHashValidoDoGerenteSeed()
    {
        var factory = await CriarFactoryAsync();
        var controller = new AuthController(factory, new PasswordHasher<Usuario>());

        var usuario = await controller.LoginAsync("gerente@locadora.com", "gerente123");

        Assert.IsType<Gerente>(usuario);
    }

    [Fact]
    public async Task LoginAsync_RejeitaSenhaIncorreta()
    {
        var factory = await CriarFactoryAsync();
        var controller = new AuthController(factory, new PasswordHasher<Usuario>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => controller.LoginAsync("gerente@locadora.com", "senhaErrada123"));
    }

    [Fact]
    public async Task RegistrarContratoAsync_UsaDiariaDoVeiculoEAtualizaStatus()
    {
        var factory = await CriarFactoryAsync();
        var cliente = CriarCliente();
        var carro = CriarCarro();
        await using (var contexto = await factory.CreateDbContextAsync())
        {
            contexto.AddRange(cliente, carro);
            await contexto.SaveChangesAsync();
        }

        using var provider = CriarTarifaProvider();
        var controller = new LocacaoController(factory, provider.GetRequiredService<ITarifaResolver>());
        var contrato = await controller.RegistrarContratoAsync(carro.Id, cliente.Id, 7);

        await using var leitura = await factory.CreateDbContextAsync();
        var carroSalvo = await leitura.Veiculos.SingleAsync(item => item.Id == carro.Id);

        Assert.Equal(840m, contrato.ValorTotal);
        Assert.Equal(StatusVeiculo.Locado, carroSalvo.Status);
        Assert.Contains(carro.Id, contrato.Veiculos.Select(item => item.Id));
        Assert.Equal(840m, contrato.ContratoVeiculos.Single().ValorLocacao);
        Assert.Equal(cliente.Id, contrato.ClienteId);
    }

    [Fact]
    public async Task RegistrarContratoAsync_NaoPermiteVeiculoInativo()
    {
        var factory = await CriarFactoryAsync();
        var cliente = CriarCliente();
        var carro = CriarCarro();
        carro.Ativo = false;
        await using (var contexto = await factory.CreateDbContextAsync())
        {
            contexto.AddRange(cliente, carro);
            await contexto.SaveChangesAsync();
        }

        using var provider = CriarTarifaProvider();
        var controller = new LocacaoController(factory, provider.GetRequiredService<ITarifaResolver>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.RegistrarContratoAsync(carro.Id, cliente.Id, 1));
    }

    [Fact]
    public void CalculadoraMulta_UsaPercentualConfiguravelPorDia()
    {
        var options = Options.Create(new TarifasConfig { PercentualMultaAtrasoPorDia = 0.25m });
        var calculadora = new CalculadoraMultaAtraso(options);

        var multa = calculadora.Calcular(100m, 2);

        Assert.Equal(50m, multa);
    }

    [Fact]
    public async Task DevolucaoAtrasada_CalculaMultaEDeixaVeiculoDisponivel()
    {
        var factory = await CriarFactoryAsync();
        var cliente = CriarCliente();
        var carro = CriarCarro();
        carro.Status = StatusVeiculo.Locado;
        var dataInicio = DateTime.UtcNow.AddDays(-3);
        var contrato = new Contrato
        {
            Id = 10,
            DataInicio = dataInicio,
            DiasLocacao = 1,
            ValorTotal = 100m
        };
        contrato.AssociarCliente(cliente);
        contrato.AdicionarVeiculo(carro, 100m);

        await using (var contexto = await factory.CreateDbContextAsync())
        {
            contexto.AddRange(cliente, carro, contrato);
            await contexto.SaveChangesAsync();
        }

        var calculadoraMulta = new CalculadoraMultaAtraso(Options.Create(new TarifasConfig()));
        var controller = new DevolucaoController(factory, calculadoraMulta);
        var devolucao = await controller.RegistrarDevolucaoAsync(contrato.Id, dataInicio.AddDays(3));

        await using var leitura = await factory.CreateDbContextAsync();
        var carroSalvo = await leitura.Veiculos.SingleAsync(item => item.Id == carro.Id);

        Assert.Equal(2, devolucao.DiasAtraso);
        Assert.Equal(40m, devolucao.ValorMulta);
        Assert.Equal(StatusVeiculo.Disponivel, carroSalvo.Status);
    }

    private static Carro CriarCalculadoraCarro()
    {
        return new Carro(Options.Create(new TarifasConfig()))
        {
            Placa = "ABC1D23",
            Marca = "Marca",
            Modelo = "Modelo",
            Categoria = "Carro"
        };
    }

    private static ServiceProvider CriarTarifaProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions<TarifasConfig>();
        services.AddKeyedTransient<ITarifa, Carro>(typeof(Carro));
        services.AddKeyedTransient<ITarifa, CalculadoraTarifaMoto>(typeof(Moto));
        services.AddTransient<ITarifaResolver, TarifaResolver>();
        return services.BuildServiceProvider();
    }

    private static async Task<TestDbContextFactory> CriarFactoryAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new TestDbContextFactory(options);
        await using var contexto = await factory.CreateDbContextAsync();
        await contexto.Database.EnsureCreatedAsync();
        return factory;
    }

    private static PessoaFisica CriarCliente()
    {
        return new PessoaFisica
        {
            Id = 2,
            Email = "cliente@locadora.com",
            SenhaHash = "hash-de-teste",
            Nome = "Cliente de Teste",
            CPF = "12345678901",
            CNH = "12345678900",
            Perfil = TipoPerfil.Cliente
        };
    }

    private static Carro CriarCarro()
    {
        return new Carro
        {
            Id = 1,
            Placa = "ABC1D23",
            Marca = "Marca",
            Modelo = "Modelo",
            Categoria = "Carro",
            Ano = 2025,
            ValorDiaria = 100m,
            Status = StatusVeiculo.Disponivel
        };
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options)
        : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(options);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateDbContext());
        }
    }
}
