using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Data;

/// <summary>Popula o banco com dados iniciais quando ele está vazio.</summary>
public static class DbSeeder
{
    // Segue a regra de senha (mínimo de 8 caracteres).
    private const string SenhaPadrao = "Frotago@123";

    public static void Seed(FrotaGODbContext db)
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
}
