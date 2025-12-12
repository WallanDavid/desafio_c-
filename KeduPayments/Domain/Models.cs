namespace Domain
{
    public enum MetodoPagamento { BOLETO, PIX }
    public enum StatusCobranca { EMITIDA, PAGA, CANCELADA }

    public record Responsavel(int Id, string Nome);
    public record CentroDeCusto(int Id, string Nome);

    public class PlanoDePagamento
    {
        public int Id { get; set; }
        public int ResponsavelId { get; set; }
        public int CentroDeCustoId { get; set; }
        public List<Cobranca> Cobrancas { get; set; } = new();
        public decimal ValorTotal => Cobrancas.Sum(c => c.Valor);
    }

    public class Cobranca
    {
        public int Id { get; set; }
        public int PlanoDePagamentoId { get; set; }
        public decimal Valor { get; set; }
        public DateOnly DataVencimento { get; set; }
        public MetodoPagamento MetodoPagamento { get; set; }
        public StatusCobranca Status { get; set; } = StatusCobranca.EMITIDA;
        public string CodigoPagamento { get; set; } = string.Empty;
        public List<Pagamento> Pagamentos { get; set; } = new();

        public bool EstaVencida(DateOnly hoje)
            => Status != StatusCobranca.PAGA && Status != StatusCobranca.CANCELADA && hoje > DataVencimento;
    }

    public record Pagamento(int Id, int CobrancaId, decimal Valor, DateTime DataPagamento);
}
