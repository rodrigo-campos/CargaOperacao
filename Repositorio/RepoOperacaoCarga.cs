using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CargaOperacao
{
    public class RepoOperacaoCarga : IRepoOperacao
    {
        private XDocument _doc;
        public RepoOperacaoCarga(XDocument doc)
        {
            _doc = doc;
        }
        public IEnumerable<string> ObterTodosCodigosPessoa()
        {
            return _doc.Root.Descendants().Where(i => i.Name == "CodigoContraparte").Select(c => c.Value).ToList();
        }
    }
}