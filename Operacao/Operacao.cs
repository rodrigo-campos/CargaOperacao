using System;
using System.Collections.Generic;
using System.Linq;

namespace CargaOperacao
{
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
        public bool PossuiCondicaoResgate => CondicoesResgate.Any();
    }
}