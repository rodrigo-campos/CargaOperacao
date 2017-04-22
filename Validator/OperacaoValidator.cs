using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CargaOperacao
{
    public class OperacaoValidator : AbstractValidator<Operacao>
    {
        HashSet<string> _hsContrapartes = null;
        HashSet<string> _hsVeiculosLegais = null;        
        public OperacaoValidator(IRepoOperacao repo)
        {
            _hsContrapartes = new HashSet<string>(repo.ObterTodosCodigosPessoa());
            var _condicaoResgateValidator = new CondicaoResgateValidator();

            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(op => op.CodigoExterno).NotNull().NotEmpty().WithMessage("Código externo não informado");
            //RuleFor(op => op.VeiculoLegal).NotNull().Must(BeAValidVeiculoLegal);
            RuleFor(op => op.Contraparte).NotNull().Must(ContraparteValida);
            RuleFor(op => op.TipoContraparte).IsInEnum();
            RuleFor(op => op.TipoOperacao).IsInEnum();
            RuleFor(op => op.Indexador).IsInEnum();
            RuleFor(op => op.DataInicio).LessThanOrEqualTo(op => op.DataMovimento);
            RuleFor(op => op.DataMovimento).LessThanOrEqualTo(op => op.DataVencimento);
            RuleFor(op => op.Valor).GreaterThan(0);
            RuleFor(op => op.LocalLiquidacao).IsInEnum();
            RuleFor(op => op.ModalidadeLiquidacao).IsInEnum();
            RuleFor(op => op.FormaLiquidacao).IsInEnum();
            RuleFor(op => op.StatusOperacao).IsInEnum();
            RuleFor(op => op.PUEmissao).GreaterThan(0).When(op => op.TipoOperacao.Equals(TipoOperacao.Emissao));
            RuleFor(op => op.Quantidade).GreaterThan(0);
            RuleFor(op => op.CondicoesResgate).SetCollectionValidator(_condicaoResgateValidator)
                .DependentRules(d =>
                {
                    d.RuleFor(op => op.CondicoesResgate).Must(CobrirDataInicioADataVencimento);
                    d.RuleFor(op => new DatasCondicaoPOCO
                    {
                        DataMovimentoOperacao = op.DataMovimento,
                        DataVencimentoOperacao = op.DataVencimento,
                        DataInicioCondicao = op.CondicoesResgate.Min(c => c.DataInicio),
                        DataFimCondicao = op.CondicoesResgate.Max(c => c.DataFim)
                    }).Must(PeriodoCondicaoResgateIgualAoPeriodoEmissao);
                }).When(op => op.PossuiCondicaoResgate);
        }

        private bool PeriodoCondicaoResgateIgualAoPeriodoEmissao(DatasCondicaoPOCO arg)
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

        private bool ContraparteValida(PessoaSimples ctp)
        {
            return _hsContrapartes.Contains(ctp.CodigoExterno);
        }

        private struct DatasCondicaoPOCO
        {
            public DateTime DataMovimentoOperacao { get; set; }
            public DateTime DataVencimentoOperacao { get; set; }
            public DateTime DataInicioCondicao { get; set; }
            public DateTime DataFimCondicao { get; set; }
        }
    }
}