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
                .HasDiscriminator<string>("Tipo")
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

            // Configuração para evitar ciclos de cascade delete
            modelBuilder.Entity<DenunciaUser>()
                .HasOne(d => d.UtilizadorAlvo)
                .WithMany(u => u.DenunciasRecebidas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Denuncia>()
                .HasOne(d => d.Comprador)
                .WithMany(c => c.Denuncias)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações de precisão
            modelBuilder.Entity<Anuncio>()
                .Property(a => a.Preco)
                .HasPrecision(10, 2);

            // Configuração de delete behavior
            modelBuilder.Entity<Visita>()
                .HasOne(v => v.Reserva)
                .WithMany(r => r.Visitas)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}