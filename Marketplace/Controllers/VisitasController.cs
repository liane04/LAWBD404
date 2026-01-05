using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    [Authorize]
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public VisitasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Visitas
        // Lista de visitas do utilizador atual (comprador ou vendedor)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            Console.WriteLine($"[DEBUG INDEX] User Identity ID: {user.Id}, UserName: {user.UserName}");

            // Buscar utilizador de domínio (Comprador, Vendedor ou Administrador)
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);

            if (domainUser == null)
            {
                Console.WriteLine($"[ERRO INDEX] Nenhum utilizador de domínio encontrado para IdentityUserId: {user.Id}");
                return NotFound();
            }

            Console.WriteLine($"[DEBUG INDEX] DomainUser encontrado: Id={domainUser.Id}, Nome={domainUser.Nome}, Tipo={domainUser.GetType().Name}");

            IQueryable<Visita> visitasQuery = _context.Visitas
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Vendedor);
            // NOTA: Não incluir v.Comprador e v.Vendedor diretamente porque causa problemas
            // quando um Vendedor agenda visita (CompradorId aponta para Vendedor, não Comprador)

            if (domainUser is Comprador)
            {
                // Compradores veem suas próprias visitas agendadas
                visitasQuery = visitasQuery.Where(v => v.CompradorId == domainUser.Id);
                ViewBag.UserRole = "Comprador";
                Console.WriteLine($"[DEBUG INDEX] Filtrando visitas para Comprador ID={domainUser.Id}");
            }
            else if (domainUser is Vendedor)
            {
                // Vendedores veem AMBAS:
                // 1. Visitas aos seus anúncios (como vendedor)
                // 2. Visitas que agendaram (como comprador)
                visitasQuery = visitasQuery.Where(v => v.VendedorId == domainUser.Id || v.CompradorId == domainUser.Id);
                ViewBag.UserRole = "Vendedor";
                ViewBag.VendedorId = domainUser.Id; // Para separar na view
                Console.WriteLine($"[DEBUG INDEX] Filtrando visitas para Vendedor ID={domainUser.Id} (VendedorId OU CompradorId)");
            }
            else
            {
                return Forbid();
            }

            var visitas = await visitasQuery
                .OrderByDescending(v => v.Data)
                .ToListAsync();

            // Carregar Comprador e Vendedor como Utilizador para evitar problemas de discriminador
            // Isto é necessário porque um Vendedor pode agendar visitas (CompradorId aponta para Vendedor)
            foreach (var visita in visitas)
            {
                if (visita.Comprador == null)
                {
                    visita.Comprador = await _context.Set<Utilizador>()
                        .FirstOrDefaultAsync(u => u.Id == visita.CompradorId) as Comprador;

                    // Se não for Comprador, é um Vendedor agindo como comprador
                    if (visita.Comprador == null)
                    {
                        var utilizador = await _context.Set<Utilizador>()
                            .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);

                        // Criar um objeto temporário para exibição com os dados do utilizador
                        if (utilizador != null)
                        {
                            visita.Comprador = new Comprador
                            {
                                Id = utilizador.Id,
                                Nome = utilizador.Nome,
                                Email = utilizador.Email
                            };
                        }
                    }
                }

                if (visita.Vendedor == null)
                {
                    visita.Vendedor = await _context.Set<Utilizador>()
                        .FirstOrDefaultAsync(u => u.Id == visita.VendedorId) as Vendedor;

                    if (visita.Vendedor == null)
                    {
                        var utilizador = await _context.Set<Utilizador>()
                            .FirstOrDefaultAsync(u => u.Id == visita.VendedorId);

                        if (utilizador != null)
                        {
                            visita.Vendedor = new Vendedor
                            {
                                Id = utilizador.Id,
                                Nome = utilizador.Nome,
                                Email = utilizador.Email
                            };
                        }
                    }
                }
            }

            Console.WriteLine($"[DEBUG INDEX] Total de visitas encontradas: {visitas.Count}");
            if (visitas.Any())
            {
                Console.WriteLine($"[DEBUG INDEX] Primeira visita: Id={visitas.First().Id}, CompradorId={visitas.First().CompradorId}, VendedorId={visitas.First().VendedorId}");
            }

            return View(visitas);
        }

        // GET: Visitas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var visita = await _context.Visitas
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Imagens)
                .Include(v => v.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            // NOTA: Não incluir v.Comprador e v.Vendedor devido a problemas de discriminador TPH

            if (visita == null)
                return NotFound();

            // Carregar Comprador e Vendedor manualmente como Utilizador
            if (visita.Comprador == null)
            {
                var utilizadorComprador = await _context.Set<Utilizador>()
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

            if (visita.Vendedor == null)
            {
                var utilizadorVendedor = await _context.Set<Utilizador>()
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

            // Verificar se o utilizador tem permissão para ver esta visita
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null)
            {
                Console.WriteLine($"[ERRO Details] Utilizador de domínio não encontrado para IdentityUserId: {user.Id}");
                return Forbid();
            }

            // Permitir acesso se for o comprador OU o vendedor da visita
            if (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId)
            {
                Console.WriteLine($"[ERRO Details] Acesso negado. DomainUser.Id={domainUser.Id}, CompradorId={visita.CompradorId}, VendedorId={visita.VendedorId}");
                return Forbid();
            }

            Console.WriteLine($"[DEBUG Details] Acesso permitido. DomainUser.Id={domainUser.Id}, Nome={domainUser.Nome}");
            return View(visita);
        }

        // GET: Visitas/Create?anuncioId=5
        [Authorize(Roles = "Comprador,Vendedor")]
        public async Task<IActionResult> Create(int? anuncioId)
        {
            if (anuncioId == null)
                return BadRequest("ID do anúncio é obrigatório");

            var anuncio = await _context.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null)
                return NotFound("Anúncio não encontrado");

            var user = await _userManager.GetUserAsync(User);

            // Buscar Comprador ou Vendedor (TPH - ambos são Utilizador)
            int? compradorId = null;
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador != null)
            {
                compradorId = comprador.Id;
            }
            else if (User.IsInRole("Vendedor"))
            {
                // Vendedor pode agendar visitas - usar o ID do Vendedor (é um Utilizador)
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
                if (vendedor != null)
                {
                    compradorId = vendedor.Id; // TPH: Vendedor e Comprador são Utilizador
                }
            }

            if (compradorId == null)
            {
                TempData["VisitasErro"] = "Precisa de entrar como comprador para agendar visitas.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncio.Id });
            }

            // Buscar disponibilidades do vendedor
            var disponibilidades = await _context.DisponibilidadesVendedor
                .Where(d => d.VendedorId == anuncio.VendedorId && d.Ativo)
                .OrderBy(d => d.DiaSemana)
                .ThenBy(d => d.HoraInicio)
                .ToListAsync();

            // Gerar slots de horários disponíveis para os próximos 30 dias
            var slotsDisponiveis = new List<DateTime>();
            var dataInicio = DateTime.Now.AddHours(1);
            var dataFim = DateTime.Now.AddDays(30);

            for (var data = dataInicio.Date; data <= dataFim; data = data.AddDays(1))
            {
                var diaSemana = (int)data.DayOfWeek;
                var disponibilidadesDoDia = disponibilidades.Where(d => d.DiaSemana == diaSemana).ToList();

                foreach (var disp in disponibilidadesDoDia)
                {
                    var horaAtual = disp.HoraInicio;
                    while (horaAtual < disp.HoraFim)
                    {
                        var slotDateTime = data.Add(horaAtual);

                        // Apenas adicionar slots futuros
                        if (slotDateTime > DateTime.Now)
                        {
                            // Verificar se já existe visita agendada neste horário
                            var temConflito = await _context.Visitas
                                .AnyAsync(v => v.VendedorId == anuncio.VendedorId
                                            && v.Data == slotDateTime
                                            && v.Estado != "Cancelada");

                            if (!temConflito)
                            {
                                slotsDisponiveis.Add(slotDateTime);
                            }
                        }

                        horaAtual = horaAtual.Add(TimeSpan.FromMinutes(disp.IntervaloMinutos));
                    }
                }
            }

            var visita = new Visita
            {
                AnuncioId = anuncio.Id,
                Anuncio = anuncio,
                CompradorId = compradorId.Value,
                VendedorId = anuncio.VendedorId,
                Data = slotsDisponiveis.FirstOrDefault() != default ? slotsDisponiveis.First() : DateTime.Now.AddDays(1),
                Estado = "Pendente"
            };

            ViewBag.Anuncio = anuncio;
            ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            ViewBag.SlotsDisponiveis = slotsDisponiveis;
            ViewBag.TemDisponibilidades = disponibilidades.Any();

            return View(visita);
        }

        // POST: Visitas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Comprador,Vendedor")]
        public async Task<IActionResult> Create([Bind("AnuncioId,Data,Observacoes")] Visita visita)
        {
            var user = await _userManager.GetUserAsync(User);

            // Buscar Comprador ou Vendedor (TPH - ambos são Utilizador)
            int? compradorId = null;
            string nomeUtilizador = user.UserName ?? "Utilizador";

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador != null)
            {
                compradorId = comprador.Id;
                nomeUtilizador = comprador.Nome;
            }
            else if (User.IsInRole("Vendedor"))
            {
                // Vendedor pode agendar visitas - usar o ID do Vendedor (é um Utilizador)
                var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
                if (vendedor != null)
                {
                    compradorId = vendedor.Id; // TPH: Vendedor e Comprador são Utilizador
                    nomeUtilizador = vendedor.Nome;
                }
            }

            if (compradorId == null)
            {
                TempData["VisitasErro"] = "Precisa de entrar como comprador para agendar visitas.";
                return RedirectToAction("Details", "Anuncios", new { id = visita.AnuncioId });
            }

            var anuncio = await _context.Anuncios
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == visita.AnuncioId);

            if (anuncio == null)
                return NotFound("Anúncio não encontrado");

            // Validações de negócio
            if (visita.Data <= DateTime.Now)
            {
                ModelState.AddModelError("Data", "A data da visita deve ser futura");
            }

            // Verificar conflitos de horário (mesma data/hora para o mesmo vendedor)
            var conflito = await _context.Visitas
                .Where(v => v.VendedorId == anuncio.VendedorId
                         && v.Data == visita.Data
                         && v.Estado != "Cancelada")
                .AnyAsync();

            if (conflito)
            {
                ModelState.AddModelError("Data", "Já existe uma visita agendada para esta data/hora com este vendedor");
            }

            // DEBUG: Log para ver se há erros de validação
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"[DEBUG] ModelState inválido. Erros: {string.Join(", ", errors)}");
            }

            if (ModelState.IsValid)
            {
                visita.CompradorId = compradorId.Value;
                visita.VendedorId = anuncio.VendedorId;
                visita.Estado = "Pendente";
                visita.DataCriacao = DateTime.Now;

                Console.WriteLine($"[DEBUG] Tentando guardar visita - CompradorId: {visita.CompradorId}, VendedorId: {visita.VendedorId}, AnuncioId: {visita.AnuncioId}, Data: {visita.Data}");

                try
                {
                    _context.Add(visita);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[DEBUG] Visita guardada com sucesso! ID: {visita.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO] Falha ao guardar visita: {ex.Message}");
                    Console.WriteLine($"[ERRO] InnerException: {ex.InnerException?.Message}");
                    ModelState.AddModelError("", $"Erro ao guardar visita: {ex.Message}");

                    // Recarregar view com dados necessários
                    ViewBag.Anuncio = anuncio;
                    ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");

                    var disponibs = await _context.DisponibilidadesVendedor
                        .Where(d => d.VendedorId == anuncio.VendedorId && d.Ativo)
                        .OrderBy(d => d.DiaSemana)
                        .ThenBy(d => d.HoraInicio)
                        .ToListAsync();

                    ViewBag.SlotsDisponiveis = new List<DateTime>();
                    ViewBag.TemDisponibilidades = disponibs.Any();

                    return View(visita);
                }

                // Enviar email ao vendedor
                try
                {
                    await _emailSender.SendAsync(
                        anuncio.Vendedor.Email,
                        "Nova Visita Agendada - 404 Ride",
                        $@"<h2>Nova Visita Agendada</h2>
                        <p>Olá {anuncio.Vendedor.Nome},</p>
                        <p>Foi agendada uma nova visita ao seu anúncio:</p>
                        <ul>
                            <li><strong>Veículo:</strong> {anuncio.Titulo}</li>
                            <li><strong>Data:</strong> {visita.Data:dd/MM/yyyy HH:mm}</li>
                            <li><strong>Comprador:</strong> {nomeUtilizador}</li>
                            <li><strong>Observações:</strong> {visita.Observacoes ?? "Nenhuma"}</li>
                        </ul>
                        <p>Por favor, confirme a visita o mais breve possível.</p>
                        <p><a href='{Url.Action("Details", "Visitas", new { id = visita.Id }, Request.Scheme)}'>Ver Detalhes</a></p>"
                    );
                }
                catch (Exception ex)
                {
                    // Log do erro mas não bloqueia o processo
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                }

                TempData["SuccessMessage"] = $"Visita agendada com sucesso para {visita.Data:dd/MM/yyyy} às {visita.Data:HH:mm}! O vendedor {anuncio.Vendedor.Nome} será notificado por email.";
                TempData["VisitaId"] = visita.Id;
                return RedirectToAction(nameof(Index));
            }

            // Se chegou aqui, houve erro - recarregar view com dados necessários
            ViewBag.Anuncio = anuncio;
            ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");

            // Recalcular slots disponíveis para recarregar a view
            var disponibilidades = await _context.DisponibilidadesVendedor
                .Where(d => d.VendedorId == anuncio.VendedorId && d.Ativo)
                .OrderBy(d => d.DiaSemana)
                .ThenBy(d => d.HoraInicio)
                .ToListAsync();

            var slotsDisponiveis = new List<DateTime>();
            var dataInicio = DateTime.Now.AddHours(1);
            var dataFim = DateTime.Now.AddDays(30);

            for (var data = dataInicio.Date; data <= dataFim; data = data.AddDays(1))
            {
                var diaSemana = (int)data.DayOfWeek;
                var disponibilidadesDoDia = disponibilidades.Where(d => d.DiaSemana == diaSemana).ToList();

                foreach (var disp in disponibilidadesDoDia)
                {
                    var horaAtual = disp.HoraInicio;
                    while (horaAtual < disp.HoraFim)
                    {
                        var slotDateTime = data.Add(horaAtual);
                        if (slotDateTime > DateTime.Now)
                        {
                            var temConflito = await _context.Visitas
                                .AnyAsync(v => v.VendedorId == anuncio.VendedorId
                                            && v.Data == slotDateTime
                                            && v.Estado != "Cancelada");

                            if (!temConflito)
                                slotsDisponiveis.Add(slotDateTime);
                        }
                        horaAtual = horaAtual.Add(TimeSpan.FromMinutes(disp.IntervaloMinutos));
                    }
                }
            }

            ViewBag.SlotsDisponiveis = slotsDisponiveis;
            ViewBag.TemDisponibilidades = disponibilidades.Any();

            return View(visita);
        }

        // GET: Visitas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var visita = await _context.Visitas
                .Include(v => v.Anuncio)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            // Carregar Comprador e Vendedor manualmente
            if (visita.Comprador == null)
            {
                var utilizadorComprador = await _context.Set<Utilizador>()
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

            if (visita.Vendedor == null)
            {
                var utilizadorVendedor = await _context.Set<Utilizador>()
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

            // Verificar permissões
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null)
            {
                Console.WriteLine($"[ERRO Edit] Utilizador de domínio não encontrado para IdentityUserId: {user.Id}");
                return Forbid();
            }

            // Permitir acesso se for o comprador OU o vendedor da visita
            if (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId)
            {
                Console.WriteLine($"[ERRO Edit] Acesso negado. DomainUser.Id={domainUser.Id}, CompradorId={visita.CompradorId}, VendedorId={visita.VendedorId}");
                return Forbid();
            }

            ViewBag.IsVendedor = domainUser.Id == visita.VendedorId;
            ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");

            Console.WriteLine($"[DEBUG Edit] Acesso permitido. DomainUser.Id={domainUser.Id}, Nome={domainUser.Nome}");
            return View(visita);
        }

        // POST: Visitas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,Estado,Observacoes,AnuncioId,CompradorId,VendedorId,ReservaId,DataCriacao")] Visita visita)
        {
            if (id != visita.Id)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null)
            {
                Console.WriteLine($"[ERRO Edit POST] Utilizador de domínio não encontrado para IdentityUserId: {user.Id}");
                return Forbid();
            }

            // Permitir acesso se for o comprador OU o vendedor da visita
            if (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId)
            {
                Console.WriteLine($"[ERRO Edit POST] Acesso negado. DomainUser.Id={domainUser.Id}, CompradorId={visita.CompradorId}, VendedorId={visita.VendedorId}");
                return Forbid();
            }

            var visitaOriginal = await _context.Visitas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (visitaOriginal == null)
                return NotFound();

            // Validações de negócio
            if (visita.Data <= DateTime.Now && visita.Data != visitaOriginal.Data)
            {
                ModelState.AddModelError("Data", "A data da visita deve ser futura");
            }

            // Verificar conflitos de horário (se a data foi alterada)
            if (visita.Data != visitaOriginal.Data)
            {
                var conflito = await _context.Visitas
                    .Where(v => v.Id != id
                             && v.VendedorId == visita.VendedorId
                             && v.Data == visita.Data
                             && v.Estado != "Cancelada")
                    .AnyAsync();

                if (conflito)
                {
                    ModelState.AddModelError("Data", "Já existe uma visita agendada para esta data/hora");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    visita.DataAtualizacao = DateTime.Now;
                    _context.Update(visita);
                    await _context.SaveChangesAsync();

                    // Enviar notificação de alteração - buscar dados sem tracking
                    var anuncio = await _context.Anuncios.AsNoTracking().FirstOrDefaultAsync(a => a.Id == visita.AnuncioId);
                    var utilizadorComprador = await _context.Set<Utilizador>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == visita.CompradorId);
                    var utilizadorVendedor = await _context.Set<Utilizador>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == visita.VendedorId);

                    string destinatario = domainUser.Id == visita.VendedorId ? utilizadorComprador?.Email : utilizadorVendedor?.Email;
                    string nomeDestinatario = domainUser.Id == visita.VendedorId ? utilizadorComprador?.Nome : utilizadorVendedor?.Nome;

                    try
                    {
                        await _emailSender.SendAsync(
                            destinatario,
                            $"Visita {visita.Estado} - 404 Ride",
                            $@"<h2>Atualização de Visita</h2>
                            <p>Olá {nomeDestinatario},</p>
                            <p>A visita ao anúncio <strong>{anuncio.Titulo}</strong> foi atualizada:</p>
                            <ul>
                                <li><strong>Estado:</strong> {visita.Estado}</li>
                                <li><strong>Data:</strong> {visita.Data:dd/MM/yyyy HH:mm}</li>
                                <li><strong>Observações:</strong> {visita.Observacoes ?? "Nenhuma"}</li>
                            </ul>
                            <p><a href='{Url.Action("Details", "Visitas", new { id = visita.Id }, Request.Scheme)}'>Ver Detalhes</a></p>"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                    }

                    TempData["SuccessMessage"] = "Visita atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitaExists(visita.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IsVendedor = domainUser.Id == visita.VendedorId;
            ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");
            return View(visita);
        }

        // POST: Visitas/Confirmar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var visita = await _context.Visitas
                .Include(v => v.Anuncio)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null || vendedor.Id != visita.VendedorId)
                return Forbid();

            // Carregar dados do comprador para enviar email (sem anexar ao contexto)
            var utilizadorComprador = await _context.Set<Utilizador>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);

            visita.Estado = "Confirmada";
            visita.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            // Notificar comprador
            if (utilizadorComprador != null)
            {
                try
                {
                    await _emailSender.SendAsync(
                        utilizadorComprador.Email,
                        "Visita Confirmada - 404 Ride",
                        $@"<h2>Visita Confirmada</h2>
                        <p>Olá {utilizadorComprador.Nome},</p>
                        <p>A sua visita ao anúncio <strong>{visita.Anuncio.Titulo}</strong> foi confirmada!</p>
                        <ul>
                            <li><strong>Data:</strong> {visita.Data:dd/MM/yyyy HH:mm}</li>
                            <li><strong>Localização:</strong> {visita.Anuncio.Localizacao}</li>
                        </ul>
                        <p>Até breve!</p>"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                }
            }

            TempData["SuccessMessage"] = "Visita confirmada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Visitas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            var visita = await _context.Visitas
                .Include(v => v.Anuncio)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null)
            {
                Console.WriteLine($"[ERRO Cancelar] Utilizador de domínio não encontrado para IdentityUserId: {user.Id}");
                return Forbid();
            }

            // Permitir acesso se for o comprador OU o vendedor da visita
            if (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId)
            {
                Console.WriteLine($"[ERRO Cancelar] Acesso negado. DomainUser.Id={domainUser.Id}, CompradorId={visita.CompradorId}, VendedorId={visita.VendedorId}");
                return Forbid();
            }

            // Carregar dados do comprador e vendedor para enviar email (sem anexar ao contexto)
            var utilizadorComprador = await _context.Set<Utilizador>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == visita.CompradorId);

            var utilizadorVendedor = await _context.Set<Utilizador>()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == visita.VendedorId);

            visita.Estado = "Cancelada";
            visita.Observacoes = $"Cancelada: {motivo ?? "Sem motivo especificado"}";
            visita.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            // Notificar a outra parte
            string destinatario = domainUser.Id == visita.VendedorId ? utilizadorComprador?.Email : utilizadorVendedor?.Email;
            string nomeDestinatario = domainUser.Id == visita.VendedorId ? utilizadorComprador?.Nome : utilizadorVendedor?.Nome;

            try
            {
                await _emailSender.SendAsync(
                    destinatario,
                    "Visita Cancelada - 404 Ride",
                    $@"<h2>Visita Cancelada</h2>
                    <p>Olá {nomeDestinatario},</p>
                    <p>A visita ao anúncio <strong>{visita.Anuncio.Titulo}</strong> foi cancelada.</p>
                    <p><strong>Motivo:</strong> {motivo ?? "Não especificado"}</p>
                    <p><strong>Data original:</strong> {visita.Data:dd/MM/yyyy HH:mm}</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            TempData["SuccessMessage"] = "Visita cancelada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Visitas/Concluir/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Concluir(int id)
        {
            var visita = await _context.Visitas
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null || vendedor.Id != visita.VendedorId)
                return Forbid();

            visita.Estado = "Concluída";
            visita.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visita marcada como concluída!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Visitas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var visita = await _context.Visitas
                .Include(v => v.Anuncio)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (visita == null)
                return NotFound();

            // Carregar Comprador e Vendedor manualmente
            var utilizadorComprador = await _context.Set<Utilizador>()
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

            var utilizadorVendedor = await _context.Set<Utilizador>()
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

            // Apenas quem agendou (comprador da visita) pode deletar
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null || domainUser.Id != visita.CompradorId)
            {
                Console.WriteLine($"[ERRO Delete] Acesso negado. DomainUser.Id={domainUser?.Id}, CompradorId={visita.CompradorId}");
                return Forbid();
            }

            return View(visita);
        }

        // POST: Visitas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visita = await _context.Visitas.FindAsync(id);

            if (visita == null)
                return NotFound();

            // Apenas quem agendou (comprador da visita) pode deletar
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio (usa Set<Utilizador> para buscar independentemente do discriminador TPH)
            var domainUser = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (domainUser == null || domainUser.Id != visita.CompradorId)
            {
                Console.WriteLine($"[ERRO Delete POST] Acesso negado. DomainUser.Id={domainUser?.Id}, CompradorId={visita.CompradorId}");
                return Forbid();
            }

            _context.Visitas.Remove(visita);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visita removida com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        private bool VisitaExists(int id)
        {
            return _context.Visitas.Any(e => e.Id == id);
        }
    }
}

