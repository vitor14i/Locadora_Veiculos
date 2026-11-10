using Locadora_Veiculos.Enums;
using Locadora_Veiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace Locadora_Veiculos.Data;

/// <summary>
/// Contexto de acesso aos dados da locadora por meio do Entity Framework Core.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Cria o contexto para uso por ferramentas de design-time.</summary>
    public AppDbContext()
    {
    }

    /// <summary>Cria o contexto com opções fornecidas por DI ou pelos testes.</summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Usuários cadastrados no sistema.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Clientes cadastrados no sistema.</summary>
    public DbSet<Cliente> Clientes => Set<Cliente>();

    /// <summary>Clientes pessoa física.</summary>
    public DbSet<PessoaFisica> PessoasFisicas => Set<PessoaFisica>();

    /// <summary>Clientes pessoa jurídica.</summary>
    public DbSet<PessoaJuridica> PessoasJuridicas => Set<PessoaJuridica>();

    /// <summary>Gerentes cadastrados no sistema.</summary>
    public DbSet<Gerente> Gerentes => Set<Gerente>();

    /// <summary>Atendentes cadastrados no sistema.</summary>
    public DbSet<Atendente> Atendentes => Set<Atendente>();

    /// <summary>Veículos cadastrados no sistema.</summary>
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();

    /// <summary>Carros cadastrados no sistema.</summary>
    public DbSet<Carro> Carros => Set<Carro>();

    /// <summary>Motos cadastradas no sistema.</summary>
    public DbSet<Moto> Motos => Set<Moto>();

    /// <summary>Contratos de locação cadastrados no sistema.</summary>
    public DbSet<Contrato> Contratos => Set<Contrato>();

    /// <summary>
    /// Configura a conexão padrão com o banco de dados LocalDB.
    /// </summary>
    /// <param name="optionsBuilder">Construtor das opções do contexto.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LocadoraDB;Integrated Security=True");
        }
    }

    /// <summary>
    /// Configura índices e dados iniciais do modelo.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo do EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasDiscriminator<string>("TipoUsuario")
            .HasValue<PessoaFisica>("PessoaFisica")
            .HasValue<PessoaJuridica>("PessoaJuridica")
            .HasValue<Gerente>("Gerente")
            .HasValue<Atendente>("Atendente");

        modelBuilder.Entity<Veiculo>()
            .HasDiscriminator<string>("TipoVeiculo")
            .HasValue<Carro>("Carro")
            .HasValue<Moto>("Moto");

        modelBuilder.Entity<Veiculo>().ToTable("Veiculos");
        modelBuilder.Entity<Contrato>().ToTable("Contratos");
        modelBuilder.Entity<Contrato>()
            .Property(contrato => contrato.ValorTotal)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Contrato>()
            .Property(contrato => contrato.ValorMulta)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Veiculo>()
            .Property(veiculo => veiculo.RowVersion)
            .IsRowVersion();

        // Filtros globais aplicam o soft delete; consultas administrativas podem usar IgnoreQueryFilters().
        modelBuilder.Entity<Usuario>().HasQueryFilter(usuario => usuario.Ativo);
        modelBuilder.Entity<Veiculo>().HasQueryFilter(veiculo => veiculo.Ativo);
        modelBuilder.Entity<Contrato>().HasQueryFilter(contrato => contrato.Ativo);

        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        modelBuilder.Entity<PessoaFisica>()
            .HasIndex(pessoa => pessoa.CPF)
            .IsUnique();

        modelBuilder.Entity<PessoaJuridica>()
            .HasIndex(pessoa => pessoa.CNPJ)
            .IsUnique();

        modelBuilder.Entity<Gerente>().HasData(new Gerente
        {
            Id = 1,
            Email = "gerente@locadora.com",
            // Hash gerado pelo PasswordHasher<Usuario> para a senha de demonstração gerente123.
            SenhaHash = "AQAAAAIAAYagAAAAEDnedMq6Om2UphMbxhfZJsz44KzMLl3TAtzAbroAMj9cAPbkRXFwXfNeoGS58Tmmrg==",
            Ativo = true,
            Perfil = TipoPerfil.Gerente
        });
    }
}