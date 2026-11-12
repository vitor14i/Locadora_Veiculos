using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;
public class LocacaoItem
{
    public int ReservaId { get; init; }
    public string Contrato { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Veiculo { get; init; } = string.Empty;
    public string DevolucaoPrevista { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}