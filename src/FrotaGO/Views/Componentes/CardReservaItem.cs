using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;
/// <summary>Dados exibidos em um card de reserva ou locação.</summary>
public class CardReservaItem
{
    public int ReservaId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Veiculo { get; init; } = string.Empty;
    public string Detalhe1 { get; init; } = string.Empty;
    public string Detalhe2 { get; init; } = string.Empty;
    public string Detalhe3 { get; init; } = string.Empty;
}
