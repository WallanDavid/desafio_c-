using Domain;

namespace Repositories
{
    public class InMemoryStore
    {
        public Dictionary<int, Responsavel> Responsaveis { get; } = new();
        public Dictionary<int, CentroDeCusto> Centros { get; } = new();
        public Dictionary<int, PlanoDePagamento> Planos { get; } = new();
        public Dictionary<int, Cobranca> Cobrancas { get; } = new();
        public Dictionary<int, Pagamento> Pagamentos { get; } = new();

        private int _nextResp = 1, _nextCentro = 1, _nextPlano = 1, _nextCobranca = 1, _nextPagamento = 1;

        public int NextResponsavelId() => _nextResp++;
        public int NextCentroDeCustoId() => _nextCentro++;
        public int NextPlanoId() => _nextPlano++;
        public int NextCobrancaId() => _nextCobranca++;
        public int NextPagamentoId() => _nextPagamento++;
    }

    public interface IResponsavelRepository
    {
        Responsavel Add(string nome);
        Responsavel? Get(int id);
        IEnumerable<PlanoDePagamento> GetPlanosByResponsavel(int id);
        IEnumerable<Cobranca> GetCobrancasByResponsavel(int id);
    }

    public interface ICentroDeCustoRepository
    {
        CentroDeCusto Add(string nome);
        IEnumerable<CentroDeCusto> GetAll();
        CentroDeCusto? Get(int id);
    }

    public interface IPlanoRepository
    {
        PlanoDePagamento Add(PlanoDePagamento plano);
        PlanoDePagamento? Get(int id);
        IEnumerable<PlanoDePagamento> GetByResponsavel(int responsavelId);
    }

    public interface ICobrancaRepository
    {
        Cobranca Add(Cobranca c);
        Cobranca? Get(int id);
        IEnumerable<Cobranca> GetByResponsavel(int responsavelId);
        void Update(Cobranca c);
    }

    public class InMemoryResponsavelRepository : IResponsavelRepository
    {
        private readonly InMemoryStore _db;
        public InMemoryResponsavelRepository(InMemoryStore db) => _db = db;
        public Responsavel Add(string nome)
        {
            var id = _db.NextResponsavelId();
            var r = new Responsavel(id, nome);
            _db.Responsaveis[id] = r;
            return r;
        }
        public Responsavel? Get(int id) => _db.Responsaveis.GetValueOrDefault(id);
        public IEnumerable<PlanoDePagamento> GetPlanosByResponsavel(int id) => _db.Planos.Values.Where(p => p.ResponsavelId == id);
        public IEnumerable<Cobranca> GetCobrancasByResponsavel(int id)
            => _db.Cobrancas.Values.Where(c => _db.Planos.TryGetValue(c.PlanoDePagamentoId, out var p) && p.ResponsavelId == id);
    }

    public class InMemoryCentroDeCustoRepository : ICentroDeCustoRepository
    {
        private readonly InMemoryStore _db;
        public InMemoryCentroDeCustoRepository(InMemoryStore db) => _db = db;
        public CentroDeCusto Add(string nome)
        {
            var id = _db.NextCentroDeCustoId();
            var c = new CentroDeCusto(id, nome);
            _db.Centros[id] = c;
            return c;
        }
        public IEnumerable<CentroDeCusto> GetAll() => _db.Centros.Values.OrderBy(c => c.Id);
        public CentroDeCusto? Get(int id) => _db.Centros.GetValueOrDefault(id);
    }

    public class InMemoryPlanoRepository : IPlanoRepository
    {
        private readonly InMemoryStore _db;
        public InMemoryPlanoRepository(InMemoryStore db) => _db = db;
        public PlanoDePagamento Add(PlanoDePagamento plano)
        {
            plano.Id = _db.NextPlanoId();
            _db.Planos[plano.Id] = plano;
            foreach (var c in plano.Cobrancas)
            {
                c.Id = _db.NextCobrancaId();
                c.PlanoDePagamentoId = plano.Id;
                _db.Cobrancas[c.Id] = c;
            }
            return plano;
        }
        public PlanoDePagamento? Get(int id) => _db.Planos.GetValueOrDefault(id);
        public IEnumerable<PlanoDePagamento> GetByResponsavel(int responsavelId) => _db.Planos.Values.Where(p => p.ResponsavelId == responsavelId);
    }

    public class InMemoryCobrancaRepository : ICobrancaRepository
    {
        private readonly InMemoryStore _db;
        public InMemoryCobrancaRepository(InMemoryStore db) => _db = db;
        public Cobranca Add(Cobranca c)
        {
            c.Id = _db.NextCobrancaId();
            _db.Cobrancas[c.Id] = c;
            return c;
        }
        public Cobranca? Get(int id) => _db.Cobrancas.GetValueOrDefault(id);
        public IEnumerable<Cobranca> GetByResponsavel(int responsavelId)
            => _db.Cobrancas.Values.Where(c => _db.Planos.TryGetValue(c.PlanoDePagamentoId, out var p) && p.ResponsavelId == responsavelId);
        public void Update(Cobranca c) => _db.Cobrancas[c.Id] = c;
    }
}
