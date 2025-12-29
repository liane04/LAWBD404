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

                        // Buscar anúncio para enviar emails e marcar como reservado
                        var anuncio = await _context.Anuncios
                            .Include(a => a.Vendedor)
                            .Include(a => a.Marca)
                            .Include(a => a.Modelo)
                            .FirstOrDefaultAsync(a => a.Id == anuncioId);

                        if (anuncio != null)
                        {
                            // Marcar anúncio como reservado
                            anuncio.Estado = "Reservado";

                            var comprador = await _context.Compradores
                                .FirstOrDefaultAsync(c => c.Id == compradorId);

                            var domain = $"{Request.Scheme}://{Request.Host}";
                            var linkAnuncio = $"{domain}/Anuncios/Details/{anuncio.Id}";
                            var valorSinal = (session.AmountTotal ?? 0) / 100m;

                            // Enviar email ao vendedor
                            try
                            {
                                await _emailSender.SendAsync(
                                    anuncio.Vendedor.Email,
                                    "Novo Veículo Reservado - 404 Ride",
                                    GetEmailVendedorReserva(anuncio, comprador, valorSinal, linkAnuncio)
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
                                    GetEmailCompradorReserva(anuncio, comprador, valorSinal, linkAnuncio)
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

        // Templates de Email Estilizados para Reservas
        private string GetEmailVendedorReserva(Anuncio anuncio, Comprador comprador, decimal valorSinal, string linkAnuncio)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 30px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #2563eb 0%, #1e40af 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .vehicle-info {{ background: #f8fafc; border-left: 4px solid #2563eb; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .buyer-info {{ background: #ecfdf5; border-left: 4px solid #10b981; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; font-size: 12px; }}
        .icon {{ font-size: 60px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>🎯</div>
            <h1>Veículo Reservado!</h1>
            <p>O seu veículo foi reservado por um comprador</p>
        </div>
        <div class='content'>
            <p>Olá <strong>{anuncio.Vendedor.Nome}</strong>,</p>
            <p>Temos boas notícias! O seu veículo foi reservado através da plataforma 404 Ride.</p>

            <div class='vehicle-info'>
                <h3 style='margin-top:0; color: #2563eb;'>🚗 Detalhes do Veículo</h3>
                <p><strong>Veículo:</strong> {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</p>
                <p><strong>Título:</strong> {anuncio.Titulo}</p>
                <p><strong>Ano:</strong> {anuncio.Ano}</p>
                <p><strong>Valor do Sinal Recebido:</strong> {valorSinal:N2}€</p>
            </div>

            <div class='buyer-info'>
                <h3 style='margin-top:0; color: #10b981;'>👤 Informações do Comprador</h3>
                <p><strong>Nome:</strong> {comprador?.Nome}</p>
                <p><strong>Email:</strong> {comprador?.Email}</p>
                <p><strong>Data da Reserva:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>

            <h3>📞 Próximos Passos</h3>
            <ol>
                <li>O comprador tem 7 dias para concluir a compra</li>
                <li>Entre em contacto para combinar uma visita</li>
                <li>Prepare a documentação do veículo</li>
            </ol>

            <p style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 5px; margin-top: 30px;'>
                <strong>💡 Dica:</strong> Responda rapidamente ao comprador para garantir uma venda bem-sucedida!
            </p>
        </div>
        <div class='footer'>
            <p>© 2025 404 Ride - Marketplace de Veículos</p>
            <p>Este é um email automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetEmailCompradorReserva(Anuncio anuncio, Comprador comprador, decimal valorSinal, string linkAnuncio)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 30px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .vehicle-info {{ background: #f0f9ff; border-left: 4px solid #2563eb; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .payment-info {{ background: #f0fdf4; border-left: 4px solid #10b981; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #2563eb; color: white !important; text-decoration: none; border-radius: 8px; margin: 20px 0; font-weight: bold; text-align: center; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; font-size: 12px; }}
        .icon {{ font-size: 60px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>✅</div>
            <h1>Reserva Confirmada!</h1>
            <p>O veículo está reservado para si</p>
        </div>
        <div class='content'>
            <p>Olá <strong>{comprador?.Nome}</strong>,</p>
            <p>A sua reserva foi confirmada com sucesso! O veículo está agora reservado exclusivamente para si.</p>

            <div class='vehicle-info'>
                <h3 style='margin-top:0; color: #2563eb;'>🚗 Veículo Reservado</h3>
                <p><strong>Veículo:</strong> {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</p>
                <p><strong>Ano:</strong> {anuncio.Ano}</p>
                <p><strong>Quilometragem:</strong> {anuncio.Quilometragem:N0} km</p>
                <p><strong>Combustível:</strong> {anuncio.Combustivel?.Tipo}</p>
            </div>

            <div class='payment-info'>
                <h3 style='margin-top:0; color: #10b981;'>💰 Detalhes do Pagamento</h3>
                <p><strong>Preço Total do Veículo:</strong> {anuncio.Preco:N2}€</p>
                <p><strong>Sinal Pago:</strong> {valorSinal:N2}€</p>
                <p><strong>Valor Restante a Pagar:</strong> {(anuncio.Preco - valorSinal):N2}€</p>
                <p style='color: #10b981;'>✓ Sinal Confirmado</p>
            </div>

            <h3>⏰ Validade da Reserva</h3>
            <p>A sua reserva é válida por <strong>7 dias</strong> (até {DateTime.Now.AddDays(7):dd/MM/yyyy}).</p>
            <p>Durante este período, o vendedor não pode vender o veículo a outros compradores.</p>

            <div style='text-align: center; margin: 30px 0;'>
                <a href='{linkAnuncio}' class='button'>
                    🛒 Concluir Compra e Pagar Restante
                </a>
            </div>

            <h3>📞 Próximos Passos</h3>
            <ol>
                <li>O vendedor entrará em contacto consigo para agendar uma visita</li>
                <li>Visite o veículo e confirme se está tudo conforme descrito</li>
                <li>Clique no botão acima para concluir a compra e pagar o valor restante</li>
                <li>Combine a entrega com o vendedor</li>
            </ol>

            <p style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 5px; margin-top: 30px;'>
                <strong>⚠️ Importante:</strong> Se não concluir a compra dentro de 7 dias, a reserva expirará e o sinal não será reembolsado.
            </p>
        </div>
        <div class='footer'>
            <p>© 2025 404 Ride - Marketplace de Veículos</p>
            <p>Este é um email automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
