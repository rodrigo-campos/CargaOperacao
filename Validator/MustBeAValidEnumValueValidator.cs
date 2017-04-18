using System;
using FluentValidation.Validators;

namespace CargaOperacao
{
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
}