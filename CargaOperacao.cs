using System;
using System.Xml.Linq;
using System.Linq;
using FluentValidation;
using System.Diagnostics;
using static CargaOperacao.Util;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;

namespace CargaOperacao
{
    public class CargaOperacao
    {
        private static void Main()
        {

            Console.WriteLine("Started.");
            const string path = @"E:\CargaEmissaoCDB_CDI.xml";

            IEnumerable<Operacao> operacoes = null;

            var carga = new CargaOperacaoXDocument(path);

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to parse.", () =>
            {
                operacoes = carga.CarregarOperacoes();
            });

            var repo = new RepoOperacaoCarga(XDocument.Load(path));

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to validate.", () =>
            {
                var validator = new OperacaoValidator(repo);
                foreach (var op in operacoes)
                {
                    validator.ValidateAndThrowAsync(op);
                }

            });

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }

    public class CargaOperacaoXDocument : ICargaOperacao
    {
        XDocument _xml = null;

        public CargaOperacaoXDocument(string path)
        {
            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to load.", () =>
            {
                _xml = XDocument.Load(path, LoadOptions.None);
            });
        }
        public IEnumerable<Operacao> CarregarOperacoes()
        {
            IEnumerable<Operacao> operacoes = null;

            if (_xml?.Root == null)
                throw new Exception("XML inválido");

            operacoes = _xml.Root.Descendants().Elements("SeniorSolution")
                .Select(o =>
                new Operacao()
                {
                    CodigoExterno = o.ObterValorHeader("Sending_App_Message_ID"),
                    VeiculoLegal = new PessoaSimples(o.ObterValorOperacao("VeiculoLegal"), o.ObterValorOperacao("CodigoVeiculoLegal")),
                    Contraparte = new PessoaSimples(o.ObterValorOperacao("Contraparte"), o.ObterValorOperacao("CodigoContraparte")),
                    TipoContraparte = o.ObterValorOperacao<TipoContraparte>("TipoContraparte"),
                    TipoOperacao = o.ObterValorOperacao<TipoOperacao>("TipoOperacao"),
                    EmissaoPrimaria = o.ObterValorOperacao<bool>("FlagEmissaoPrimaria"),
                    LocalCustodia = o.ObterValorOperacao<LocalCustodia>("LocalCustodiaSubsistema"),
                    DataMovimento = o.ObterValorOperacao<DateTime>("DataMovimento"),
                    DataInicio = o.ObterValorOperacao<DateTime>("DataInicio"),
                    DataVencimento = o.ObterValorOperacao<DateTime>("DataVencimento"),
                    DataLiquidez = o.ObterValorOperacao<DateTime>("DataLiquidez"),
                    Valor = o.ObterValorOperacao<decimal>("Valor"),
                    Indexador = o.ObterValorOperacao<Indexador>("Indexador"),
                    PercentualIndexador = o.ObterValorOperacao<decimal>("PercIndexador"),
                    Taxa = o.ObterValorOperacao<decimal>("TaxaNegociada"),
                    Produto = new ProdutoSimples(o.ObterValorOperacao("Produto")),
                    LocalLiquidacao = o.ObterValorOperacao<LocalLiquidacao>("LocalLiquidacao"),
                    ModalidadeLiquidacao = o.ObterValorOperacao<ModalidadeLiquidacao>("ModalidadeLiquidacao"),
                    FormaLiquidacao = o.ObterValorOperacao<FormaLiquidacao>("FormaLiquidacao"),
                    StatusOperacao = o.ObterValorOperacao<StatusOperacao>("StatusOperacao"),
                    CodigoAtivo = o.ObterValorOperacao("CodigoAtivo"),
                    PUEmissao = o.ObterValorOperacao<decimal>("PUEmissao"),
                    Quantidade = o.ObterValorOperacao<decimal>("Quantidade"),
                    CondicoesResgate = o.ObterCondicaoResgate()
                    .Select(condicao => new CondicaoResgate()
                    {
                        DataInicio = condicao.ObterValor<DateTime>("CondResgDataInicio"),
                        DataFim = condicao.ObterValor<DateTime>("CondResgDataFim"),
                        PercentualIndexador = condicao.ObterValor<decimal>("CondResgPercIndexador"),
                        Taxa = condicao.ObterValor<decimal>("CondResgTaxa252")
                    })
                }).ToList();

            return operacoes;
        }
    }
}