#if USE_EF
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Repositories
{
    public class EfResponsavelRepository : IResponsavelRepository
    {
        private readonly KeduDbContext _db;
        public EfResponsavelRepository(KeduDbContext db) => _db = db;
        public Responsavel Add(string nome)
        {
            var id = (_db.Responsaveis.Any() ? _db.Responsaveis.Max(x => x.Id) : 0) + 1;
            var r = new Responsavel(id, nome);
            _db.Responsaveis.Add(r);
            _db.SaveChanges();
            return r;
        }
        public Responsavel? Get(int id) => _db.Responsaveis.Find(id);
        public IEnumerable<PlanoDePagamento> GetPlanosByResponsavel(int id)
            => _db.Planos.Include(p => p.Cobrancas).Where(p => p.ResponsavelId == id).AsNoTracking().ToList();
        public IEnumerable<Cobranca> GetCobrancasByResponsavel(int id)
            => _db.Cobrancas.Where(c => _db.Planos.Any(p => p.Id == c.PlanoDePagamentoId && p.ResponsavelId == id)).AsNoTracking().ToList();
    }

    public class EfCentroDeCustoRepository : ICentroDeCustoRepository
    {
        private readonly KeduDbContext _db;
        public EfCentroDeCustoRepository(KeduDbContext db) => _db = db;
        public CentroDeCusto Add(string nome)
        {
            var id = (_db.Centros.Any() ? _db.Centros.Max(x => x.Id) : 0) + 1;
            var c = new CentroDeCusto(id, nome);
            _db.Centros.Add(c);
            _db.SaveChanges();
            return c;
        }
        public IEnumerable<CentroDeCusto> GetAll() => _db.Centros.AsNoTracking().OrderBy(c => c.Id).ToList();
        public CentroDeCusto? Get(int id) => _db.Centros.Find(id);
    }

    public class EfPlanoRepository : IPlanoRepository
    {
        private readonly KeduDbContext _db;
        public EfPlanoRepository(KeduDbContext db) => _db = db;
        public PlanoDePagamento Add(PlanoDePagamento plano)
        {
            plano.Id = (_db.Planos.Any() ? _db.Planos.Max(x => x.Id) : 0) + 1;
            _db.Planos.Add(plano);
            foreach (var c in plano.Cobrancas)
            {
                c.Id = (_db.Cobrancas.Any() ? _db.Cobrancas.Max(x => x.Id) : 0) + 1;
                c.PlanoDePagamentoId = plano.Id;
                _db.Cobrancas.Add(c);
            }
            _db.SaveChanges();
            return plano;
        }
        public PlanoDePagamento? Get(int id) => _db.Planos.Include(p => p.Cobrancas).FirstOrDefault(p => p.Id == id);
        public IEnumerable<PlanoDePagamento> GetByResponsavel(int responsavelId) => _db.Planos.Where(p => p.ResponsavelId == responsavelId).AsNoTracking().ToList();
    }

    public class EfCobrancaRepository : ICobrancaRepository
    {
        private readonly KeduDbContext _db;
        public EfCobrancaRepository(KeduDbContext db) => _db = db;
        public Cobranca Add(Cobranca c)
        {
            c.Id = (_db.Cobrancas.Any() ? _db.Cobrancas.Max(x => x.Id) : 0) + 1;
            _db.Cobrancas.Add(c);
            _db.SaveChanges();
            return c;
        }
        public Cobranca? Get(int id) => _db.Cobrancas.Include(x => x.Pagamentos).FirstOrDefault(x => x.Id == id);
        public IEnumerable<Cobranca> GetByResponsavel(int responsavelId)
            => _db.Cobrancas.Where(c => _db.Planos.Any(p => p.Id == c.PlanoDePagamentoId && p.ResponsavelId == responsavelId)).AsNoTracking().ToList();
        public void Update(Cobranca c)
        {
            _db.Cobrancas.Update(c);
            _db.SaveChanges();
        }
    }
}
#endif
