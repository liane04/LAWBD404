using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;
using System.Security.Claims;

namespace Marketplace.Controllers
{
    [Authorize]
    public class MensagensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Marketplace.Services.IEmailSender _emailSender;

        public MensagensController(ApplicationDbContext context, Marketplace.Services.IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            ViewBag.CurrentUserId = userId;
            return View();
        }

        // API: Obter conversas do utilizador
        [HttpGet]
        public async Task<IActionResult> GetConversas(string filtro = "todos")
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var query = _context.Conversas
                .Include(c => c.Anuncio)
                    .ThenInclude(a => a.Imagens)
                .Include(c => c.Vendedor)
                .Include(c => c.Comprador)
                .Include(c => c.Mensagens)
                .AsQueryable();

            // Filtrar apenas conversas onde o user participa
            query = query.Where(c => c.Vendedor.IdentityUserId == userId || c.Comprador.IdentityUserId == userId);

            // Filtros adicionais
            if (filtro == "comprar")
            {
                query = query.Where(c => c.Comprador.IdentityUserId == userId);
            }
            else if (filtro == "vender")
            {
                query = query.Where(c => c.Vendedor.IdentityUserId == userId);
            }

            var conversas = await query
                .OrderByDescending(c => c.UltimaMensagemData)
                .Select(c => new
                {
                    id = c.Id,
                    anuncioId = c.AnuncioId,
                    tituloAnuncio = c.Anuncio.Titulo,
                    imagemAnuncio = c.Anuncio.Imagens.Any() ? c.Anuncio.Imagens.First().ImagemCaminho : "/images/default-car.png",
                    outroParticipanteNome = c.Vendedor.IdentityUserId == userId ? c.Comprador.Nome : c.Vendedor.Nome,
                    outroParticipanteImagem = c.Vendedor.IdentityUserId == userId ? (c.Comprador.ImagemPerfil ?? "https://placehold.co/150") : (c.Vendedor.ImagemPerfil ?? "https://placehold.co/150"),
                    ultimaMensagem = c.Mensagens.OrderByDescending(m => m.DataEnvio).FirstOrDefault().Conteudo,
                    ultimaMensagemData = c.UltimaMensagemData,
                    naoLidas = c.Mensagens.Count(m => !m.Lida && m.RemetenteId != userId),
                    tipo = c.Vendedor.IdentityUserId == userId ? "vender" : "comprar"
                })
                .ToListAsync();

