using FluentValidation;

namespace CargaOperacao
{
    public static class OperacaoExtensionsValidator
    {
        public static IRuleBuilderOptions<T, TElement> MustHaveAValidEnumValue<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MustBeAValidEnumValueValidator<TElement>());
        }
    }
}