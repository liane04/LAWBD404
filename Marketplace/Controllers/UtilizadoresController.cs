using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using System.Text;
using System.Text.Encodings.Web;

namespace Marketplace.Controllers
{
    public class UtilizadoresController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;

        public UtilizadoresController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _env = env;
        }

        // GET: Utilizadores
        public IActionResult Index() => View();
        
        // GET: Utilizadores/Notificacoes
        [Authorize]
        public async Task<IActionResult> Notificacoes()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Tentar encontrar como Comprador ou Vendedor
            // Nota: Notificações estão associadas ao Utilizador (via CompradorId na BD)
            var utilizador = await _db.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == userId);

            if (utilizador == null) return RedirectToAction("Index", "Home");

            var notificacoes = await _db.Notificacoes
                .Where(n => n.CompradorId == utilizador.Id)
                .Include(n => n.Utilizador)
                .Include(n => n.FiltrosFav)
                .Include(n => n.MarcasFav)
                .ThenInclude(mf => mf.Marca)
                .OrderByDescending(n => n.Data)
                .Take(50)
                .ToListAsync();

            return View(notificacoes);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Json(new { available = false });
            var existing = await _userManager.FindByNameAsync(username);
            return Json(new { available = existing == null });
        }

        // GET: Utilizadores/Perfil
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());

            if (appUser == null) return RedirectToAction("Login");

            // Dados comuns do ApplicationUser
            ViewBag.UserName = appUser.UserName;
            ViewBag.Email = appUser.Email;
            ViewBag.Telefone = appUser.PhoneNumber ?? "Não definido";
            ViewBag.DataCriacao = DateTime.Now; // Temporário - será substituído pelos dados de Comprador/Vendedor

            // Se o usuário for vendedor, carregar seus anúncios
            if (User.IsInRole("Vendedor"))
            {
                var vendedor = await _db.Vendedores
                    .Include(v => v.Morada)
                    .Include(v => v.AvaliacoesRecebidas)
                    .FirstOrDefaultAsync(v => v.IdentityUserId == userId);

                if (vendedor != null)
                {
                    var anuncios = await _db.Anuncios
                        .Include(a => a.Marca)
                        .Include(a => a.Modelo)
                        .Include(a => a.Tipo)
                        .Include(a => a.Categoria)
                        .Include(a => a.Combustivel)
                        .Where(a => a.VendedorId == vendedor.Id)
                        .OrderByDescending(a => a.Id)
                        .ToListAsync();

                    // Carregar apenas a primeira imagem de cada anúncio (otimização de performance)
                    if (anuncios.Any())
                    {
                        var anuncioIds = anuncios.Select(a => a.Id).ToList();
                        var primeiraImagemPorAnuncio = await _db.Imagens
                            .Where(i => i.AnuncioId.HasValue && anuncioIds.Contains(i.AnuncioId.Value))
                            .GroupBy(i => i.AnuncioId.Value)
                            .Select(g => g.OrderBy(i => i.Id).First())
                            .ToListAsync();

                        // Associar primeira imagem a cada anúncio
                        foreach (var anuncio in anuncios)
                        {
                            var primeiraImagem = primeiraImagemPorAnuncio.FirstOrDefault(i => i.AnuncioId == anuncio.Id);
                            if (primeiraImagem != null)
                            {
                                anuncio.Imagens = new List<Imagem> { primeiraImagem };
                            }
                        }
                    }

                    ViewBag.MeusAnuncios = anuncios;
                    ViewBag.AnunciosCount = anuncios.Count;

                    // Contagens por estado para os filtros
                    ViewBag.AnunciosAtivos = anuncios.Count(a => a.Estado == "Ativo");
                    ViewBag.AnunciosReservados = anuncios.Count(a => a.Estado == "Reservado");
                    ViewBag.AnunciosVendidos = anuncios.Count(a => a.Estado == "Vendido");
                    ViewBag.AnunciosPausados = anuncios.Count(a => a.Estado == "Pausado");

                    // Métricas de Desempenho
                    ViewBag.TotalVisualizacoes = anuncios.Sum(a => a.NVisualizacoes);
                    if (vendedor.AvaliacoesRecebidas != null && vendedor.AvaliacoesRecebidas.Any())
                    {
                        ViewBag.NotaMedia = vendedor.AvaliacoesRecebidas.Average(a => a.Nota);
                        ViewBag.TotalAvaliacoes = vendedor.AvaliacoesRecebidas.Count;
                    }
                    else
                    {
                        ViewBag.NotaMedia = 0;
                        ViewBag.TotalAvaliacoes = 0;
                    }

                    // Carregar reservas RECEBIDAS (nos meus anúncios - como vendedor)
                    var reservasRecebidas = await _db.Reservas
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Include(r => r.Comprador)
                        .Where(r => r.Anuncio.VendedorId == vendedor.Id)
                        .OrderByDescending(r => r.Data)
                        .ToListAsync();

                    ViewBag.ReservasRecebidas = reservasRecebidas;
                    ViewBag.ReservasRecebidasCount = reservasRecebidas.Count;
                    ViewBag.ReservasAtivasVendedor = reservasRecebidas.Count(r => r.Estado == "Ativa");
                    ViewBag.TotalSinais = reservasRecebidas.Where(r => r.Estado == "Ativa").Sum(r => r.Anuncio.ValorSinal);

                    // Carregar visitas RECEBIDAS (aos meus anúncios - como vendedor)
                    var visitasRecebidas = await _db.Visitas
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Where(v => v.VendedorId == vendedor.Id)
                        .OrderByDescending(v => v.Data)
                        .ToListAsync();

                    // Carregar Comprador manualmente para cada visita recebida
                    foreach (var visita in visitasRecebidas)
                    {
                        if (visita.Comprador == null)
                        {
                            var utilizadorComprador = await _db.Set<Utilizador>()
                                .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);
                            if (utilizadorComprador != null)
                            {
                                visita.Comprador = new Comprador
                                {
                                    Id = utilizadorComprador.Id,
                                    Nome = utilizadorComprador.Nome,
                                    Email = utilizadorComprador.Email
                                };
                            }
                        }
                    }

                    ViewBag.VisitasRecebidas = visitasRecebidas;
                    ViewBag.VisitasRecebidasCount = visitasRecebidas.Count;

                    // Carregar visitas AGENDADAS (que eu agendei - como comprador)
                    var visitasAgendadas = await _db.Visitas
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Where(v => v.CompradorId == vendedor.Id)
                        .OrderByDescending(v => v.Data)
                        .ToListAsync();

                    // Carregar Vendedor manualmente para cada visita agendada
                    foreach (var visita in visitasAgendadas)
                    {
                        if (visita.Vendedor == null)
                        {
                            var utilizadorVendedor = await _db.Set<Utilizador>()
                                .FirstOrDefaultAsync(u => u.Id == visita.VendedorId);
                            if (utilizadorVendedor != null)
                            {
                                visita.Vendedor = new Vendedor
                                {
                                    Id = utilizadorVendedor.Id,
                                    Nome = utilizadorVendedor.Nome,
                                    Email = utilizadorVendedor.Email
                                };
                            }
                        }
                    }

                    ViewBag.VisitasAgendadas = visitasAgendadas;
                    ViewBag.VisitasAgendadasCount = visitasAgendadas.Count;

                    // Total de visitas (para compatibilidade com código existente)
                    ViewBag.MinhasVisitasVendedor = visitasRecebidas.Concat(visitasAgendadas).OrderByDescending(v => v.Data).ToList();

                    // Carregar disponibilidades do vendedor
                    var disponibilidades = await _db.DisponibilidadesVendedor
                        .Where(d => d.VendedorId == vendedor.Id)
                        .OrderBy(d => d.DiaSemana)
                        .ThenBy(d => d.HoraInicio)
                        .ToListAsync();
                    ViewBag.Disponibilidades = disponibilidades;
                    ViewBag.DisponibilidadesCount = disponibilidades.Count;

                    ViewBag.Nome = vendedor.Nome;
                    ViewBag.ImagemPerfil = string.IsNullOrWhiteSpace(vendedor.ImagemPerfil) ? null : vendedor.ImagemPerfil;
                    ViewBag.VendedorEstado = vendedor.Estado;
                    ViewBag.EstadoConta = vendedor.Estado ?? "Ativo";

                    // Dados de morada
                    if (vendedor.Morada != null)
                    {
                        ViewBag.Rua = vendedor.Morada.Rua;
                        ViewBag.CodigoPostal = vendedor.Morada.CodigoPostal;
                        ViewBag.Localidade = vendedor.Morada.Localidade;
                        ViewBag.Pais = "Portugal";
                    }
                    else
                    {
                        ViewBag.Rua = "Não definido";
                        ViewBag.CodigoPostal = "Não definido";
                        ViewBag.Localidade = "Não definido";
                        ViewBag.Pais = "Portugal";
                    }

                    ViewBag.Nif = vendedor.Nif ?? "Não definido";
                    ViewBag.Nib = vendedor.Nib ?? "Não definido";

                    // Carregar favoritos do vendedor (vendedores também podem ter favoritos)
                    var favoritosVendedor = await _db.AnunciosFavoritos
                        .Include(af => af.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(af => af.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(af => af.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Include(af => af.Anuncio)
                            .ThenInclude(a => a.Combustivel)
                        .Include(af => af.Anuncio)
                            .ThenInclude(a => a.Tipo)
                        .Where(af => af.CompradorId == vendedor.Id)
                        .OrderByDescending(af => af.Id)
                        .ToListAsync();

                    ViewBag.MeusFavoritos = favoritosVendedor;
                    ViewBag.FavoritosCount = favoritosVendedor.Count;

                    // Marcas Favoritas
                    var marcasFav = await _db.Set<MarcasFav>()
                        .Include(m => m.Marca)
                        .Where(m => m.CompradorId == vendedor.Id)
                        .ToListAsync();
                    ViewBag.MarcasFavoritas = marcasFav;
                    ViewBag.MarcasFavoritasCount = marcasFav.Count;

                    // Pesquisas Guardadas e Histórico (se o vendedor tiver criado um Comprador)
                    var compradorDoVendedor = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                    if (compradorDoVendedor != null)
                    {
                        // Pesquisas guardadas (Filtros Favoritos)
                        var filtrosVendedor = await _db.FiltrosFavoritos
                            .Where(f => f.CompradorId == compradorDoVendedor.Id)
                            .OrderByDescending(f => f.CreatedAt)
                            .ToListAsync();
                        ViewBag.SavedFilters = filtrosVendedor;

                        // Pesquisas Passadas (Histórico)
                        var pesquisasPassadasVendedor = await _db.PesquisasPassadas
                            .Where(p => p.CompradorId == compradorDoVendedor.Id)
                            .OrderByDescending(p => p.Data)
                            .Take(20)
                            .ToListAsync();
                        ViewBag.PesquisasPassadas = pesquisasPassadasVendedor;

                        // Carregar compras do vendedor (como comprador)
                        var comprasVendedor = await _db.Compras
                            .Include(c => c.Anuncio)
                                .ThenInclude(a => a.Marca)
                            .Include(c => c.Anuncio)
                                .ThenInclude(a => a.Modelo)
                            .Include(c => c.Anuncio)
                                .ThenInclude(a => a.Imagens)
                            .Include(c => c.Anuncio)
                                .ThenInclude(a => a.Combustivel)
                            .Include(c => c.Anuncio)
                                .ThenInclude(a => a.Vendedor)
                            .Where(c => c.CompradorId == compradorDoVendedor.Id)
                            .OrderByDescending(c => c.Data)
                            .ToListAsync();
                        ViewBag.Compras = comprasVendedor;
                        ViewBag.ComprasCount = comprasVendedor.Count;

                        // Carregar reservas FEITAS pelo vendedor (como comprador)
                        var reservasFeitas = await _db.Reservas
                            .Include(r => r.Anuncio)
                                .ThenInclude(a => a.Marca)
                            .Include(r => r.Anuncio)
                                .ThenInclude(a => a.Modelo)
                            .Include(r => r.Anuncio)
                                .ThenInclude(a => a.Imagens)
                            .Include(r => r.Anuncio)
                                .ThenInclude(a => a.Vendedor)
                            .Include(r => r.Anuncio)
                                .ThenInclude(a => a.Combustivel)
                            .Where(r => r.CompradorId == compradorDoVendedor.Id)
                            .OrderByDescending(r => r.Data)
                            .ToListAsync();

                        ViewBag.MinhasReservas = reservasFeitas;
                        ViewBag.MinhasReservasCount = reservasFeitas.Count;
                        ViewBag.ReservasAtivasComprador = reservasFeitas.Count(r => r.Estado == "Ativa");
                        ViewBag.ReservasExpiradas = reservasFeitas.Count(r => r.Estado == "Expirada");
                    }
                    else
                    {
                        // Se ainda não tem Comprador, inicializar listas vazias
                        ViewBag.SavedFilters = new List<FiltrosFav>();
                        ViewBag.PesquisasPassadas = new List<PesquisasPassadas>();
                    }
                }
            }
            // Se o usuário for comprador, carregar seus favoritos
            else if (User.IsInRole("Comprador"))
            {
                var comprador = await _db.Compradores
                    .Include(c => c.Morada)
                    .Include(c => c.AnunciosFavoritos)
                        .ThenInclude(af => af.Anuncio)
                            .ThenInclude(a => a.Marca)
                    .Include(c => c.AnunciosFavoritos)
                        .ThenInclude(af => af.Anuncio)
                            .ThenInclude(a => a.Modelo)
                    .Include(c => c.AnunciosFavoritos)
                        .ThenInclude(af => af.Anuncio)
                            .ThenInclude(a => a.Imagens)
                    .Include(c => c.AnunciosFavoritos)
                        .ThenInclude(af => af.Anuncio)
                            .ThenInclude(a => a.Combustivel)
                    .Include(c => c.AnunciosFavoritos)
                        .ThenInclude(af => af.Anuncio)
                            .ThenInclude(a => a.Tipo)
                    .FirstOrDefaultAsync(c => c.IdentityUserId == userId);

                if (comprador != null)
                {
                    ViewBag.MeusFavoritos = comprador.AnunciosFavoritos.OrderByDescending(af => af.Id).ToList();
                    ViewBag.FavoritosCount = comprador.AnunciosFavoritos.Count;

                    // Carregar visitas do comprador
                    var visitasComprador = await _db.Visitas
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Where(v => v.CompradorId == comprador.Id)
                        .OrderByDescending(v => v.Data)
                        .ToListAsync();

                    // Carregar Vendedor manualmente para cada visita
                    foreach (var visita in visitasComprador)
                    {
                        if (visita.Vendedor == null)
                        {
                            var utilizadorVendedor = await _db.Set<Utilizador>()
                                .FirstOrDefaultAsync(u => u.Id == visita.VendedorId);
                            if (utilizadorVendedor != null)
                            {
                                visita.Vendedor = new Vendedor
                                {
                                    Id = utilizadorVendedor.Id,
                                    Nome = utilizadorVendedor.Nome,
                                    Email = utilizadorVendedor.Email
                                };
                            }
                        }
                    }

                    ViewBag.MinhasVisitasComprador = visitasComprador;

                    // Carregar compras do comprador
                    var compras = await _db.Compras
                        .Include(c => c.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(c => c.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(c => c.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Include(c => c.Anuncio)
                            .ThenInclude(a => a.Combustivel)
                        .Include(c => c.Anuncio)
                            .ThenInclude(a => a.Vendedor)
                        .Where(c => c.CompradorId == comprador.Id)
                        .OrderByDescending(c => c.Data)
                        .ToListAsync();
                    ViewBag.Compras = compras;
                    ViewBag.ComprasCount = compras.Count;

                    // Carregar reservas FEITAS (como comprador)
                    var reservasComprador = await _db.Reservas
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Vendedor)
                        .Include(r => r.Anuncio)
                            .ThenInclude(a => a.Combustivel)
                        .Where(r => r.CompradorId == comprador.Id)
                        .OrderByDescending(r => r.Data)
                        .ToListAsync();

                    ViewBag.MinhasReservas = reservasComprador;
                    ViewBag.MinhasReservasCount = reservasComprador.Count;
                    ViewBag.ReservasAtivasComprador = reservasComprador.Count(r => r.Estado == "Ativa");
                    ViewBag.ReservasExpiradas = reservasComprador.Count(r => r.Estado == "Expirada");

                    ViewBag.Nome = comprador.Nome;
                    ViewBag.ImagemPerfil = string.IsNullOrWhiteSpace(comprador.ImagemPerfil) ? null : comprador.ImagemPerfil;
                    ViewBag.EstadoConta = comprador.Estado ?? "Ativo";

                    // Dados de morada
                    if (comprador.Morada != null)
                    {
                        ViewBag.Rua = comprador.Morada.Rua;
                        ViewBag.CodigoPostal = comprador.Morada.CodigoPostal;
                        ViewBag.Localidade = comprador.Morada.Localidade;
                        ViewBag.Pais = "Portugal";
                    }
                    else
                    {
                        ViewBag.Rua = "Não definido";
                        ViewBag.CodigoPostal = "Não definido";
                        ViewBag.Localidade = "Não definido";
                        ViewBag.Pais = "Portugal";
                    }

                    // Pesquisas guardadas (Filtros Favoritos)
                    var filtros = await _db.FiltrosFavoritos
                        .Where(f => f.CompradorId == comprador.Id)
                        .OrderByDescending(f => f.CreatedAt)
                        .ToListAsync();
                    ViewBag.SavedFilters = filtros;

                    // Marcas Favoritas
                    var marcasFav = await _db.Set<MarcasFav>()
                        .Include(m => m.Marca)
                        .Where(m => m.CompradorId == comprador.Id)
                        .ToListAsync();
                    ViewBag.MarcasFavoritas = marcasFav;
                    ViewBag.MarcasFavoritasCount = marcasFav.Count;

                    // Pesquisas Passadas (Histórico)
                    var pesquisasPassadas = await _db.PesquisasPassadas
                        .Where(p => p.CompradorId == comprador.Id)
                        .OrderByDescending(p => p.Data)
                        .Take(20) // Mostrar apenas as últimas 20
                        .ToListAsync();
                    ViewBag.PesquisasPassadas = pesquisasPassadas;
                }
            }

            return View();
        }

        // GET: Utilizadores/PerfilDados (JSON para dinamizar a view de Perfil)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PerfilDados()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            string? nome = appUser?.FullName ?? appUser?.UserName;
            string? email = appUser?.Email;
            string? phone = appUser?.PhoneNumber;
            string? imagem = null;
            string? rua = null;
            string? localidade = null;
            string? codigoPostal = null;

            if (User.IsInRole("Vendedor"))
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == userId);
                if (vendedor != null)
                {
                    nome = vendedor.Nome;
                    imagem = vendedor.ImagemPerfil;
                    if (vendedor.Morada != null)
                    {
                        rua = vendedor.Morada.Rua;
                        localidade = vendedor.Morada.Localidade;
                        codigoPostal = vendedor.Morada.CodigoPostal;
                    }
                }
            }
            else if (User.IsInRole("Comprador"))
            {
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                if (comprador != null)
                {
                    nome = comprador.Nome;
                    imagem = comprador.ImagemPerfil;
                    if (comprador.Morada != null)
                    {
                        rua = comprador.Morada.Rua;
                        localidade = comprador.Morada.Localidade;
                        codigoPostal = comprador.Morada.CodigoPostal;
                    }
                }
            }

            return Json(new { nome, email, phone, imagemPerfil = imagem, morada = new { rua, localidade, codigoPostal } });
        }

        // GET: Utilizadores/Edit
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null) return RedirectToAction("Login");

            var model = new Marketplace.Models.ViewModels.EditarPerfilViewModel
            {
                Nome = appUser.FullName ?? appUser.UserName ?? string.Empty,
                Email = appUser.Email,
                Telefone = appUser.PhoneNumber,
                IsVendedor = await _userManager.IsInRoleAsync(appUser, "Vendedor")
            };

            if (model.IsVendedor)
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    model.Nome = vendedor.Nome;
                    model.Nif = vendedor.Nif;
                    model.Nib = vendedor.Nib;
                    model.DadosFaturacao = vendedor.DadosFaturacao;
                    model.ImagemPerfilAtual = vendedor.ImagemPerfil;
                    if (vendedor.Morada != null)
                    {
                        model.Rua = vendedor.Morada.Rua;
                        model.Localidade = vendedor.Morada.Localidade;
                        model.CodigoPostal = vendedor.Morada.CodigoPostal;
                    }
                }
            }
            else
            {
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    model.Nome = comprador.Nome;
                    model.ImagemPerfilAtual = comprador.ImagemPerfil;
                    if (comprador.Morada != null)
                    {
                        model.Rua = comprador.Morada.Rua;
                        model.Localidade = comprador.Morada.Localidade;
                        model.CodigoPostal = comprador.Morada.CodigoPostal;
                    }
                }
            }

            return View(model);
        }

        // POST: Utilizadores/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marketplace.Models.ViewModels.EditarPerfilViewModel model, IFormFile? fotoPerfil)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validação extra: NIF (checksum PT)
            if (!string.IsNullOrWhiteSpace(model.Nif) && !IsValidNif(model.Nif))
            {
                ModelState.AddModelError("Nif", "NIF inválido (checksum)");
                return View(model);
            }

            appUser.FullName = model.Nome;
            if (!string.IsNullOrWhiteSpace(model.Telefone))
            {
                appUser.PhoneNumber = model.Telefone;
            }
            await _userManager.UpdateAsync(appUser);

            if (await _userManager.IsInRoleAsync(appUser, "Vendedor"))
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    vendedor.Nome = model.Nome;
                    vendedor.Nif = model.Nif;
                    vendedor.Nib = model.Nib;
                    vendedor.DadosFaturacao = model.DadosFaturacao;

                    // Morada
                    if (!string.IsNullOrWhiteSpace(model.Rua) && !string.IsNullOrWhiteSpace(model.Localidade) && !string.IsNullOrWhiteSpace(model.CodigoPostal))
                    {
                        if (vendedor.Morada == null)
                        {
                            vendedor.Morada = new Morada
                            {
                                Rua = model.Rua!,
                                Localidade = model.Localidade!,
                                CodigoPostal = model.CodigoPostal!
                            };
                        }
                        else
                        {
                            vendedor.Morada.Rua = model.Rua!;
                            vendedor.Morada.Localidade = model.Localidade!;
                            vendedor.Morada.CodigoPostal = model.CodigoPostal!;
                        }
                    }

                    if (fotoPerfil != null)
                    {
                        if (ImageUploadHelper.IsValidProfileImage(fotoPerfil, out var error))
                        {
                            var newPath = await ImageUploadHelper.UploadProfileImage(fotoPerfil, _env.WebRootPath, appUser.Id);
                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                if (!string.IsNullOrWhiteSpace(vendedor.ImagemPerfil))
                                {
                                    ImageUploadHelper.DeleteProfileImage(vendedor.ImagemPerfil, _env.WebRootPath);
                                }
                                vendedor.ImagemPerfil = newPath;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Falha ao guardar a imagem de perfil.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, error);
                            return View(model);
                        }
                    }
                }
            }
            else
            {
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    comprador.Nome = model.Nome;
                    // Morada
                    if (!string.IsNullOrWhiteSpace(model.Rua) && !string.IsNullOrWhiteSpace(model.Localidade) && !string.IsNullOrWhiteSpace(model.CodigoPostal))
                    {
                        if (comprador.Morada == null)
                        {
                            comprador.Morada = new Morada
                            {
                                Rua = model.Rua!,
                                Localidade = model.Localidade!,
                                CodigoPostal = model.CodigoPostal!
                            };
                        }
                        else
                        {
                            comprador.Morada.Rua = model.Rua!;
                            comprador.Morada.Localidade = model.Localidade!;
                            comprador.Morada.CodigoPostal = model.CodigoPostal!;
                        }
                    }
                    if (fotoPerfil != null)
                    {
                        if (ImageUploadHelper.IsValidProfileImage(fotoPerfil, out var error))
                        {
                            var newPath = await ImageUploadHelper.UploadProfileImage(fotoPerfil, _env.WebRootPath, appUser.Id);
                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                if (!string.IsNullOrWhiteSpace(comprador.ImagemPerfil))
                                {
                                    ImageUploadHelper.DeleteProfileImage(comprador.ImagemPerfil, _env.WebRootPath);
                                }
                                comprador.ImagemPerfil = newPath;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Falha ao guardar a imagem de perfil.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, error);
                            return View(model);
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            TempData["PerfilSucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("Perfil");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SecurityStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
            var hasAuthenticator = !string.IsNullOrWhiteSpace(await _userManager.GetAuthenticatorKeyAsync(user));

            return Json(new
            {
                email = user.Email,
                twoFactorEnabled = is2FaEnabled,
                recoveryCodes = recoveryCodesLeft,
                hasAuthenticator
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TwoFactorSetup()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrWhiteSpace(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest(new { error = "Não foi possível gerar uma chave de autenticação. Tente novamente." });
            }

            var sharedKey = FormatKey(key);
            var authenticatorUri = GenerateQrCodeUri(user.Email ?? user.UserName ?? "DriveDeal", key);

            return Json(new { sharedKey, authenticatorUri });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactor([FromForm] EnableTwoFactorRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var code = (request.Code ?? string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                code);

            if (!isValid)
            {
                return BadRequest(new { error = "Código de verificação inválido. Confirme o valor na app de autenticação." });
            }

            var enableResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enableResult.Succeeded)
            {
                return BadRequest(new { error = "Não foi possível ativar a autenticação de dois fatores. Tente novamente." });
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
            await _signInManager.RefreshSignInAsync(user);

            return Json(new { success = true, recoveryCodes });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Não foi possível desativar a autenticação de dois fatores." });
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPassword([FromForm] ChangePasswordRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.PasswordAtual) ||
                string.IsNullOrWhiteSpace(request.PasswordNova) ||
                string.IsNullOrWhiteSpace(request.PasswordNovaConfirmacao))
            {
                return BadRequest(new { error = "Preencha todos os campos." });
            }

            if (!string.Equals(request.PasswordNova, request.PasswordNovaConfirmacao, StringComparison.Ordinal))
            {
                return BadRequest(new { error = "As palavras-passe novas não coincidem." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            IdentityResult result;
            if (await _userManager.HasPasswordAsync(user))
            {
                result = await _userManager.ChangePasswordAsync(user, request.PasswordAtual, request.PasswordNova);
            }
            else
            {
                result = await _userManager.AddPasswordAsync(user, request.PasswordNova);
            }

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { errors });
            }

            await _signInManager.RefreshSignInAsync(user);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _emailSender.SendAsync(
                        user.Email,
                        "Password alterada - DriveDeal",
                        "<p>A sua password foi alterada com sucesso.</p><p>Se não reconhece esta alteração, reponha a sua palavra-passe imediatamente.</p>");
                }
                catch
                {
                }
            }

            return Json(new { success = true, message = "Password alterada com sucesso." });
        }

        private bool IsValidNif(string? nif)
        {
            if (string.IsNullOrWhiteSpace(nif)) return true;
            var digits = new string(nif.Where(char.IsDigit).ToArray());
            if (digits.Length != 9) return false;
            var first = digits[0];
            if ("1235689".IndexOf(first) < 0) return false;
            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                sum += (digits[i] - '0') * (9 - i);
            }
            var mod11 = sum % 11;
            var check = 11 - mod11;
            if (check >= 10) check = 0;
            return check == (digits[8] - '0');
        }

        // GET: Utilizadores/Registar
        [HttpGet]
        public IActionResult Registar() => View();

        // POST: Utilizadores/Registar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(string nome, string email, string username, string password, string confirmPassword, string userType)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["RegistarError"] = "Preencha todos os campos obrigatórios.";
                return View();
            }
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                TempData["RegistarError"] = "As palavras-passe não coincidem.";
                return View();
            }
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                TempData["RegistarError"] = "Já existe uma conta com este email.";
                return View();
            }

            
            var appUser = new ApplicationUser { UserName = username, Email = email, FullName = nome };
            var createRes = await _userManager.CreateAsync(appUser, password);
            if (!createRes.Succeeded)
            {
                TempData["RegistarError"] = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return View();
            }

            string role = string.Equals(userType, "vendedor", StringComparison.OrdinalIgnoreCase) ? "Vendedor" : "Comprador";
            await _userManager.AddToRoleAsync(appUser, role);

            // Entidade de domínio associada
            if (role == "Vendedor")
            {
                _db.Vendedores.Add(new Vendedor
                {
                    Username = username,
                    Email = email,
                    Nome = nome,
                    PasswordHash = "IDENTITY",
                    Estado = "Pendente",
                    Tipo = role,
                    IdentityUserId = appUser.Id
                });
            }
            else
            {
                _db.Compradores.Add(new Comprador
                {
                    Username = username,
                    Email = email,
                    Nome = nome,
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = role,
                    IdentityUserId = appUser.Id
                });
            }
            await _db.SaveChangesAsync();

            // Email de confirmação - OPCIONAL (não falhar se SMTP não estiver configurado)
            bool emailEnviado = false;
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var link = Url.Action("ConfirmarEmail", "Utilizadores", new { userId = appUser.Id, token }, Request.Scheme)!;
                var html = Marketplace.Services.EmailTemplates.ConfirmEmail("DriveDeal", link);
                await _emailSender.SendAsync(email, "Confirmação de Email - DriveDeal", html);
                emailEnviado = true;
            }
            catch (Exception ex)
            {
                // Log do erro mas não falhar o registo
                Console.WriteLine($"⚠️  Erro ao enviar email de confirmação: {ex.Message}");
            }

            // Confirmar email automaticamente se o envio falhou (para desenvolvimento)
            if (!emailEnviado && !appUser.EmailConfirmed)
            {
                appUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(appUser);
            }

            TempData["RegistarSucesso"] = emailEnviado
                ? "Conta criada. Verifique o seu email para confirmar."
                : "Conta criada com sucesso! Pode agora fazer login.";
            return RedirectToAction("Login");
        }

        // GET: Utilizadores/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Utilizadores/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string identifier, string password, bool rememberMe = false, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Credenciais inválidas.";
                return View();
            }
            var user = await _userManager.FindByEmailAsync(identifier);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(identifier);
            }
            if (user == null)
            {
                TempData["LoginError"] = "Credenciais incorretas.";
                return View();
            }

            // NOTA: EmailConfirmed não é verificado aqui porque RequireConfirmedEmail = false no Program.cs

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                var lockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.LocalDateTime.ToString("dd/MM/yyyy HH:mm") : "indefinidamente";
                
                TempData["LoginError"] = $"A sua conta encontra-se bloqueada até {lockoutEnd}.";
                return View();
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(Login2FA), new { returnUrl, rememberMe });
            }

            if (!result.Succeeded)
            {
                TempData["LoginError"] = "Credenciais incorretas.";
                return View();
            }

            if (await _userManager.IsInRoleAsync(user, "Administrador"))
                return RedirectToAction("Index", "Administrador");
            if (await _userManager.IsInRoleAsync(user, "Vendedor"))
                return RedirectToAction("Index", "Anuncios");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login2FA(bool rememberMe, string? returnUrl = null)
        {
            return View(new TwoFactorViewModel { RememberMe = rememberMe, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login2FA(TwoFactorViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError(string.Empty, "Introduza o código de 6 dígitos.");
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Sessão de autenticação expirada. Volte a iniciar sessão.");
                return View(model);
            }

            var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada devido a tentativas falhadas. Aguarde ou redefina a password.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Código inválido. Tente novamente.");
            return View(model);
        }

        // POST: Utilizadores/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // POST: Utilizadores/ExternalLogin (Google)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
                return RedirectToAction(nameof(Login));

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Utilizadores", new { returnUrl });
            var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(props, provider);
        }

        // GET: Utilizadores/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                TempData["LoginError"] = $"Falha no login externo: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["LoginError"] = "Não foi possível obter informações do login externo.";
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? "Utilizador";

            if (email == null)
            {
                TempData["LoginError"] = "A conta externa não forneceu um email válido.";
                return RedirectToAction(nameof(Login));
            }

            // Se já existe conta com este email, associar o login externo
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                try { await _userManager.AddLoginAsync(existingUser, info); } catch { /* ignorar se já associado */ }
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/");
            }

            // Criar nova conta e entidade de domínio (Comprador)
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                TempData["LoginError"] = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }

            await _userManager.AddLoginAsync(user, info);
            await _userManager.AddToRoleAsync(user, "Comprador");

            var comprador = new Comprador
            {
                Username = user.UserName!,
                Email = user.Email!,
                Nome = name,
                IdentityUserId = user.Id,
                PasswordHash = "IDENTITY"
            };
            _db.Compradores.Add(comprador);
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? "/");
        }

        // GET: Utilizadores/ConfirmarEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                ViewBag.Sucesso = false;
                ViewBag.Mensagem = "Utilizador não encontrado.";
                return View("ConfirmarEmailNew");
            }

            var res = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.Sucesso = res.Succeeded;
            ViewBag.Mensagem = res.Succeeded
                ? "Email confirmado com sucesso! Pode agora fazer login na sua conta."
                : "Token inválido ou expirado. Por favor, solicite um novo email de confirmação.";
            ViewBag.Email = user.Email;

            return View("ConfirmarEmailNew");
        }

        // POST: Utilizadores/ReenviarConfirmacao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarConfirmacao(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["LoginError"] = "Conta não encontrada.";
                return RedirectToAction("Login");
            }
            if (user.EmailConfirmed)
            {
                TempData["LoginInfo"] = "Email já confirmado.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmarEmail", "Utilizadores", new { userId = user.Id, token }, Request.Scheme)!;
            var html = Marketplace.Services.EmailTemplates.ConfirmEmail("DriveDeal", link);
            await _emailSender.SendAsync(email, "Confirmação de Email - DriveDeal", html);
            TempData["LoginInfo"] = "Link de confirmação reenviado.";
            return RedirectToAction("Login");
        }

        // GET: Utilizadores/EsqueceuPassword
        [HttpGet]
        public IActionResult EsqueceuPassword() => View();

        // POST: Utilizadores/EsqueceuPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueceuPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Erro = "Por favor, insira o seu email.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Por segurança, não revelar se o email existe ou não
                ViewBag.Sucesso = true;
                ViewBag.Email = email;
                return View();
            }

            // Gerar token de recuperação de password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("RedefinirPassword", "Utilizadores", new { userId = user.Id, token }, Request.Scheme)!;

            // Tentar enviar email (opcional - não falhar se SMTP não configurado)
            bool emailEnviado = false;
            try
            {
                await _emailSender.SendAsync(
                    email,
                    "Recuperação de Palavra-passe - DriveDeal",
                    $@"<h2>Recuperação de Palavra-passe</h2>
                       <p>Recebemos um pedido para redefinir a palavra-passe da sua conta.</p>
                       <p>Clique no link abaixo para definir uma nova palavra-passe:</p>
                       <p><a href=""{link}"" style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;"">Redefinir Palavra-passe</a></p>
                       <p>Ou copie e cole este link no seu navegador:</p>
                       <p>{link}</p>
                       <p><small>Se não solicitou esta alteração, ignore este email.</small></p>");
                emailEnviado = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erro ao enviar email de recuperação: {ex.Message}");
            }

            ViewBag.Sucesso = true;
            ViewBag.Email = email;
            ViewBag.EmailEnviado = emailEnviado;
            ViewBag.LinkRecuperacao = emailEnviado ? null : link; // Mostrar link se email não foi enviado (dev)
            return View();
        }

        // GET: Utilizadores/RedefinirPassword
        [HttpGet]
        public async Task<IActionResult> RedefinirPassword(int userId, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserId = userId;
            ViewBag.Token = token;
            ViewBag.Email = user.Email;
            return View();
        }

        // POST: Utilizadores/RedefinirPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirPassword(int userId, string token, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Erro = "Por favor, preencha todos os campos.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ViewBag.Erro = "As palavras-passe não coincidem.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                ViewBag.Erro = "Utilizador não encontrado.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join("; ", result.Errors.Select(e => e.Description));
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                ViewBag.Email = user.Email;
                return View();
            }

            ViewBag.Sucesso = true;
            ViewBag.Email = user.Email;
            return View();
        }

        private static string FormatKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            var result = new StringBuilder();
            int current = 0;
            while (current + 4 < key.Length)
            {
                result.Append(key.AsSpan(current, 4)).Append(' ');
                current += 4;
            }
            if (current < key.Length)
            {
                result.Append(key[current..]);
            }
            return result.ToString().Trim().ToUpperInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            var issuer = UrlEncoder.Default.Encode("DriveDeal");
            var encodedEmail = UrlEncoder.Default.Encode(email);
            return $"otpauth://totp/{issuer}:{encodedEmail}?secret={unformattedKey}&issuer={issuer}&digits=6";
        }

        public class ChangePasswordRequest
        {
            public string PasswordAtual { get; set; } = string.Empty;
            public string PasswordNova { get; set; } = string.Empty;
            public string PasswordNovaConfirmacao { get; set; } = string.Empty;
        }

        public class EnableTwoFactorRequest
        {
            public string Code { get; set; } = string.Empty;
        }

        public class TwoFactorViewModel
        {
            public string? Code { get; set; }
            public bool RememberMe { get; set; }
            public bool RememberMachine { get; set; }
            public string? ReturnUrl { get; set; }
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> NotificationSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var claims = await _userManager.GetClaimsAsync(user);
            bool email = GetNotificationFlag(claims, "notif:email", true);
            bool novos = GetNotificationFlag(claims, "notif:novos", true);
            bool preco = GetNotificationFlag(claims, "notif:preco", false);

            return Json(new { email, novos, preco });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetNotification(string tipo, bool ativo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var key = tipo?.ToLowerInvariant() switch
            {
                "email" => "notif:email",
                "novos" => "notif:novos",
                "preco" => "notif:preco",
                _ => null
            };
            if (key == null) return BadRequest(new { error = "Tipo de notificação inválido." });

            await SetNotificationFlag(user, key, ativo);
            return Json(new { success = true, tipo = key, ativo });
        }

        private bool GetNotificationFlag(IEnumerable<System.Security.Claims.Claim> claims, string claimType, bool defaultValue)
        {
            var claim = claims.FirstOrDefault(c => c.Type == claimType);
            if (claim == null) return defaultValue;
            return bool.TryParse(claim.Value, out var val) ? val : defaultValue;
        }

        private async Task SetNotificationFlag(ApplicationUser user, string claimType, bool value)
        {
            var existing = await _userManager.GetClaimsAsync(user);
            var current = existing.FirstOrDefault(c => c.Type == claimType);
            if (current != null)
            {
                await _userManager.ReplaceClaimAsync(user, current, new System.Security.Claims.Claim(claimType, value.ToString()));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, value.ToString()));
            }
            await _signInManager.RefreshSignInAsync(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PrivacySettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var claims = await _userManager.GetClaimsAsync(user);
            bool perfilPublico = GetNotificationFlag(claims, "privacy:public", true);
            bool mostrarEmail = GetNotificationFlag(claims, "privacy:showEmail", false);

            return Json(new { perfilPublico, mostrarEmail });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrivacy(string tipo, bool ativo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var key = tipo?.ToLowerInvariant() switch
            {
                "perfil" => "privacy:public",
                "email" => "privacy:showEmail",
                _ => null
            };
            if (key == null) return BadRequest(new { error = "Tipo de privacidade inválido." });

            await SetNotificationFlag(user, key, ativo);
            return Json(new { success = true, tipo = key, ativo });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var identityId = user.Id;
            var util = await _db.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (util != null)
            {
                util.Estado = "Desativado";
                _db.Update(util);
                await _db.SaveChangesAsync();
            }

            // Lock account via Identity
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await _signInManager.SignOutAsync();

            TempData["PerfilSucesso"] = "Conta desativada. Pode reativar contactando suporte.";
            return RedirectToAction("Login", "Utilizadores");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var identityId = user.Id;

            // Soft-delete: marcar estado e bloquear login
            var util = await _db.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (util != null)
            {
                util.Estado = "Eliminado";
                _db.Update(util);
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            await _signInManager.SignOutAsync();
            await _db.SaveChangesAsync();

            TempData["PerfilSucesso"] = "Conta eliminada (soft delete). Contacte suporte se precisar reverter.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TerminateOtherSessions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return Json(new { success = true, message = "Sessões em outros dispositivos foram terminadas." });
        }

        // GET: Utilizadores/GetNotificationPreferences
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prefs = await _db.NotificationPreferences
                .FirstOrDefaultAsync(np => np.IdentityUserId == user.Id.ToString());

            if (prefs == null)
            {
                // Criar preferências padrão se não existirem
                prefs = new NotificationPreferences
                {
                    IdentityUserId = user.Id.ToString(),
                    EmailNotifications = true,
                    NewListingsAlerts = true,
                    PriceDropAlerts = false,
                    Newsletter = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.NotificationPreferences.Add(prefs);
                await _db.SaveChangesAsync();
            }

            return Json(new
            {
                success = true,
                email = prefs.EmailNotifications,
                novos = prefs.NewListingsAlerts,
                preco = prefs.PriceDropAlerts,
                newsletter = prefs.Newsletter
            });
        }

        // POST: Utilizadores/UpdateNotificationPreference
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotificationPreference(string tipo, bool ativo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prefs = await _db.NotificationPreferences
                .FirstOrDefaultAsync(np => np.IdentityUserId == user.Id.ToString());

            if (prefs == null)
            {
                prefs = new NotificationPreferences
                {
                    IdentityUserId = user.Id.ToString(),
                    EmailNotifications = true,
                    NewListingsAlerts = true,
                    PriceDropAlerts = false,
                    Newsletter = true
                };
                _db.NotificationPreferences.Add(prefs);
            }

            // Atualizar a preferência específica
            switch (tipo.ToLower())
            {
                case "email":
                    prefs.EmailNotifications = ativo;
                    break;
                case "novos":
                    prefs.NewListingsAlerts = ativo;
                    break;
                case "preco":
                    prefs.PriceDropAlerts = ativo;
                    break;
                case "newsletter":
                    prefs.Newsletter = ativo;
                    break;
                default:
                    return Json(new { success = false, message = "Tipo de notificação inválido." });
            }

            prefs.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Preferência atualizada com sucesso." });
        }

        // POST: Utilizadores/UploadFotoPerfil
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadFotoPerfil(IFormFile fotoPerfil)
        {
            if (fotoPerfil == null || fotoPerfil.Length == 0)
            {
                return Json(new { success = false, message = "Nenhuma imagem foi selecionada." });
            }

            // Validar imagem
            if (!ImageUploadHelper.IsValidProfileImage(fotoPerfil, out var error))
            {
                return Json(new { success = false, message = error });
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null)
            {
                return Json(new { success = false, message = "Utilizador não encontrado." });
            }

            try
            {
                string? imagemAnterior = null;
                string? novaImagemPath = null;

                // Upload da imagem
                novaImagemPath = await ImageUploadHelper.UploadProfileImage(fotoPerfil, _env.WebRootPath, appUser.Id);
                if (string.IsNullOrWhiteSpace(novaImagemPath))
                {
                    return Json(new { success = false, message = "Erro ao guardar a imagem." });
                }

                // Atualizar no banco de dados
                if (User.IsInRole("Vendedor"))
                {
                    var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);
                    if (vendedor != null)
                    {
                        imagemAnterior = vendedor.ImagemPerfil;
                        vendedor.ImagemPerfil = novaImagemPath;
                    }
                }
                else if (User.IsInRole("Comprador"))
                {
                    var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                    if (comprador != null)
                    {
                        imagemAnterior = comprador.ImagemPerfil;
                        comprador.ImagemPerfil = novaImagemPath;
                    }
                }

                await _db.SaveChangesAsync();

                // Deletar imagem anterior
                if (!string.IsNullOrWhiteSpace(imagemAnterior))
                {
                    ImageUploadHelper.DeleteProfileImage(imagemAnterior, _env.WebRootPath);
                }

                return Json(new
                {
                    success = true,
                    message = "Foto de perfil atualizada com sucesso!",
                    imagemUrl = Url.Content(novaImagemPath)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao processar imagem: {ex.Message}" });
            }
        }

        // POST: Utilizadores/PedirSerVendedor
        [Authorize(Roles = "Comprador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PedirSerVendedor(string nif, string? dadosFaturacao, string motivacao)
        {
            if (string.IsNullOrWhiteSpace(nif) || string.IsNullOrWhiteSpace(motivacao))
            {
                return Json(new { success = false, message = "Por favor preencha todos os campos obrigatórios." });
            }

            // Validar NIF
            if (!IsValidNif(nif))
            {
                return Json(new { success = false, message = "NIF inválido." });
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var comprador = await _db.Compradores
                .Include(c => c.Morada)
                .FirstOrDefaultAsync(c => c.IdentityUserId == userId);

            if (comprador == null)
            {
                return Json(new { success = false, message = "Utilizador não encontrado." });
            }

            try
            {
                // Verificar se já tem pedido pendente
                var pedidoExistente = await _db.PedidosVendedor
                    .FirstOrDefaultAsync(p => p.CompradorId == comprador.Id && p.Estado == "Pendente");

                if (pedidoExistente != null)
                {
                    return Json(new { success = false, message = "Já tem um pedido pendente de aprovação." });
                }

                // Verificar se já é vendedor
                var jaEVendedor = await _userManager.IsInRoleAsync(
                    await _userManager.FindByIdAsync(userId.ToString()),
                    "Vendedor"
                );

                if (jaEVendedor)
                {
                    return Json(new { success = false, message = "Já é vendedor na plataforma." });
                }

                // Criar pedido
                var pedido = new PedidoVendedor
                {
                    CompradorId = comprador.Id,
                    Nif = nif,
                    DadosFaturacao = dadosFaturacao,
                    Motivacao = motivacao,
                    Estado = "Pendente",
                    DataPedido = DateTime.Now
                };

                _db.PedidosVendedor.Add(pedido);
                await _db.SaveChangesAsync();

                // Enviar email para administradores
                await EnviarEmailNoticePedidoVendedor(comprador, pedido);

                return Json(new
                {
                    success = true,
                    message = "Pedido enviado com sucesso! Aguarde aprovação do administrador."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao processar pedido: {ex.Message}" });
            }
        }

        private async Task EnviarEmailNoticePedidoVendedor(Comprador comprador, PedidoVendedor pedido)
        {
            try
            {
                // Buscar emails dos administradores
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");

                foreach (var admin in admins)
                {
                    if (!string.IsNullOrWhiteSpace(admin.Email))
                    {
                        var subject = "🔔 Novo Pedido para Tornar-se Vendedor - 404 Ride";
                        var message = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                                    <h1 style='color: white; margin: 0;'>404 Ride</h1>
                                    <p style='color: white; margin: 10px 0 0 0;'>Marketplace de Veículos Usados</p>
                                </div>
                                <div style='background: #f7f7f7; padding: 30px;'>
                                    <h2 style='color: #333; margin-top: 0;'>Novo Pedido de Vendedor</h2>
                                    <p>Um comprador solicitou tornar-se vendedor na plataforma:</p>

                                    <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                        <p><strong>Nome:</strong> {comprador.Nome}</p>
                                        <p><strong>Email:</strong> {comprador.Email}</p>
                                        <p><strong>NIF:</strong> {pedido.Nif}</p>
                                        <p><strong>Data do Pedido:</strong> {pedido.DataPedido:dd/MM/yyyy HH:mm}</p>
                                        <hr style='border: none; border-top: 1px solid #eee; margin: 15px 0;'>
                                        <p><strong>Motivação:</strong></p>
                                        <p style='background: #f9f9f9; padding: 15px; border-radius: 5px; border-left: 3px solid #667eea;'>
                                            {pedido.Motivacao}
                                        </p>
                                        {(!string.IsNullOrWhiteSpace(pedido.DadosFaturacao) ? $@"
                                        <p><strong>Dados de Faturação:</strong></p>
                                        <p style='background: #f9f9f9; padding: 15px; border-radius: 5px;'>
                                            {pedido.DadosFaturacao}
                                        </p>" : "")}
                                    </div>

                                    <p style='text-align: center; margin: 30px 0;'>
                                        <a href='{Url.Action("Index", "Administrador", null, Request.Scheme)}#validar-vendedores'
                                           style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                            Gerir Pedidos
                                        </a>
                                    </p>

                                    <p style='color: #666; font-size: 12px; text-align: center;'>
                                        Este é um email automático do sistema 404 Ride.<br>
                                        Por favor, não responda a este email.
                                    </p>
                                </div>
                            </div>";

                        await _emailSender.SendAsync(admin.Email, subject, message);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }
        }

        public async Task<IActionResult> PromoteMe()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);
            if (vendedor != null)
            {
                vendedor.Estado = "Ativo";
                await _db.SaveChangesAsync();
                return Content("Promoted!");
            }
            return Content("Not found");
        }
        // POST: Utilizadores/AvaliarVendedor
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvaliarVendedor(int vendedorId, int nota, string? comentario, string? returnUrl)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Obter o perfil de comprador do utilizador logado
            var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            
            if (comprador == null)
            {
                // Se for um vendedor a avaliar outro, verificar se existe perfil de comprador (criado anteriormente para msgs) ou criar
                if (User.IsInRole("Vendedor"))
                {
                    // Tentar encontrar/criar perfil de comprador para o vendedor
                    var vendedorLogado = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);
                    if (vendedorLogado != null && vendedorLogado.Id != vendedorId) // Não auto-avaliar
                    {
                        comprador = new Comprador
                        {
                            IdentityUserId = userId,
                            Username = vendedorLogado.Username,
                            Email = vendedorLogado.Email,
                            Nome = vendedorLogado.Nome,
                            PasswordHash = vendedorLogado.PasswordHash,
                            Estado = vendedorLogado.Estado,
                            ImagemPerfil = vendedorLogado.ImagemPerfil,
                            MoradaId = vendedorLogado.MoradaId
                        };
                        _db.Compradores.Add(comprador);
                        await _db.SaveChangesAsync();
                    }
                }
                
                if (comprador == null)
                {
                    TempData["ErroAvaliacao"] = "Apenas compradores podem avaliar vendedores.";
                    return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
                }
            }

            // Validar dados
            if (nota < 1 || nota > 5)
            {
                TempData["ErroAvaliacao"] = "A nota deve ser entre 1 e 5 estrelas.";
                return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
            }

            // Verificar se já avaliou recentemente (Intervalo de segurança de 1 semana)
            var ultimaAvaliacao = await _db.Avaliacoes
                .Where(a => a.VendedorId == vendedorId && a.CompradorId == comprador.Id)
                .OrderByDescending(a => a.Data)
                .FirstOrDefaultAsync();

            if (ultimaAvaliacao != null && ultimaAvaliacao.Data > DateTime.Now.AddDays(-7))
            {
                var diasRestantes = (ultimaAvaliacao.Data.AddDays(7).Date - DateTime.Now.Date).Days;
                // Se for 0 dias (mas horas diferentes), dizemos "amanhã" ou "brevemente"
                var avisoTempo = diasRestantes > 0 ? $"{diasRestantes} dias" : "brevemente";
                
                TempData["ErroAvaliacao"] = $"Já avaliou este vendedor recentemente. Poderá avaliar novamente em {avisoTempo}.";
                return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
            }

            var avaliacao = new Avaliacao
            {
                VendedorId = vendedorId,
                CompradorId = comprador.Id,
                Nota = nota,
                Comentario = comentario,
                Data = DateTime.Now
            };

            _db.Avaliacoes.Add(avaliacao);
            await _db.SaveChangesAsync();

            TempData["SucessoAvaliacao"] = "Avaliação enviada com sucesso!";
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}




