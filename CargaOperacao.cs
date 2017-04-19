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

namespace CargaOperacao
{
    public class CargaOperacao
    {
        private static void Main()
        {

            Console.WriteLine("Started.");
            const string path = @"E:\CargaEmissaoCDB_CDI.xml";

            IEnumerable<Operacao> operacoes = null;

            var carga0 = new CargaOperacaoLinqToXML(path);

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to parse.", () =>
            {
                operacoes = carga0.CarregarOperacoes();
            });


            var carga = new CargaOperacaoXPathDocument(path);

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to parse.", () =>
            {
                operacoes = carga.CarregarOperacoes();
            });

            var carga2 = new CargaOperacaoXDocument(path);

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to parse.", () =>
            {
                operacoes = carga2.CarregarOperacoes();
            });

            var repo = new RepoOperacaoCarga(XDocument.Load(path));

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to validate.", () =>
            {
                var validator = new OperacaoValidator(repo);

                foreach (var op in operacoes)
                {
                    validator.ValidateAndThrow(op);
                }
            });

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }

    public class CargaOperacaoLinqToXML : ICargaOperacao
    {
        XDocument doc = null;
        public CargaOperacaoLinqToXML(string path)
        {
            doc = XDocument.Load(path);
        }
        public IEnumerable<Operacao> CarregarOperacoes()
        {
            doc.Root.Elements("SeniorSolution").AsParallel()
                .Select(x => new Operacao()
                {
                    CodigoExterno = x.Value
                });

            throw new NotImplementedException();
        }
    }

    public class CargaOperacaoXPathDocument : ICargaOperacao
    {
        XPathDocument doc = null;
        XPathNodeIterator headers = null;
        XPathNodeIterator bodies = null;
        public CargaOperacaoXPathDocument(string path)
        {
            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to load.", () =>
            {
                doc = new XPathDocument(path);

                var nav = doc.CreateNavigator();

                var headerExp = nav.Compile("Start/SeniorSolution/Header");
                headers = nav.Select(headerExp);

                var bodyExp = nav.Compile("Start/SeniorSolution/Body/NEGRFPRV0002");
                bodies = nav.Select(bodyExp);
            });            
        }
        enum TipoConversao
        {
            String,
            Decimal,
            DateTime,
            Bool,
            Int
        }

        //Dictionary<string, TipoConversao> TiposConversao = new Dictionary<string, TipoConversao>()
        //{
        //    { "Sending_App_Message_ID", TipoConversao.String }
        //};

        //public Type ObterTipo(string path)
        //{
        //    switch (TiposConversao[path])
        //    {
        //        case TipoConversao.String: return "".GetType();
        //    }

        //    return null;
        //}

        public T ObterValorHeader<T>(string path)
        {
            return (T)Convert.ChangeType(headers.Current.SelectSingleNode(path)?.Value, typeof(T));
        }

        public string ObterValorOperacao(string path)
        {
            return ObterValorOperacao<string>(path);
        }
        public T ObterValorOperacao<T>(string path)
        {
            var format = CultureInfo.InvariantCulture;
            var value = bodies.Current.SelectSingleNode(path)?.Value;

            //switch(Type.GetTypeCode(value.GetType()))
            //{
            //    case TypeCode.Boolean:  break;
            //}

            if (typeof(T) == typeof(bool))
            {
                value = value.ToBool().ToString();
            } else if (typeof(T).GetTypeInfo().IsEnum)
            {
                return (T)Enum.Parse(typeof(T), value);
            }

            return (T)Convert.ChangeType(value, typeof(T), format);
        }
        public IEnumerable<Operacao> CarregarOperacoes()
        {
            headers.MoveNext();
            bodies.MoveNext();

            var operacoes = new List<Operacao>();

            while (headers.MoveNext())
            {
                bodies.MoveNext();

                operacoes.Add(new Operacao()
                {
                    CodigoExterno = ObterValorHeader<string>("Sending_App_Message_ID"),
                    VeiculoLegal = new PessoaSimples(ObterValorOperacao("VeiculoLegal"), ObterValorOperacao("CodigoVeiculoLegal")),
                    Contraparte = new PessoaSimples(ObterValorOperacao("Contraparte"), ObterValorOperacao("CodigoContraparte")),
                    TipoContraparte = ObterValorOperacao<TipoContraparte>("TipoContraparte"),
                    TipoOperacao = ObterValorOperacao<TipoOperacao>("TipoOperacao"),
                    EmissaoPrimaria = ObterValorOperacao<bool>("FlagEmissaoPrimaria"),
                    LocalCustodia = ObterValorOperacao<LocalCustodia>("LocalCustodiaSubsistema"),
                    DataMovimento = ObterValorOperacao<DateTime>("DataMovimento"),
                    DataInicio = ObterValorOperacao<DateTime>("DataInicio"),
                    DataVencimento = ObterValorOperacao<DateTime>("DataVencimento"),
                    DataLiquidez = ObterValorOperacao<DateTime>("DataLiquidez"),
                    Valor = ObterValorOperacao<decimal>("Valor"),
                    Indexador = ObterValorOperacao<Indexador>("Indexador"),
                    PercentualIndexador = ObterValorOperacao<decimal>("PercIndexador"),
                    Taxa = ObterValorOperacao<decimal>("TaxaNegociada"),
                    Produto = new ProdutoSimples(ObterValorOperacao("Produto")),
                    LocalLiquidacao = ObterValorOperacao<LocalLiquidacao>("LocalLiquidacao"),
                    ModalidadeLiquidacao = ObterValorOperacao<ModalidadeLiquidacao>("ModalidadeLiquidacao"),
                    FormaLiquidacao = ObterValorOperacao<FormaLiquidacao>("FormaLiquidacao"),
                    StatusOperacao = ObterValorOperacao<StatusOperacao>("StatusOperacao"),
                    CodigoAtivo = ObterValorOperacao("CodigoAtivo"),
                    PUEmissao = ObterValorOperacao<decimal>("PUEmissao"),
                    Quantidade = ObterValorOperacao<decimal>("Quantidade"),
                    //CondicoesResgate = from c in bodies.Current.Select("CondicoesResgate/CondicaoResgate")
                                       //    .Select(condicao => new CondicaoResgate()
                                       //    {
                                       //        DataInicio = condicao.GetValue("CondResgDataInicio").ToDateTime(),
                                       //        DataFim = condicao.GetValue("CondResgDataFim").ToDateTime(),
                                       //        PercentualIndexador = condicao.GetValue("CondResgPercIndexador").ToDecimal(),
                                       //        Taxa = condicao.GetValue("CondResgTaxa252").ToDecimal()
                                       //    })
                });
            }

            return operacoes;
        }
    }

    public class CargaOperacaoXDocument : ICargaOperacao
    {
        XDocument xml = null;

        public CargaOperacaoXDocument(string path)
        {
            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to load.", () =>
            {
                xml = XDocument.Load(path);
            });
        }
        public IEnumerable<Operacao> CarregarOperacoes()
        {
            IEnumerable<Operacao> operacoes = null;

            

            if (xml?.Root == null)
                throw new Exception("XML inválido");

            operacoes = xml.Root.Descendants().Where(i => i.Name == "SeniorSolution").AsParallel()
                .Select(item =>
                new Operacao()
                {
                    CodigoExterno = item.GetValue("Sending_App_Message_ID"),
                    VeiculoLegal = new PessoaSimples(item.GetValue("VeiculoLegal"), item.GetValue("CodigoVeiculoLegal")),
                    Contraparte = new PessoaSimples(item.GetValue("Contraparte"), item.GetValue("CodigoContraparte")),
                    TipoContraparte = (TipoContraparte)Enum.Parse(typeof(TipoContraparte), item.GetValue("TipoContraparte")),
                    TipoOperacao = (TipoOperacao)Enum.Parse(typeof(TipoOperacao), item.GetValue("TipoOperacao")),
                    EmissaoPrimaria = item.GetValue("FlagEmissaoPrimaria").ToBool(),
                    LocalCustodia = (LocalCustodia)Enum.Parse(typeof(LocalCustodia), item.GetValue("LocalCustodiaSubsistema")),
                    DataMovimento = item.GetValue("DataMovimento").ToDateTime(),
                    DataInicio = item.GetValue("DataInicio").ToDateTime(),
                    DataVencimento = item.GetValue("DataVencimento").ToDateTime(),
                    DataLiquidez = item.GetValue("DataLiquidez").ToDateTime(),
                    Valor = item.GetValue("Valor").ToDecimal(),
                    Indexador = (Indexador)Enum.Parse(typeof(Indexador), item.GetValue("Indexador")),
                    PercentualIndexador = item.GetValue("PercIndexador").ToDecimal(),
                    Taxa = item.GetValue("TaxaNegociada").ToDecimal(),
                    Produto = new ProdutoSimples(item.GetValue("Produto")),
                    LocalLiquidacao = (LocalLiquidacao)Enum.Parse(typeof(LocalLiquidacao), item.GetValue("LocalLiquidacao")),
                    ModalidadeLiquidacao = (ModalidadeLiquidacao)Enum.Parse(typeof(ModalidadeLiquidacao), item.GetValue("ModalidadeLiquidacao")),
                    FormaLiquidacao = (FormaLiquidacao)Enum.Parse(typeof(FormaLiquidacao), item.GetValue("FormaLiquidacao")),
                    StatusOperacao = (StatusOperacao)Enum.Parse(typeof(StatusOperacao), item.GetValue("StatusOperacao")),
                    CodigoAtivo = item.GetValue("CodigoAtivo"),
                    PUEmissao = item.GetValue("PUEmissao").ToDecimal(),
                    Quantidade = item.GetValue("Quantidade").ToDecimal(),
                    //CondicoesResgate = item.Descendants().Where(i => i.Name == "CondicaoResgate")
                    //.Select(condicao => new CondicaoResgate()
                    //{
                    //    DataInicio = condicao.GetValue("CondResgDataInicio").ToDateTime(),
                    //    DataFim = condicao.GetValue("CondResgDataFim").ToDateTime(),
                    //    PercentualIndexador = condicao.GetValue("CondResgPercIndexador").ToDecimal(),
                    //    Taxa = condicao.GetValue("CondResgTaxa252").ToDecimal()
                    //})
                }).ToList();

            return operacoes;
        }
    }
}