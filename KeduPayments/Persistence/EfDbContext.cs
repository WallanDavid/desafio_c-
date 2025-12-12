#if USE_EF
using Microsoft.EntityFrameworkCore;
using Domain;

namespace Persistence
{
    public class KeduDbContext : DbContext
    {
        public KeduDbContext(DbContextOptions<KeduDbContext> options) : base(options) { }

        public DbSet<Responsavel> Responsaveis => Set<Responsavel>();
        public DbSet<CentroDeCusto> Centros => Set<CentroDeCusto>();
        public DbSet<PlanoDePagamento> Planos => Set<PlanoDePagamento>();
        public DbSet<Cobranca> Cobrancas => Set<Cobranca>();
        public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Responsavel>().HasKey(x => x.Id);
            b.Entity<CentroDeCusto>().HasKey(x => x.Id);
            b.Entity<PlanoDePagamento>().HasKey(x => x.Id);
            b.Entity<Cobranca>().HasKey(x => x.Id);
            b.Entity<Pagamento>().HasKey(x => x.Id);

            b.Entity<Responsavel>().Property(x => x.Id).ValueGeneratedNever();
            b.Entity<CentroDeCusto>().Property(x => x.Id).ValueGeneratedNever();

            b.Entity<PlanoDePagamento>()
                .HasMany(p => p.Cobrancas)
                .WithOne()
                .HasForeignKey(c => c.PlanoDePagamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Responsavel>()
                .HasMany<PlanoDePagamento>()
                .WithOne()
                .HasForeignKey(p => p.ResponsavelId);

            b.Entity<Cobranca>()
                .HasMany(c => c.Pagamentos)
                .WithOne()
                .HasForeignKey(p => p.CobrancaId);
        }
    }
}
#endif
