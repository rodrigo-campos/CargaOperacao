using FluentValidation;

namespace CargaOperacao
{
    public class CondicaoResgateValidator : AbstractValidator<CondicaoResgate>
    {
        public CondicaoResgateValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(c => c.DataInicio).LessThan(c => c.DataFim);
            RuleFor(c => c.DataFim).GreaterThan(c => c.DataInicio);
        }
    }
}