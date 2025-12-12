#if USE_EF
using Domain;
using Persistence;
using Repositories;

namespace Services
{
    public class PagamentoEfService : IPagamentoService
    {
        private readonly KeduDbContext _db;
        private readonly ICobrancaRepository _cobrancas;
        public PagamentoEfService(KeduDbContext db, ICobrancaRepository cobrancas)
        {
            _db = db;
            _cobrancas = cobrancas;
        }

        public Pagamento RegistrarPagamento(int cobrancaId, decimal valor, DateTime dataPagamento)
        {
            var c = _cobrancas.Get(cobrancaId) ?? throw new InvalidOperationException("Cobrança não encontrada");
            if (c.Status == StatusCobranca.CANCELADA) throw new InvalidOperationException("Não é permitido pagar uma cobrança CANCELADA");
            var id = (_db.Pagamentos.Any() ? _db.Pagamentos.Max(x => x.Id) : 0) + 1;
            var pagamento = new Pagamento(id, cobrancaId, valor, dataPagamento);
            c.Pagamentos.Add(pagamento);
            c.Status = StatusCobranca.PAGA;
            _db.Pagamentos.Add(pagamento);
            _cobrancas.Update(c);
            return pagamento;
        }
    }
}
#endif
