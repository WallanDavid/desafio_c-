using Domain;
using Repositories;

namespace Services
{
    public interface IPaymentCodeGenerator
    {
        string Generate(MetodoPagamento metodo);
    }

    public class PaymentCodeGenerator : IPaymentCodeGenerator
    {
        public string Generate(MetodoPagamento metodo)
        {
            return metodo switch
            {
                MetodoPagamento.BOLETO => GenerateBoletoLinhaDigitavel(),
                MetodoPagamento.PIX => GeneratePixChave(),
                _ => Guid.NewGuid().ToString()
            };
        }

        private string GenerateBoletoLinhaDigitavel()
        {
            var rnd = new Random();
            string Block(int len) => string.Concat(Enumerable.Range(0, len).Select(_ => rnd.Next(0, 10).ToString()));
            return $"{Block(5)}.{Block(5)} {Block(5)}.{Block(6)} {Block(5)}.{Block(6)} {Block(1)} {Block(14)}";
        }

        private string GeneratePixChave()
        {
            return $"PIX-{Guid.NewGuid().ToString("N").Substring(0, 32)}";
        }
    }

    public interface IPagamentoService
    {
        Pagamento RegistrarPagamento(int cobrancaId, decimal valor, DateTime dataPagamento);
    }

    public class PagamentoService : IPagamentoService
    {
        private readonly InMemoryStore _db;
        private readonly ICobrancaRepository _cobrancas;
        public PagamentoService(InMemoryStore db, ICobrancaRepository cobrancas)
        {
            _db = db;
            _cobrancas = cobrancas;
        }

        public Pagamento RegistrarPagamento(int cobrancaId, decimal valor, DateTime dataPagamento)
        {
            var c = _cobrancas.Get(cobrancaId) ?? throw new InvalidOperationException("Cobrança não encontrada");
            if (c.Status == StatusCobranca.CANCELADA) throw new InvalidOperationException("Não é permitido pagar uma cobrança CANCELADA");
            var pagamento = new Pagamento(_db.NextPagamentoId(), cobrancaId, valor, dataPagamento);
            c.Pagamentos.Add(pagamento);
            c.Status = StatusCobranca.PAGA;
            _cobrancas.Update(c);
            _db.Pagamentos[pagamento.Id] = pagamento;
            return pagamento;
        }
    }
}
