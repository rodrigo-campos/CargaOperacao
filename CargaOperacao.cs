using System;
using System.Xml.Linq;
using System.Linq;
using FluentValidation;
using System.Diagnostics;
using static CargaOperacao.Util;

namespace CargaOperacao
{
    public class CargaOperacao
    {
        private static void Main()
        {
            Console.WriteLine("Started.");
            const string operacoesXml = @"E:\CargaEmissaoCDB_CDI.xml";

            XDocument xml = null;

            WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to load.", () =>
            {
                xml = XDocument.Load(operacoesXml);
            });

            var watch = new Stopwatch();
            watch.Start();

            if (xml?.Root == null)
                return;

            var operacoes = xml.Root.Descendants().Where(i => i.Name == "SeniorSolution").AsParallel()
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
                    CondicoesResgate = item.Descendants().Where(i => i.Name == "CondicaoResgate")
                    .Select(condicao => new CondicaoResgate()
                    {
                        DataInicio = condicao.GetValue("CondResgDataInicio").ToDateTime(),
                        DataFim = condicao.GetValue("CondResgDataFim").ToDateTime(),
                        PercentualIndexador = condicao.GetValue("CondResgPercIndexador").ToDecimal(),
                        Taxa = condicao.GetValue("CondResgTaxa252").ToDecimal()
                    })
                }).ToList();


            Console.WriteLine($"Took {watch.ElapsedMilliseconds}ms to parse.");


            WithWatch(w => $"Took {w.ElapsedMilliseconds}s to validate.", () =>
            {
                var repo = new RepoOperacaoCarga(xml);
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
}