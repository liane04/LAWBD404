using Marketplace.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Marketplace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para todas as entidades
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Comprador> Compradores { get; set; }
        public DbSet<Morada> Moradas { get; set; }
        public DbSet<Contactos> Contactos { get; set; }
        public DbSet<ContactosComprador> ContactosCompradores { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Modelo> Modelos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Combustivel> Combustiveis { get; set; }
        public DbSet<Tipo> Tipos { get; set; }
        public DbSet<Imagem> Imagens { get; set; }
        public DbSet<PesquisasPassadas> PesquisasPassadas { get; set; }
        public DbSet<Notificacoes> Notificacoes { get; set; }
        public DbSet<FiltrosFav> FiltrosFavoritos { get; set; }
        public DbSet<AnuncioFav> AnunciosFavoritos { get; set; }
        public DbSet<MarcasFav> MarcasFavoritas { get; set; }
        public DbSet<AcaoAnuncio> AcoesAnuncio { get; set; }
        public DbSet<AcaoUser> AcoesUser { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagens> Mensagens { get; set; }
        public DbSet<DenunciaAnuncio> DenunciasAnuncio { get; set; }
        public DbSet<DenunciaUser> DenunciasUser { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da herança TPH para Utilizador
            modelBuilder.Entity<Utilizador>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Administrador>("Administrador")
                .HasValue<Vendedor>("Vendedor")
                .HasValue<Comprador>("Comprador");

            // Configuração da herança TPH para HistoricoAcao
            modelBuilder.Entity<HistoricoAcao>()
                .HasDiscriminator<string>("TipoAcao")
                .HasValue<AcaoAnuncio>("AcaoAnuncio")
                .HasValue<AcaoUser>("AcaoUser");

            // Configuração da herança TPH para Denuncia
            modelBuilder.Entity<Denuncia>()
                .HasDiscriminator<string>("TipoDenuncia")
                .HasValue<DenunciaAnuncio>("DenunciaAnuncio")
                .HasValue<DenunciaUser>("DenunciaUser");

            // IMPORTANTE: Configurações para evitar ciclos de cascade delete

            // AnunciosFavoritos - desativa cascade para Comprador
            modelBuilder.Entity<AnuncioFav>()
                .HasOne(af => af.Comprador)
                .WithMany(c => c.AnunciosFavoritos)
                .OnDelete(DeleteBehavior.Restrict);

            // MarcasFavoritas - desativa cascade para Comprador
            modelBuilder.Entity<MarcasFav>()
                .HasOne(mf => mf.Comprador)
                .WithMany(c => c.MarcasFavoritas)
                .OnDelete(DeleteBehavior.Restrict);

            // Conversas - desativa cascade para evitar múltiplos paths
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Comprador)
                .WithMany(comp => comp.Conversas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Vendedor)
                .WithMany(v => v.Conversas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Anuncio)
                .WithMany(a => a.Conversas)
                .OnDelete(DeleteBehavior.Restrict);

            // Denuncias - desativa cascade para evitar ciclos
            modelBuilder.Entity<Denuncia>()
                .HasOne(d => d.Comprador)
                .WithMany(c => c.Denuncias)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DenunciaUser>()
                .HasOne(d => d.UtilizadorAlvo)
                .WithMany(u => u.DenunciasRecebidas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DenunciaAnuncio>()
                .HasOne(d => d.Anuncio)
                .WithMany(a => a.Denuncias)
                .OnDelete(DeleteBehavior.Restrict);

            // HistoricoAcao - desativa cascade para evitar múltiplos paths
            modelBuilder.Entity<HistoricoAcao>()
                .HasOne(h => h.Administrador)
                .WithMany(a => a.HistoricoAcoes)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AcaoAnuncio>()
                .HasOne(a => a.Anuncio)
                .WithMany(an => an.AcoesAnuncio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AcaoUser>()
                .HasOne(a => a.Utilizador)
                .WithMany(u => u.AcoesUser)
                .OnDelete(DeleteBehavior.Restrict);

            // Notificacoes - desativa cascade para Comprador
            modelBuilder.Entity<Notificacoes>()
                .HasOne(n => n.Comprador)
                .WithMany(c => c.Notificacoes)
                .OnDelete(DeleteBehavior.Restrict);

            // Compras - desativa cascade
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Comprador)
                .WithMany(comp => comp.Compras)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Anuncio)
                .WithMany(a => a.Compras)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservas - desativa cascade
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Comprador)
                .WithMany(c => c.Reservas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Anuncio)
                .WithMany(a => a.Reservas)
                .OnDelete(DeleteBehavior.Restrict);

            // Visitas - desativa cascade para Reserva
            modelBuilder.Entity<Visita>()
                .HasOne(v => v.Reserva)
                .WithMany(r => r.Visitas)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurações de precisão
            modelBuilder.Entity<Anuncio>()
                .Property(a => a.Preco)
                .HasPrecision(10, 2);
        }

    }
}