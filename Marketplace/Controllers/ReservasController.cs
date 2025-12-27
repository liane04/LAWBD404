using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Marketplace.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public ReservasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar comprador
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
                return Forbid();

            var reservas = await _context.Reservas
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Imagens)
                .Where(r => r.CompradorId == comprador.Id)
                .OrderByDescending(r => r.Data)
                .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/Create?anuncioId=5
        [HttpGet]
        public async Task<IActionResult> Create(int? anuncioId)
        {
            if (anuncioId == null)
                return NotFound();

            var anuncio = await _context.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null)
                return NotFound();

            // Verificar se anúncio existe
            // (Nota: O modelo Anuncio não tem propriedade Estado)

            // Verificar se o usuário não é o vendedor
            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
            {
                TempData["Error"] = "Apenas compradores podem fazer reservas.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            if (anuncio.VendedorId == comprador.Id)
            {
                TempData["Error"] = "Não pode reservar o seu próprio anúncio.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            // Verificar se já existe reserva ativa
            var reservaExistente = await _context.Reservas
                .AnyAsync(r => r.AnuncioId == anuncioId &&
                              (r.Estado == "Ativa" || r.Estado == "Pendente"));

            if (reservaExistente)
            {
                TempData["Error"] = "Já existe uma reserva ativa para este veículo.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            ViewBag.Anuncio = anuncio;
            // Usar o ValorSinal definido pelo vendedor
            ViewBag.ValorReserva = anuncio.ValorSinal > 0 ? anuncio.ValorSinal : anuncio.Preco * 0.1m;

            return View();
        }

        // POST: Reservas/CreateCheckoutSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(int anuncioId)
        {
            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
                return Forbid();

            var anuncio = await _context.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null)
                return NotFound();

            // Usar o ValorSinal definido pelo vendedor (ou 10% se não estiver definido)
            var valorReserva = anuncio.ValorSinal > 0 ? anuncio.ValorSinal : anuncio.Preco * 0.1m;

            // Criar a sessão de checkout do Stripe
            var domain = $"{Request.Scheme}://{Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = _configuration["Stripe:Currency"] ?? "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Reserva: {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}",
                                Description = $"Reserva do veículo - {anuncio.Titulo}",
                                Images = anuncio.Imagens.Any()
                                    ? new List<string> { $"{domain}{anuncio.Imagens.First().ImagemCaminho}" }
                                    : null
                            },
                            UnitAmount = (long)(valorReserva * 100), // Stripe usa centavos
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/Reservas/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Reservas/Cancel?anuncioId={anuncioId}",
                Metadata = new Dictionary<string, string>
                {
                    { "anuncio_id", anuncioId.ToString() },
                    { "comprador_id", comprador.Id.ToString() },
                    { "valor_total", anuncio.Preco.ToString("F2") }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            // Guardar session ID temporariamente
            TempData["StripeSessionId"] = session.Id;
            TempData["AnuncioId"] = anuncioId;

            return Redirect(session.Url);
        }

        // GET: Reservas/Success
        public async Task<IActionResult> Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
                return RedirectToAction("Index", "Home");

            try
            {
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(session_id);

                if (session.PaymentStatus == "paid")
                {
                    // Extrair metadata
                    var anuncioId = int.Parse(session.Metadata["anuncio_id"]);
                    var compradorId = int.Parse(session.Metadata["comprador_id"]);

                    // Verificar se a reserva já foi criada (evitar duplicados)
                    var reservaExistente = await _context.Reservas
                        .FirstOrDefaultAsync(r => r.AnuncioId == anuncioId &&
                                                  r.CompradorId == compradorId &&
                                                  r.Estado == "Ativa");

                    if (reservaExistente == null)
                    {
                        // Criar a reserva
                        var reserva = new Reserva
                        {
                            AnuncioId = anuncioId,
                            CompradorId = compradorId,
                            Data = DateTime.Now,
                            Estado = "Ativa",
                            DataExpiracao = DateTime.Now.AddDays(7) // Reserva válida por 7 dias
                        };

                        _context.Reservas.Add(reserva);

                        // Buscar anúncio para enviar emails
                        var anuncio = await _context.Anuncios
                            .Include(a => a.Vendedor)
                            .Include(a => a.Marca)
                            .Include(a => a.Modelo)
                            .FirstOrDefaultAsync(a => a.Id == anuncioId);

                        if (anuncio != null)
                        {
                            // Nota: O modelo Anuncio não tem propriedade Estado

                            var comprador = await _context.Compradores
                                .FirstOrDefaultAsync(c => c.Id == compradorId);

                            // Enviar email ao vendedor
                            try
                            {
                                await _emailSender.SendAsync(
                                    anuncio.Vendedor.Email,
                                    "Novo Veículo Reservado - 404 Ride",
                                    $@"<h2>Veículo Reservado</h2>
                                    <p>Olá {anuncio.Vendedor.Nome},</p>
                                    <p>O seu veículo <strong>{anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</strong> foi reservado!</p>
                                    <p><strong>Comprador:</strong> {comprador?.Nome}</p>
                                    <p><strong>Email:</strong> {comprador?.Email}</p>
                                    <p><strong>Data da Reserva:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                                    <p>O comprador tem 7 dias para concluir a compra. Entre em contacto para combinar os detalhes.</p>"
                                );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                            }

                            // Enviar email ao comprador
                            try
                            {
                                await _emailSender.SendAsync(
                                    comprador?.Email ?? "",
                                    "Reserva Confirmada - 404 Ride",
                                    $@"<h2>Reserva Confirmada</h2>
                                    <p>Olá {comprador?.Nome},</p>
                                    <p>A sua reserva do veículo <strong>{anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</strong> foi confirmada!</p>
                                    <p><strong>Valor pago:</strong> {session.AmountTotal / 100:C}</p>
                                    <p><strong>Validade:</strong> 7 dias</p>
                                    <p>O vendedor irá contactá-lo em breve para combinar os detalhes da compra.</p>
                                    <p>Pode ver os detalhes da reserva na sua área pessoal.</p>"
                                );
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    ViewBag.SessionId = session_id;
                    ViewBag.AmountPaid = session.AmountTotal / 100;
                    ViewBag.AnuncioId = anuncioId;

                    return View();
                }
                else
                {
                    return RedirectToAction("Cancel");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar pagamento: {ex.Message}");
                TempData["Error"] = "Erro ao processar a reserva. Contacte o suporte.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reservas/Cancel
        public IActionResult Cancel(int? anuncioId)
        {
            ViewBag.AnuncioId = anuncioId;
            return View();
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
                return Forbid();

            var reserva = await _context.Reservas
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Imagens)
                .Include(r => r.Anuncio)
                    .ThenInclude(a => a.Vendedor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
                return NotFound();

            // Verificar se o usuário é o comprador da reserva
            if (reserva.CompradorId != comprador.Id)
                return Forbid();

            return View(reserva);
        }

        // POST: Reservas/Cancel/5
        [HttpPost, ActionName("CancelReserva")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReserva(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
                return Forbid();

            var reserva = await _context.Reservas
                .Include(r => r.Anuncio)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
                return NotFound();

            if (reserva.CompradorId != comprador.Id)
                return Forbid();

            // Cancelar reserva
            reserva.Estado = "Cancelada";

            // Nota: O modelo Anuncio não tem propriedade Estado
            // O anúncio fica automaticamente disponível quando não houver reserva ativa

            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva cancelada com sucesso.";
            return RedirectToAction(nameof(Index));
        }
    }
}
