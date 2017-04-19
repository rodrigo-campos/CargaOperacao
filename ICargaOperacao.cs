using System.Collections;
using System.Collections.Generic;

namespace CargaOperacao
{
    public interface ICargaOperacao
    {
        IEnumerable<Operacao> CarregarOperacoes();
    }
}