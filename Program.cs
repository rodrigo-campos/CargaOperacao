using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using FluentValidation;
using System.Globalization;
using System.Diagnostics;
using FluentValidation.Validators;
using System.Threading;
using System.Threading.Tasks;

namespace CargaOperacao
{
    public class PessoaSimples
    {
        public string Nome;
        public string CodigoExterno;

        public PessoaSimples(string nome, string codigo)
        {
            Nome = nome;
            CodigoExterno = codigo;
        }
    }

    public enum TipoContraparte
    {
        MERCADO = 1,
        CLIENTE_UM = 2,
        MERCADO_COLIGADA = 4,
        CLIENTE_LIGADO = 5
    }

    public enum TipoOperacao
    {
        EMISSAO = 103
    }

    public enum LocalCustodia
    {
        CETIP = 6
    }

    public enum Indexador
    {
        PRE = 1,
        DI = 7,
        IPCA = 14
    }

    public enum LocalLiquidacao
    {
        CETIP = 2
    }

    public enum ModalidadeLiquidacao
    {
        BRUTA = 1,
        SEM_MODALIDADE = 4
    }

    public enum FormaLiquidacao
    {
        STR = 1,
        TESOURARIA = 4
    }

    public enum StatusOperacao
    {
        EFETIVADA = 7
    }

    public class Operacao
    {
        public string CodigoExterno { get; set; }
        public PessoaSimples VeiculoLegal { get; set; }
        public PessoaSimples Contraparte { get; set; }
        public TipoContraparte TipoContraparte { get; set; }
        public TipoOperacao TipoOperacao { get; set; }
        public bool EmissaoPrimaria { get; set; }
        public LocalCustodia LocalCustodia { get; set; }
        public DateTime DataMovimento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime DataLiquidez { get; set; }
        public decimal Valor { get; set; }
        public Indexador Indexador { get; set; }
        public decimal PercentualIndexador { get; set; }
        public decimal Taxa { get; set; }
        public ProdutoSimples Produto { get; set; }
        public LocalLiquidacao LocalLiquidacao { get; internal set; }
        public ModalidadeLiquidacao ModalidadeLiquidacao { get; internal set; }
        public FormaLiquidacao FormaLiquidacao { get; set; }
        public StatusOperacao StatusOperacao { get; internal set; }
        public string CodigoAtivo { get; internal set; }
        public decimal PUEmissao { get; internal set; }
        public decimal Quantidade { get; internal set; }
        public IEnumerable<CondicaoResgate> CondicoesResgate { get; internal set; }
        public bool PossuiCondicaoResgate => CondicoesResgate.Count() > 0;
    }

