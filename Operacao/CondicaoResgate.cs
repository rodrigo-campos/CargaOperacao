using System;

namespace CargaOperacao
{
    public class CondicaoResgate
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; internal set; }
        public decimal PercentualIndexador { get; internal set; }
        public decimal Taxa { get; internal set; }
    }
}