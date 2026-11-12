using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;

/// <summary>Linha da tabela de veículos.</summary>
public record LinhaVeiculo(int Id, string Placa, string Veiculo, string Tipo, string Diaria, string Status);
