using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;

/// <summary>Linha da tabela de manutenções.</summary>
public record LinhaManutencao(int Id, string Veiculo, string Descricao, string Inicio, string Fim, string Status, bool EmAndamento);
