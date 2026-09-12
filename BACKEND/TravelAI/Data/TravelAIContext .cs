using Microsoft.EntityFrameworkCore;
using TravelAI.Models;

namespace TravelAI.Data
{
    public class TravelAIContext : DbContext
    {
        public TravelAIContext(DbContextOptions<TravelAIContext> options)
            : base(options) { }

        public DbSet<Viagem> Viagens => Set<Viagem>();
        public DbSet<Itinerario> Itinerarios => Set<Itinerario>();
        public DbSet<DiaItinerario> DiasItinerario => Set<DiaItinerario>();
        public DbSet<Atividade> Atividades => Set<Atividade>();
        public DbSet<PrevisaoTempo> PrevisoesTempo => Set<PrevisaoTempo>();
        public DbSet<Utilizador> Utilizadores => Set<Utilizador>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Viagem>()
                .HasMany(v => v.Itinerarios)
                .WithOne(i => i.Viagem)
                .HasForeignKey(i => i.ViagemId);

            modelBuilder.Entity<Itinerario>()
                .HasMany(i => i.Dias)
                .WithOne(d => d.Itinerario)
                .HasForeignKey(d => d.ItinerarioId);

            modelBuilder.Entity<DiaItinerario>()
                .HasMany(d => d.Atividades)
                .WithOne(a => a.DiaItinerario)
                .HasForeignKey(a => a.DiaItinerarioId);

            modelBuilder.Entity<DiaItinerario>()
                .HasOne(d => d.PrevisaoTempo)
                .WithOne(p => p.DiaItinerario)
                .HasForeignKey<PrevisaoTempo>(p => p.DiaItinerarioId);

            modelBuilder.Entity<Viagem>()
                .Property(v => v.Orcamento)
                .HasPrecision(10, 2);

            // Utilizador — email único, e ligação opcional a Viagem (nullable,
            // para não partir viagens já existentes criadas antes desta feature)
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Viagem>()
                .HasOne(v => v.Utilizador)
                .WithMany()
                .HasForeignKey(v => v.UtilizadorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}