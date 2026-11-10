using FrotaGO.Models;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Data;

public class FrotaGODbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Manutencao> Manutencoes => Set<Manutencao>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ItemContrato> ItensContrato => Set<ItemContrato>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Devolucao> Devolucoes => Set<Devolucao>();
    public DbSet<Vistoria> Vistorias => Set<Vistoria>();
    public DbSet<Multa> Multas => Set<Multa>();

    // Subclasses concretas (TPH): precisam estar no modelo para o EF mapear a herança.
    public DbSet<Atendente> Atendentes => Set<Atendente>();
    public DbSet<Gerente> Gerentes => Set<Gerente>();
    public DbSet<PessoaFisica> PessoasFisicas => Set<PessoaFisica>();
    public DbSet<PessoaJuridica> PessoasJuridicas => Set<PessoaJuridica>();
    public DbSet<Carro> Carros => Set<Carro>();
    public DbSet<Moto> Motos => Set<Moto>();
    public DbSet<Cartao> Cartoes => Set<Cartao>();
    public DbSet<Pix> Pixes => Set<Pix>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=FrotaGO;Trusted_Connection=True;");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Precisão de todos os campos decimal.
        builder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Herança: TPH (padrão do EF) - Usuario, Veiculo e Pagamento.

        // Valores únicos
        mb.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        // CPF e CNPJ são salvos somente com dígitos (ver DocumentoHelper), por isso o tamanho é fixo.
        mb.Entity<PessoaFisica>().Property(p => p.Cpf).HasMaxLength(11);
        mb.Entity<PessoaJuridica>().Property(p => p.Cnpj).HasMaxLength(14);
        mb.Entity<PessoaFisica>().HasIndex(p => p.Cpf).IsUnique().HasFilter("[Cpf] IS NOT NULL");
        mb.Entity<PessoaJuridica>().HasIndex(p => p.Cnpj).IsUnique().HasFilter("[Cnpj] IS NOT NULL");
        mb.Entity<Veiculo>().HasIndex(v => v.Placa).IsUnique();

        // 1xN Cliente -> Reserva
        mb.Entity<Reserva>()
            .HasOne(r => r.Cliente).WithMany(c => c.Reservas)
            .HasForeignKey(r => r.ClienteId).OnDelete(DeleteBehavior.Restrict);

        // 1xN Veiculo -> Reserva
        mb.Entity<Reserva>()
            .HasOne(r => r.Veiculo).WithMany(v => v.Reservas)
            .HasForeignKey(r => r.VeiculoId).OnDelete(DeleteBehavior.Restrict);

        // 1x1 Reserva - Contrato
        mb.Entity<Contrato>()
            .HasOne(c => c.Reserva).WithOne(r => r.Contrato)
            .HasForeignKey<Contrato>(c => c.ReservaId).OnDelete(DeleteBehavior.Restrict);

        // 1x1 Contrato - Devolucao
        mb.Entity<Devolucao>()
            .HasOne(d => d.Contrato).WithOne(c => c.Devolucao)
            .HasForeignKey<Devolucao>(d => d.ContratoId).OnDelete(DeleteBehavior.Restrict);

        // 1x1 ItemContrato - Vistoria
        mb.Entity<Vistoria>()
            .HasOne(v => v.ItemContrato).WithOne(i => i.Vistoria)
            .HasForeignKey<Vistoria>(v => v.ItemContratoId).OnDelete(DeleteBehavior.Restrict);

        // NxN Contrato - Veiculo por meio de ItemContrato.
        // A chave primária de ItensContrato é explicitamente o Id (e não o par ContratoId+VeiculoId,
        // que é o padrão do EF para tabelas de junção), pois o item tem dados próprios (diária, dias, km).
        mb.Entity<Contrato>()
            .HasMany(c => c.Veiculos).WithMany(v => v.Contratos)
            .UsingEntity<ItemContrato>(
                right => right.HasOne(i => i.Veiculo).WithMany(v => v.ItensContrato)
                              .HasForeignKey(i => i.VeiculoId).OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne(i => i.Contrato).WithMany(c => c.Itens)
                            .HasForeignKey(i => i.ContratoId).OnDelete(DeleteBehavior.Cascade),
                join => join.HasKey(i => i.Id));

        // Composições (cascade): Contrato -> Pagamento, Devolucao -> Multa
        mb.Entity<Pagamento>()
            .HasOne(p => p.Contrato).WithMany(c => c.Pagamentos)
            .HasForeignKey(p => p.ContratoId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Multa>()
            .HasOne(m => m.Devolucao).WithMany(d => d.Multas)
            .HasForeignKey(m => m.DevolucaoId).OnDelete(DeleteBehavior.Cascade);

        // Devolucao -> Vistoria: Restrict para evitar múltiplos caminhos de cascade (via ItemContrato)
        mb.Entity<Vistoria>()
            .HasOne(v => v.Devolucao).WithMany(d => d.Vistorias)
            .HasForeignKey(v => v.DevolucaoId).OnDelete(DeleteBehavior.Restrict);

        // Manutencao: Veiculo 1xN e Gerente 1xN
        mb.Entity<Manutencao>()
            .HasOne(m => m.Veiculo).WithMany(v => v.Manutencoes)
            .HasForeignKey(m => m.VeiculoId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Manutencao>()
            .HasOne(m => m.Gerente).WithMany(g => g.Manutencoes)
            .HasForeignKey(m => m.GerenteId).OnDelete(DeleteBehavior.Restrict);
    }
}