    public class CondicaoResgate
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; internal set; }
        public decimal PercentualIndexador { get; internal set; }
        public decimal Taxa { get; internal set; }
    }

    public class Util
    {
        public static void WithWatch(Func<Stopwatch, string> messageFn, Action action)
        {
            var watch = new Stopwatch();
            watch.Start();

            action();

            watch.Stop();
            Console.WriteLine(messageFn(watch) ?? string.Empty);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started.");
            var operacoesXml = @"E:\CargaEmissaoCDB_CDI.xml";

            XDocument xml = null;

            Util.WithWatch(w => $"Took {w.ElapsedMilliseconds}ms to load.", () =>
            {
                xml = XDocument.Load(operacoesXml);
            });

            var watch = new Stopwatch();
            watch.Start();

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

            watch.Restart();

            var repo = new RepoOperacaoCarga(xml);
            var validator = new OperacaoValidator(repo);

            foreach (var op in operacoes)
            {
                validator.ValidateAndThrow(op);
            }

            watch.Stop();

            Console.WriteLine($"Took {watch.ElapsedMilliseconds / 1000}s to validate.");

            var opToDebug = operacoes.Where(op => op.PossuiCondicaoResgate).First();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }

    public interface IRepoOperacao
    {
        IEnumerable<string> ObterTodosCodigosPessoa();
    }

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

    public class OperacaoValidator : AbstractValidator<Operacao>
    {
        HashSet<string> _hsContrapartes = null;
        HashSet<string> _hsVeiculosLegais = null;
        public OperacaoValidator(IRepoOperacao repo)
        {
            _hsContrapartes = new HashSet<string>(repo.ObterTodosCodigosPessoa());

            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(op => op.CodigoExterno).NotNull().NotEmpty().WithMessage("Código externo não informado");
            //RuleFor(op => op.VeiculoLegal).NotNull().Must(BeAValidVeiculoLegal);
            RuleFor(op => op.Contraparte).NotNull().Must(BeAValidContraparte);
            RuleFor(op => op.TipoContraparte).MustHaveAValidEnumValue();
            RuleFor(op => op.TipoOperacao).MustHaveAValidEnumValue();
            RuleFor(op => op.Indexador).MustHaveAValidEnumValue();
            RuleFor(op => op.DataInicio).LessThanOrEqualTo(op => op.DataMovimento);
            RuleFor(op => op.DataMovimento).LessThanOrEqualTo(op => op.DataVencimento);
            RuleFor(op => op.Valor).GreaterThan(0);
            RuleFor(op => op.LocalLiquidacao).MustHaveAValidEnumValue();
            RuleFor(op => op.ModalidadeLiquidacao).MustHaveAValidEnumValue();
            RuleFor(op => op.FormaLiquidacao).MustHaveAValidEnumValue();
            RuleFor(op => op.StatusOperacao).MustHaveAValidEnumValue();
            RuleFor(op => op.PUEmissao).GreaterThan(0).When(op => op.TipoOperacao.Equals(TipoOperacao.EMISSAO));
            RuleFor(op => op.Quantidade).GreaterThan(0);
            RuleFor(op => op.CondicoesResgate).SetCollectionValidator(new CondicaoResgateValidator())
                .DependentRules(d =>
                {
                    d.RuleFor(op => op.CondicoesResgate).Must(CobrirDataInicioADataVencimento);
                    d.RuleFor(op => new DatasCondicaoPOCO
                    {
                        DataMovimentoOperacao = op.DataMovimento,
                        DataVencimentoOperacao = op.DataVencimento,
                        DataInicioCondicao = op.CondicoesResgate.Min(c => c.DataInicio),
                        DataFimCondicao = op.CondicoesResgate.Max(c => c.DataFim)
                    }).Must(CompreenderDoInicioAoFimDaOperacao);
                }).When(op => op.PossuiCondicaoResgate);
        }

        private bool CompreenderDoInicioAoFimDaOperacao(DatasCondicaoPOCO arg)
        {
            return arg.DataInicioCondicao == arg.DataMovimentoOperacao &&
                arg.DataFimCondicao == arg.DataVencimentoOperacao;
        }

        private bool CobrirDataInicioADataVencimento(IEnumerable<CondicaoResgate> condicoes)
        {
            var enumerator = condicoes.OrderBy(c => c.DataInicio).GetEnumerator();
            enumerator.MoveNext();

            var anterior = enumerator.Current;
            var atual = anterior;

            while (enumerator.MoveNext())
            {
                atual = enumerator.Current;

                if (anterior.DataFim >= atual.DataInicio)
                    return false;

                anterior = atual;
            }

            return true;
        }

        private bool BeAValidVeiculoLegal(PessoaSimples vl)
        {
            return _hsVeiculosLegais.Contains(vl.CodigoExterno);
        }

        private bool BeAValidContraparte(PessoaSimples ctp)
        {
            return _hsContrapartes.Contains(ctp.CodigoExterno);
        }

        private class DatasCondicaoPOCO
        {
            public DateTime DataMovimentoOperacao { get; set; }
            public DateTime DataVencimentoOperacao { get; set; }
            public DateTime DataInicioCondicao { get; set; }
            public DateTime DataFimCondicao { get; set; }
        }
    }

    public class CondicaoResgateValidator : AbstractValidator<CondicaoResgate>
    {
        public CondicaoResgateValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(c => c.DataInicio).LessThan(c => c.DataFim);
            RuleFor(c => c.DataFim).GreaterThan(c => c.DataInicio);
        }
    }

    public class MustBeAValidEnumValueValidator<T> : PropertyValidator
    {
        public MustBeAValidEnumValueValidator()
            : base("{PropertyValue} não é um ID válido para {PropertyName}")
        {

        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            return Enum.IsDefined(context.PropertyValue.GetType(), context.PropertyValue);
        }
    }

    public static class OperacaoExtensionsValidator
    {
        public static IRuleBuilderOptions<T, TElement> MustHaveAValidEnumValue<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MustBeAValidEnumValueValidator<TElement>());
        }
    }

    public class ProdutoSimples
    {
        public string Codigo { get; set; }

        public ProdutoSimples(string codigo)
        {
            Codigo = codigo;
        }
    }

    public static class AuxExtensions
    {
        public static bool ToBool(this string value)
        {
            return (value.Trim().Equals("S") || value.Trim().Equals("1"));
        }

        public static DateTime ToDateTime(this string value)
        {
            return DateTime.Parse(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        public static string GetValue(this XElement item, string element)
        {
            if (item.Descendants().Elements().Count() > 0)
                return item.Descendants().Elements().First(i => i.Name == element).Value;
            else
                return item.Descendants().First(i => i.Name == element).Value;
        }

    }
}