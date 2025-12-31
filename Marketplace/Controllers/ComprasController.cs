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
    public class ComprasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public ComprasController(
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

        // GET: Compras
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar comprador (vendedores também podem ter compras após criarem comprador)
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
            {
                // Se não tem comprador, verificar se é vendedor
                var vendedor = await _context.Vendedores
                    .FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

                if (vendedor == null)
                    return Forbid();

                // Ainda não tem compras pois não tem comprador
                return View(new List<Compra>());
            }

            var compras = await _context.Compras
                .Include(c => c.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(c => c.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(c => c.Anuncio)
                    .ThenInclude(a => a.Imagens)
                .Include(c => c.Anuncio)
                    .ThenInclude(a => a.Vendedor)
                .Where(c => c.CompradorId == comprador.Id)
                .OrderByDescending(c => c.Data)
                .ToListAsync();

            return View(compras);
        }

        // API: Verificar se o utilizador tem reserva ativa para o anúncio
        [HttpGet]
        public async Task<IActionResult> VerificarReserva(int anuncioId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { temReserva = false, valorSinal = 0 });

            // Buscar comprador
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
            {
                // Se não tem comprador, não tem reserva
                return Json(new { temReserva = false, valorSinal = 0 });
            }

            // Buscar reserva ativa do comprador para este anúncio
            var reserva = await _context.Reservas
                .Include(r => r.Anuncio)
                .FirstOrDefaultAsync(r =>
                    r.AnuncioId == anuncioId &&
                    r.CompradorId == comprador.Id &&
                    r.Estado == "Ativa" &&
                    r.DataExpiracao > DateTime.Now);

            if (reserva != null)
            {
                // Usar o ValorSinal do anúncio (que foi o valor pago na reserva)
                var valorSinal = reserva.Anuncio.ValorSinal > 0
                    ? reserva.Anuncio.ValorSinal
                    : reserva.Anuncio.Preco * 0.1m;

                return Json(new { temReserva = true, valorSinal = valorSinal, reservaId = reserva.Id });
            }

            return Json(new { temReserva = false, valorSinal = 0 });
        }

        // POST: Compras/CreateCheckoutSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(int anuncioId)
        {
            var user = await _userManager.GetUserAsync(User);

            var anuncio = await _context.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null)
                return NotFound();

            // Verificar se o usuário é o vendedor do anúncio
            var vendedor = await _context.Vendedores
                .FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor != null && anuncio.VendedorId == vendedor.Id)
            {
                TempData["Error"] = "Não pode comprar o seu próprio anúncio.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            // Obter ou criar comprador (vendedores também podem comprar)
            var comprador = await _context.Compradores
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
            {
                // Se não existe comprador mas existe vendedor, criar comprador automaticamente
                if (vendedor != null)
                {
                    comprador = new Comprador
                    {
                        IdentityUserId = user.Id,
                        Username = user.UserName ?? "",
                        Email = user.Email ?? "",
                        Nome = vendedor.Nome,
                        Estado = "Ativo",
                        Tipo = "Comprador",
                        ImagemPerfil = vendedor.ImagemPerfil,
                        MoradaId = vendedor.MoradaId
                    };
                    _context.Compradores.Add(comprador);
                    await _context.SaveChangesAsync();

                    // Adicionar role "Comprador" ao utilizador se não tiver
                    if (!await _userManager.IsInRoleAsync(user, "Comprador"))
                    {
                        await _userManager.AddToRoleAsync(user, "Comprador");
                    }
                }
                else
                {
                    TempData["Error"] = "É necessário ter uma conta de comprador ou vendedor para fazer compras.";
                    return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
                }
            }

            // Verificar se já comprou
            var jaComprou = await _context.Compras
                .AnyAsync(c => c.AnuncioId == anuncioId && c.CompradorId == comprador.Id);

            if (jaComprou)
            {
                TempData["Error"] = "Já registou uma compra para este anúncio.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            // Verificar se tem reserva ativa e calcular valor
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r =>
                    r.AnuncioId == anuncioId &&
                    r.CompradorId == comprador.Id &&
                    r.Estado == "Ativa" &&
                    r.DataExpiracao > DateTime.Now);

            decimal valorAPagar = anuncio.Preco;
            decimal valorSinalPago = 0;

            if (reserva != null)
            {
                // Deduzir o valor do sinal
                valorSinalPago = anuncio.ValorSinal > 0 ? anuncio.ValorSinal : anuncio.Preco * 0.1m;
                valorAPagar = anuncio.Preco - valorSinalPago;
            }

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
                                Name = $"Compra: {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}",
                                Description = reserva != null
                                    ? $"{anuncio.Titulo} (Valor restante após dedução do sinal de {valorSinalPago:N0}€)"
                                    : $"{anuncio.Titulo}",
                                Images = anuncio.Imagens.Any()
                                    ? new List<string> { $"{domain}{anuncio.Imagens.First().ImagemCaminho}" }
                                    : null
                            },
                            UnitAmount = (long)(valorAPagar * 100), // Stripe usa centavos
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/Compras/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Compras/Cancel?anuncioId={anuncioId}",
                Metadata = new Dictionary<string, string>
                {
                    { "anuncio_id", anuncioId.ToString() },
                    { "comprador_id", comprador.Id.ToString() },
                    { "valor_total", anuncio.Preco.ToString("F2") },
                    { "valor_sinal_pago", valorSinalPago.ToString("F2") },
                    { "tem_reserva", (reserva != null).ToString() },
                    { "reserva_id", reserva?.Id.ToString() ?? "0" }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }

        // GET: Compras/Success
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
                    var temReserva = bool.Parse(session.Metadata["tem_reserva"]);
                    var reservaId = int.Parse(session.Metadata["reserva_id"]);

                    // Verificar se a compra já foi criada (evitar duplicados)
                    var compraExistente = await _context.Compras
                        .FirstOrDefaultAsync(c => c.AnuncioId == anuncioId &&
                                                  c.CompradorId == compradorId &&
                                                  c.EstadoPagamento == "Pago");

                    if (compraExistente == null)
                    {
                        // Criar a compra
                        var compra = new Compra
                        {
                            AnuncioId = anuncioId,
                            CompradorId = compradorId,
                            Data = DateTime.Now,
                            EstadoPagamento = "Pago"
                        };

                        _context.Compras.Add(compra);

                        // Marcar anúncio como vendido
                        var anuncioVendido = await _context.Anuncios.FindAsync(anuncioId);
                        if (anuncioVendido != null)
                        {
                            anuncioVendido.Estado = "Vendido";
                        }

                        // Se tinha reserva, marcar como concluída
                        if (temReserva && reservaId > 0)
                        {
                            var reserva = await _context.Reservas.FindAsync(reservaId);
                            if (reserva != null)
                            {
                                reserva.Estado = "Concluída";
                            }
                        }

                        // Buscar anúncio para enviar emails
                        var anuncio = await _context.Anuncios
                            .Include(a => a.Vendedor)
                            .Include(a => a.Marca)
                            .Include(a => a.Modelo)
                            .FirstOrDefaultAsync(a => a.Id == anuncioId);

                        var comprador = await _context.Compradores
                            .FirstOrDefaultAsync(c => c.Id == compradorId);

                        if (anuncio != null && comprador != null)
                        {
                            var valorTotalPago = (session.AmountTotal ?? 0) / 100m;
                            var valorSinalPago = decimal.Parse(session.Metadata["valor_sinal_pago"]);

                            // Enviar email ao vendedor
                            try
                            {
                                await _emailSender.SendAsync(
                                    anuncio.Vendedor.Email,
                                    "Veículo Vendido - 404 Ride",
                                    GetEmailVendedorVendido(anuncio, comprador, valorTotalPago, valorSinalPago, temReserva)
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
                                    comprador.Email ?? "",
                                    "Compra Confirmada - 404 Ride",
                                    GetEmailCompradorConfirmacao(anuncio, comprador, valorTotalPago, valorSinalPago, temReserva)
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
                TempData["Error"] = "Erro ao processar a compra. Contacte o suporte.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Compras/Cancel
        public IActionResult Cancel(int? anuncioId)
        {
            ViewBag.AnuncioId = anuncioId;
            return View();
        }

        // Templates de Email Estilizados
        private string GetEmailVendedorVendido(Anuncio anuncio, Comprador comprador, decimal valorPago, decimal valorSinal, bool tinhaReserva)
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
        .price-info {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; font-size: 12px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #2563eb; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .success-icon {{ font-size: 60px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>🎉</div>
            <h1>Veículo Vendido!</h1>
            <p>Parabéns! O seu veículo foi vendido com sucesso</p>
        </div>
        <div class='content'>
            <p>Olá <strong>{anuncio.Vendedor.Nome}</strong>,</p>
            <p>Temos excelentes notícias! O seu veículo foi vendido através da plataforma 404 Ride.</p>

            <div class='vehicle-info'>
                <h3 style='margin-top:0; color: #2563eb;'>📋 Detalhes do Veículo</h3>
                <p><strong>Veículo:</strong> {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</p>
                <p><strong>Título:</strong> {anuncio.Titulo}</p>
                <p><strong>Ano:</strong> {anuncio.Ano}</p>
            </div>

            <div class='buyer-info'>
                <h3 style='margin-top:0; color: #10b981;'>👤 Informações do Comprador</h3>
                <p><strong>Nome:</strong> {comprador.Nome}</p>
                <p><strong>Email:</strong> {comprador.Email}</p>
                <p><strong>Data da Compra:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>

            <div class='price-info'>
                <h3 style='margin-top:0; color: #f59e0b;'>💰 Detalhes do Pagamento</h3>
                <p><strong>Valor Total do Veículo:</strong> {anuncio.Preco:N2}€</p>
                {(tinhaReserva ? $"<p><strong>Sinal Recebido Anteriormente:</strong> {valorSinal:N2}€</p>" : "")}
                <p><strong>Valor Recebido Agora:</strong> {valorPago:N2}€</p>
                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 15px 0;'>
                <p style='font-size: 18px;'><strong>Total Recebido:</strong> {(tinhaReserva ? valorPago + valorSinal : valorPago):N2}€</p>
            </div>

            <h3>📞 Próximos Passos</h3>
            <ol>
                <li>Entre em contacto com o comprador para combinar a entrega do veículo</li>
                <li>Prepare toda a documentação necessária para a transferência</li>
                <li>Confirme a data e local da entrega</li>
            </ol>

            <p style='color: #64748b; font-size: 14px; margin-top: 30px;'>
                <strong>Dica:</strong> Certifique-se de que toda a documentação está em ordem antes da entrega.
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

        private string GetEmailCompradorConfirmacao(Anuncio anuncio, Comprador comprador, decimal valorPago, decimal valorSinal, bool tinhaReserva)
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
        .footer {{ background: #1e293b; color: #94a3b8; padding: 20px; text-align: center; font-size: 12px; }}
        .success-icon {{ font-size: 60px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='success-icon'>✅</div>
            <h1>Compra Confirmada!</h1>
            <p>Parabéns pela sua nova aquisição</p>
        </div>
        <div class='content'>
            <p>Olá <strong>{comprador.Nome}</strong>,</p>
            <p>A sua compra foi confirmada com sucesso! Obrigado por escolher a 404 Ride.</p>

            <div class='vehicle-info'>
                <h3 style='margin-top:0; color: #2563eb;'>🚗 O Seu Novo Veículo</h3>
                <p><strong>Veículo:</strong> {anuncio.Marca?.Nome} {anuncio.Modelo?.Nome}</p>
                <p><strong>Ano:</strong> {anuncio.Ano}</p>
                <p><strong>Quilometragem:</strong> {anuncio.Quilometragem:N0} km</p>
                <p><strong>Combustível:</strong> {anuncio.Combustivel?.Tipo}</p>
            </div>

            <div class='payment-info'>
                <h3 style='margin-top:0; color: #10b981;'>💳 Resumo do Pagamento</h3>
                <p><strong>Valor Total do Veículo:</strong> {anuncio.Preco:N2}€</p>
                {(tinhaReserva ? $"<p><strong>Sinal Pago Anteriormente:</strong> {valorSinal:N2}€</p>" : "")}
                <p><strong>Valor Pago Agora:</strong> {valorPago:N2}€</p>
                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 15px 0;'>
                <p style='font-size: 18px;'><strong>Total Pago:</strong> {(tinhaReserva ? valorPago + valorSinal : valorPago):N2}€</p>
                <p style='color: #10b981;'>✓ Pagamento Confirmado</p>
            </div>

            <h3>📞 Próximos Passos</h3>
            <ol>
                <li>O vendedor entrará em contacto consigo nas próximas 24-48 horas</li>
                <li>Combinará consigo a data e local para entrega do veículo</li>
                <li>Prepare a documentação necessária para a transferência</li>
            </ol>

            <h3>👤 Dados do Vendedor</h3>
            <p><strong>Nome:</strong> {anuncio.Vendedor.Nome}</p>
            <p><strong>Email:</strong> {anuncio.Vendedor.Email}</p>

            <p style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; border-radius: 5px; margin-top: 30px;'>
                <strong>⚠️ Importante:</strong> Verifique toda a documentação do veículo antes de finalizar a transação.
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
