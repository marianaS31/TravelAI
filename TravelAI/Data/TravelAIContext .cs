using Microsoft.EntityFrameworkCore;
using TravelAI.Models;

namespace TravelAI.Data
{
    public class TravelAIContext : DbContext
    {
        public TravelAIContext(DbContextOptions<TravelAIContext> options)
            : base(options) { }

        public DbSet<Viagem> Viagens => Set<Viagem>();
        public DbSet<Itinerario> Itenerarios => Set<Itinerario>();
        public DbSet<DiaItenerario> DiasItenerario => Set<DiaItenerario>();
        public DbSet<Atividade> Atividades => Set<Atividade>();
        public DbSet<PrevisaoTempo> PrevisoesTempo => Set<PrevisaoTempo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Viagem>()
                .HasMany(v => v.Itenerarios)
                .WithOne(i => i.Viagem)
                .HasForeignKey(i => i.ViagemId);

            modelBuilder.Entity<Itinerario>()
                .HasMany(i => i.Dias)
                .WithOne(d => d.Itenerario)
                .HasForeignKey(d => d.ItinerarioId);

            modelBuilder.Entity<DiaItenerario>()
                .HasMany(d => d.Atividades)
                .WithOne(a => a.DiaItenerario)
                .HasForeignKey(a => a.DiaItenerarioId);

            modelBuilder.Entity<DiaItenerario>()
                .HasOne(d => d.PrevisaoTempo)
                .WithOne(p => p.DiaItenerario)
                .HasForeignKey<PrevisaoTempo>(p => p.DiaItenerarioId);

            modelBuilder.Entity<Viagem>()
                .Property(v => v.Orcamento)
                .HasPrecision(10, 2);
        }
    }
}