            return Json(conversas);
        }

        // API: Obter mensagens de uma conversa
        [HttpGet]
        public async Task<IActionResult> GetMensagens(int conversaId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var conversa = await _context.Conversas
                .Include(c => c.Anuncio)
                .Include(c => c.Vendedor)
                .Include(c => c.Comprador)
                .FirstOrDefaultAsync(c => c.Id == conversaId);

            if (conversa == null) return NotFound();

            // Verificar permissão
            if (conversa.Vendedor.IdentityUserId != userId && conversa.Comprador.IdentityUserId != userId)
            {
                return Forbid();
            }

            var mensagens = await _context.Mensagens
                .Where(m => m.ConversaId == conversaId)
                .OrderBy(m => m.DataEnvio)
                .Select(m => new
                {
                    id = m.Id,
                    conteudo = m.Conteudo,
                    dataEnvio = m.DataEnvio,
                    enviadaPorMim = m.RemetenteId == userId,
                    lida = m.Lida
                })
                .ToListAsync();

            // Marcar como lidas
            var mensagensNaoLidas = await _context.Mensagens
                .Where(m => m.ConversaId == conversaId && !m.Lida && m.RemetenteId != userId)
                .ToListAsync();

            if (mensagensNaoLidas.Any())
            {
                foreach (var msg in mensagensNaoLidas)
                {
                    msg.Lida = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new
            {
                conversaId = conversa.Id,
                anuncioId = conversa.AnuncioId,
                anuncioTitulo = conversa.Anuncio.Titulo,
                anuncioPreco = conversa.Anuncio.Preco,
                outroParticipanteNome = conversa.Vendedor.IdentityUserId == userId ? conversa.Comprador.Nome : conversa.Vendedor.Nome,
                outroParticipanteImagem = conversa.Vendedor.IdentityUserId == userId ? (conversa.Comprador.ImagemPerfil ?? "https://placehold.co/150") : (conversa.Vendedor.ImagemPerfil ?? "https://placehold.co/150"),
                mensagens = mensagens
            });
        }

        // API: Enviar mensagem
        [HttpPost]
        public async Task<IActionResult> EnviarMensagem([FromForm] int conversaId, [FromForm] string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo)) return BadRequest("Mensagem vazia");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var conversa = await _context.Conversas.FindAsync(conversaId);
            if (conversa == null) return NotFound();

            var mensagem = new Mensagens
            {
                ConversaId = conversaId,
                RemetenteId = userId,
                Conteudo = conteudo,
                DataEnvio = DateTime.Now,
                Lida = false,
                Estado = "enviado"
            };

            _context.Mensagens.Add(mensagem);
            
            // Atualizar data da última mensagem na conversa
            conversa.UltimaMensagemData = mensagem.DataEnvio;
            _context.Conversas.Update(conversa);

            await _context.SaveChangesAsync();

            // Enviar notificação por email (Fire and forget para não bloquear)
            _ = Task.Run(async () =>
            {
                try
                {
                    // Determinar destinatário
                    var destinatarioId = conversa.Vendedor.IdentityUserId == userId ? conversa.Comprador.IdentityUserId : conversa.Vendedor.IdentityUserId;
                    var destinatarioUser = await _context.Users.FindAsync(destinatarioId);

                    if (destinatarioUser != null && !string.IsNullOrEmpty(destinatarioUser.Email))
                    {
                        var remetenteNome = User.Identity?.Name ?? "Alguém";
                        var assunto = $"Nova mensagem de {remetenteNome} - 404 Car Marketplace";
                        var corpo = $@"
                            <h2>Nova Mensagem Recebida</h2>
                            <p>Recebeu uma nova mensagem sobre o anúncio <strong>{conversa.Anuncio?.Titulo ?? "Veículo"}</strong>.</p>
                            <p><strong>De:</strong> {remetenteNome}</p>
                            <p><strong>Mensagem:</strong></p>
                            <blockquote style='background: #f9f9f9; border-left: 10px solid #ccc; margin: 1.5em 10px; padding: 0.5em 10px;'>
                                {conteudo}
                            </blockquote>
                            <p><a href='http://localhost:5184/Mensagens?conversaId={conversaId}'>Clique aqui para responder</a></p>
                        ";

                        await _emailSender.SendAsync(destinatarioUser.Email, assunto, corpo);
                    }
                }
                catch (Exception ex)
                {
                    // Logar erro silenciosamente ou ignorar para não afetar o user
                    Console.WriteLine($"Erro ao enviar email de notificação: {ex.Message}");
                }
            });

            return Json(new 
            { 
                success = true, 
                mensagem = new 
                {
                    id = mensagem.Id,
                    conteudo = mensagem.Conteudo,
                    dataEnvio = mensagem.DataEnvio,
                    enviadaPorMim = true,
                    lida = mensagem.Lida
                } 
            });
        }

        // Action: Iniciar conversa a partir de um anúncio
        public async Task<IActionResult> IniciarConversa(int anuncioId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Verificar se o user é comprador (vendedores não iniciam conversas de compra consigo mesmos)
            // Mas vendedores podem querer contactar outros vendedores? Assumimos que sim.
            
            var anuncio = await _context.Anuncios
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == anuncioId);

            if (anuncio == null) return NotFound();

            // Não permitir iniciar conversa consigo mesmo
            if (anuncio.Vendedor.IdentityUserId == userId)
            {
                TempData["Error"] = "Não pode iniciar uma conversa sobre o seu próprio anúncio.";
                return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
            }

            // Verificar se já existe conversa
            var conversaExistente = await _context.Conversas
                .Include(c => c.Comprador)
                .FirstOrDefaultAsync(c => c.AnuncioId == anuncioId && c.Comprador.IdentityUserId == userId);

            if (conversaExistente != null)
            {
                // Redirecionar para a conversa existente (passando o ID para abrir automaticamente)
                return RedirectToAction("Index", new { conversaId = conversaExistente.Id });
            }

            // Criar nova conversa
            // Precisamos obter o CompradorId (do domínio) correspondente ao userId (Identity)
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == userId);
            
            if (comprador == null)
            {
                // Se for um vendedor a tentar comprar, talvez precisemos de lógica extra ou impedir.
                // Por agora, assumimos que apenas Compradores iniciam conversas.
                // Se o user for Vendedor, ele também pode ser Comprador? O sistema parece separar roles.
                // Se for Vendedor, não pode iniciar conversa como Comprador.
                
                if (User.IsInRole("Vendedor"))
                {
                     TempData["Error"] = "Apenas contas de Comprador podem iniciar conversas de compra.";
                     return RedirectToAction("Details", "Anuncios", new { id = anuncioId });
                }
                return NotFound("Perfil de comprador não encontrado.");
            }

            var novaConversa = new Conversa
            {
                AnuncioId = anuncioId,
                VendedorId = anuncio.VendedorId,
                CompradorId = comprador.Id,
                Tipo = "A comprar",
                UltimaMensagemData = DateTime.Now
            };

            _context.Conversas.Add(novaConversa);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { conversaId = novaConversa.Id });
        }
    }
}

