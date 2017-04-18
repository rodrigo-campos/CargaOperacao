using System.Collections.Generic;

namespace CargaOperacao
{
    public interface IRepoOperacao
    {
        IEnumerable<string> ObterTodosCodigosPessoa();
    }
}