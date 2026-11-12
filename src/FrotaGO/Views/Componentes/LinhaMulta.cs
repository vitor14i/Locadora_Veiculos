using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views.Componentes;
public record LinhaMulta(string Chave, string Motivo, string Valor, bool Cancelada, bool PodeGerenciar)
{
    public string Acao => Cancelada ? "Restaurar" : "Cancelar";
}
