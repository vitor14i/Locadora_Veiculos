using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;
/// <summary>Linha da lista de visitas do dia.</summary>
public class VisitaItem
{
    public int ReservaId { get; init; }
    public string Horario { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Veiculo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
