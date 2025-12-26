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

            // Buscar utilizador de domínio (Comprador, Vendedor ou Administrador)
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);

            if (domainUser == null)
                return NotFound();

            IQueryable<Visita> visitasQuery = _context.Visitas
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Marca)
                .Include(v => v.Anuncio)
                    .ThenInclude(a => a.Modelo)
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor);

            if (domainUser is Comprador)
            {
                // Compradores veem suas próprias visitas agendadas
                visitasQuery = visitasQuery.Where(v => v.CompradorId == domainUser.Id);
                ViewBag.UserRole = "Comprador";
            }
            else if (domainUser is Vendedor)
            {
                // Vendedores veem visitas aos seus anúncios
                visitasQuery = visitasQuery.Where(v => v.VendedorId == domainUser.Id);
                ViewBag.UserRole = "Vendedor";
            }
            else
            {
                return Forbid();
            }

            var visitas = await visitasQuery
                .OrderByDescending(v => v.Data)
                .ToListAsync();

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
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor)
                .Include(v => v.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (visita == null)
                return NotFound();

            // Verificar se o utilizador tem permissão para ver esta visita
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (domainUser == null || (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId))
                return Forbid();

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
                return Forbid();

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
                return Forbid();

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

            if (ModelState.IsValid)
            {
                visita.CompradorId = compradorId.Value;
                visita.VendedorId = anuncio.VendedorId;
                visita.Estado = "Pendente";
                visita.DataCriacao = DateTime.Now;

                _context.Add(visita);
                await _context.SaveChangesAsync();

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
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            // Verificar permissões
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (domainUser == null || (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId))
                return Forbid();

            ViewBag.IsVendedor = domainUser.Id == visita.VendedorId;
            ViewBag.MinDate = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");

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

            // Buscar utilizador de domínio
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            // Verificar permissões
            if (domainUser == null || (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId))
                return Forbid();

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

                    // Enviar notificação de alteração
                    var anuncio = await _context.Anuncios.FindAsync(visita.AnuncioId);
                    var comprador = await _context.Compradores.FindAsync(visita.CompradorId);
                    var vendedor = await _context.Vendedores.FindAsync(visita.VendedorId);

                    string destinatario = domainUser.Id == visita.VendedorId ? comprador.Email : vendedor.Email;
                    string nomeDestinatario = domainUser.Id == visita.VendedorId ? comprador.Nome : vendedor.Nome;

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
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null || vendedor.Id != visita.VendedorId)
                return Forbid();

            visita.Estado = "Confirmada";
            visita.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            // Notificar comprador
            try
            {
                await _emailSender.SendAsync(
                    visita.Comprador.Email,
                    "Visita Confirmada - 404 Ride",
                    $@"<h2>Visita Confirmada</h2>
                    <p>Olá {visita.Comprador.Nome},</p>
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
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Utilizadores");

            // Buscar utilizador de domínio
            Utilizador? domainUser = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
            if (domainUser == null)
                domainUser = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (domainUser == null || (domainUser.Id != visita.CompradorId && domainUser.Id != visita.VendedorId))
                return Forbid();

            visita.Estado = "Cancelada";
            visita.Observacoes = $"Cancelada: {motivo ?? "Sem motivo especificado"}";
            visita.DataAtualizacao = DateTime.Now;
            await _context.SaveChangesAsync();

            // Notificar a outra parte
            string destinatario = domainUser.Id == visita.VendedorId ? visita.Comprador.Email : visita.Vendedor.Email;
            string nomeDestinatario = domainUser.Id == visita.VendedorId ? visita.Comprador.Nome : visita.Vendedor.Nome;

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
                .Include(v => v.Comprador)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (visita == null)
                return NotFound();

            // Apenas comprador pode deletar
            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null || comprador.Id != visita.CompradorId)
                return Forbid();

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

            // Apenas comprador pode deletar
            var user = await _userManager.GetUserAsync(User);
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null || comprador.Id != visita.CompradorId)
                return Forbid();

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

