using Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Data
{
    public class MarketplaceContext : DbContext
    {
        public MarketplaceContext(DbContextOptions<MarketplaceContext> options)
            : base(options)
        {
        }

        public DbSet<Anuncio> Anuncio { get; set; }
        // ... outros DbSets
        public DbSet<Comprador> Comprador { get; set; }
        public DbSet<AnuncioFav> AnuncioFav { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dizemos à BD para NÃO apagar em cascata quando um Comprador é apagado.
            // Isto resolve o erro de múltiplos caminhos.
            modelBuilder.Entity<AnuncioFav>()
                .HasOne(af => af.Comprador)
                .WithMany(c => c.AnunciosFavoritos)
                .HasForeignKey(af => af.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

