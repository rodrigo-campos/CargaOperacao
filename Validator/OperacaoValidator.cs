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
}