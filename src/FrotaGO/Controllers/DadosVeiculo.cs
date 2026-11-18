using FrotaGO.Models.Enums;

namespace FrotaGO.Controllers;

/// <summary>
/// Campos do formulário de veículo, com os números já convertidos pela tela.
/// Os campos de carro ficam vazios (nulos ou texto vazio) quando o veículo é moto, e vice-versa.
/// </summary>
public record DadosVeiculo(
    bool EhCarro,
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    decimal Quilometragem,
    decimal ValorDiaria,
    string Categoria,
    int? NumeroPortas,
    TipoCambio? Cambio,
    int? Cilindradas);
