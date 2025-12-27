using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;

namespace Marketplace.Controllers
{
    public class AnunciosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AnunciosController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Anuncios
        public async Task<IActionResult> Index(int? marcaId, int? modeloId, int? tipoId, int? categoriaId, string? categoria, int? combustivelId,
            decimal? precoMax, int? anoMin, int? anoMax, int? kmMax, string? caixa, string? localizacao,
            string? ordenacao)
        {
            // Carregar todos os anúncios com informações relacionadas
            var query = _context.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Tipo)
                .Include(a => a.Vendedor)
                .Include(a => a.Imagens)
                .AsQueryable();

            // Aplicar filtros
            if (marcaId.HasValue)
                query = query.Where(a => a.MarcaId == marcaId.Value);

            if (modeloId.HasValue)
                query = query.Where(a => a.ModeloId == modeloId.Value);

            if (tipoId.HasValue)
                query = query.Where(a => a.TipoId == tipoId.Value);

            // Filtering by Category ID (Dropdown)
            if (categoriaId.HasValue)
            {
                query = query.Where(a => a.CategoriaId == categoriaId.Value);
            }
            // Filtering by Category Name (Homepage Links)
            else if (!string.IsNullOrWhiteSpace(categoria))
            {
                // Find category ID from name to avoid Join issues if possible, or just Where on navigation property
                query = query.Where(a => a.Categoria.Nome == categoria);
                
                // Try to resolve the ID for the viewbag to select the dropdown correctly
                var catObj = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == categoria);
                if (catObj != null) categoriaId = catObj.Id;
            }

            if (combustivelId.HasValue)
                query = query.Where(a => a.CombustivelId == combustivelId.Value);

            if (precoMax.HasValue)
                query = query.Where(a => a.Preco <= precoMax.Value);

            if (anoMin.HasValue)
                query = query.Where(a => a.Ano >= anoMin.Value);

            if (anoMax.HasValue)
                query = query.Where(a => a.Ano <= anoMax.Value);

            if (kmMax.HasValue)
                query = query.Where(a => a.Quilometragem <= kmMax.Value);

            if (!string.IsNullOrWhiteSpace(caixa))
                query = query.Where(a => a.Caixa != null && a.Caixa.ToLower() == caixa.ToLower());

            if (!string.IsNullOrWhiteSpace(localizacao))
                query = query.Where(a => a.Localizacao != null && a.Localizacao.ToLower().Contains(localizacao.ToLower()));

            // Aplicar ordenação
            query = ordenacao switch
            {
                "preco-asc" => query.OrderBy(a => a.Preco),
                "preco-desc" => query.OrderByDescending(a => a.Preco),
                "ano-desc" => query.OrderByDescending(a => a.Ano),
                "km-asc" => query.OrderBy(a => a.Quilometragem),

                "relevancia" => query.OrderByDescending(a => a.NVisualizacoes).ThenByDescending(a => a.Id),

                _ => query.OrderByDescending(a => a.Id)
            };

            var anuncios = await query.ToListAsync();

            // Carregar dados para os filtros
            ViewBag.Marcas = await _context.Set<Marca>().OrderBy(m => m.Nome).ToListAsync();
            ViewBag.Modelos = await _context.Set<Modelo>().OrderBy(m => m.Nome).ToListAsync();
            ViewBag.Tipos = await _context.Set<Tipo>().OrderBy(t => t.Nome).ToListAsync();
            ViewBag.Categorias = await _context.Set<Categoria>().OrderBy(c => c.Nome).ToListAsync(); // Added Categorias
            ViewBag.Combustiveis = await _context.Set<Combustivel>().OrderBy(c => c.Tipo).ToListAsync();

            // Manter valores dos filtros aplicados
            ViewBag.MarcaIdSelecionada = marcaId;
            ViewBag.ModeloIdSelecionado = modeloId;
            ViewBag.TipoIdSelecionado = tipoId;
            ViewBag.CategoriaIdSelecionada = categoriaId; // Added CategoriaId
            ViewBag.CombustivelIdSelecionado = combustivelId;
            ViewBag.PrecoMax = precoMax;
            ViewBag.AnoMin = anoMin;
            ViewBag.AnoMax = anoMax;
            ViewBag.KmMax = kmMax;
            ViewBag.Caixa = caixa;
            ViewBag.Localizacao = localizacao;
            ViewBag.Ordenacao = ordenacao;

            // Carregar pesquisas guardadas do comprador autenticado
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Comprador"))
            {
                var identityIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(identityIdStr, out var identityId))
                {
                    var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == identityId);
                    if (comprador != null)
                    {
                        var filtros = await _context.FiltrosFavoritos
                            .Where(f => f.CompradorId == comprador.Id)
                            .OrderByDescending(f => f.CreatedAt)
                            .ToListAsync();
                        ViewBag.SavedFilters = filtros;
                    }
                }
            }

            return View(anuncios);
        }

        // POST: Anuncios/GuardarFiltro
        [Authorize(Roles = "Comprador")]
        [HttpPost]
        public async Task<IActionResult> GuardarFiltro(
            string? nome,
            int? marcaId, int? modeloId, int? tipoId, int? combustivelId,
            decimal? precoMax, int? anoMin, int? anoMax, int? kmMax, string? caixa, string? localizacao,
            string? ordenacao)
        {
            var identityIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(identityIdStr, out var identityId))
                return Forbid();

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == identityId);
            if (comprador == null)
                return Forbid();

            // Computar nome sugerido (opcionalmente inclui nomes de marca/modelo)
            var partes = new List<string>();
            if (marcaId.HasValue)
            {
                var marcaNome = await _context.Marcas
                    .Where(m => m.Id == marcaId.Value)
                    .Select(m => m.Nome)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(marcaNome)) partes.Add(marcaNome);
            }
            if (modeloId.HasValue)
            {
                var modeloNome = await _context.Modelos
                    .Where(m => m.Id == modeloId.Value)
                    .Select(m => m.Nome)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(modeloNome)) partes.Add(modeloNome);
            }
            if (precoMax.HasValue) partes.Add($"≤ {precoMax.Value:N0}€");
            if (anoMin.HasValue || anoMax.HasValue) partes.Add($"Ano {(anoMin?.ToString() ?? "?")}–{(anoMax?.ToString() ?? "?")}");
            var computedNome = string.IsNullOrWhiteSpace(nome) ? (partes.Count > 0 ? string.Join(" ", partes) : "Pesquisa") : nome!.Trim();

            var filtro = new FiltrosFav
            {
                CompradorId = comprador.Id,
                Nome = computedNome,
                MarcaId = marcaId,
                ModeloId = modeloId,
                TipoId = tipoId,
                CombustivelId = combustivelId,
                PrecoMax = precoMax,
                AnoMin = anoMin,
                AnoMax = anoMax,
                KmMax = kmMax,
                Caixa = string.IsNullOrWhiteSpace(caixa) ? null : caixa,
                Localizacao = string.IsNullOrWhiteSpace(localizacao) ? null : localizacao,
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.FiltrosFavoritos.Add(filtro);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pesquisa guardada. Receberá alertas de novidades.";
            return RedirectToAction(nameof(Index), new
            {
                marcaId, modeloId, tipoId, combustivelId,
                precoMax, anoMin, anoMax, kmMax, caixa, localizacao,
                ordenacao
            });
        }

        // POST: Anuncios/ToggleFiltro
        [Authorize(Roles = "Comprador")]
        [HttpPost]
        public async Task<IActionResult> ToggleFiltro(int id, bool ativo)
        {
            var identityIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(identityIdStr, out var identityId)) return Forbid();
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == identityId);
            if (comprador == null) return Forbid();

            var filtro = await _context.FiltrosFavoritos.FirstOrDefaultAsync(f => f.Id == id && f.CompradorId == comprador.Id);
            if (filtro == null) return NotFound();

            filtro.Ativo = ativo;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Comprador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(int id)
        {
            var identityIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(identityIdStr, out var identityId)) return Forbid();

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == identityId);
            if (comprador == null)
            {
                TempData["CompraErro"] = "É necessário perfil de comprador para concluir a compra.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var anuncio = await _context.Anuncios
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (anuncio == null)
            {
                TempData["CompraErro"] = "Anúncio não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (anuncio.VendedorId == comprador.Id)
            {
                TempData["CompraErro"] = "Não pode comprar o seu próprio anúncio.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var jaComprou = await _context.Compras.AnyAsync(c => c.AnuncioId == id && c.CompradorId == comprador.Id);
            if (jaComprou)
            {
                TempData["CompraErro"] = "Já registou uma compra para este anúncio.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Compras.Add(new Compra
            {
                AnuncioId = id,
                CompradorId = comprador.Id,
                Data = DateTime.UtcNow,
                EstadoPagamento = "Pendente"
            });
            await _context.SaveChangesAsync();

            TempData["CompraSucesso"] = "Compra registada! O vendedor será notificado para concluir o processo.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Anuncios/DeleteFiltro
        [Authorize(Roles = "Comprador")]
        [HttpPost]
        public async Task<IActionResult> DeleteFiltro(int id)
        {
            var identityIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(identityIdStr, out var identityId)) return Forbid();
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == identityId);
            if (comprador == null) return Forbid();

            var filtro = await _context.FiltrosFavoritos.FirstOrDefaultAsync(f => f.Id == id && f.CompradorId == comprador.Id);
            if (filtro == null) return NotFound();

            _context.FiltrosFavoritos.Remove(filtro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Anuncios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _context.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Tipo)
                .Include(a => a.Vendedor)
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anuncio == null)
            {
                return NotFound();
            }

            // Incrementar visualizações
            anuncio.NVisualizacoes++;
            await _context.SaveChangesAsync();

            return View(anuncio);
        }

        // GET: Anuncios/Create
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Create()
        {
            // Verificar se o vendedor está aprovado
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);

            if (vendedor == null || vendedor.Estado != "Ativo")
            {
                TempData["Error"] = "A sua conta de vendedor ainda não foi aprovada pelo administrador. Aguarde a aprovação para poder criar anúncios.";
                return RedirectToAction("Perfil", "Utilizadores");
            }

            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nome");
            ViewData["CombustivelId"] = new SelectList(_context.Set<Combustivel>(), "Id", "Tipo");
            ViewData["MarcaId"] = new SelectList(_context.Set<Marca>(), "Id", "Nome");
            ViewData["ModeloId"] = new SelectList(_context.Set<Modelo>(), "Id", "Nome");
            ViewData["TipoId"] = new SelectList(_context.Set<Tipo>(), "Id", "Nome");
            // CORREÇÃO: Alterado "Discriminator" para "Nome" para ser user-friendly.
            ViewData["VendedorId"] = new SelectList(_context.Set<Vendedor>(), "Id", "Nome");
            return View();
        }

        // POST: Anuncios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Create([Bind("Id,Preco,Ano,Cor,Descricao,Quilometragem,Titulo,Caixa,MarcaId,ModeloId,CategoriaId,CombustivelId,TipoId,Localizacao,Portas,Lugares,Potencia,Cilindrada")] Anuncio anuncio, List<IFormFile> imagens)
        {
            // Obter o vendedor autenticado via Identity
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);

            if (vendedor == null)
            {
                ModelState.AddModelError("", "Vendedor não encontrado. Certifique-se de que está autenticado corretamente.");
                ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nome", anuncio.CategoriaId);
                ViewData["CombustivelId"] = new SelectList(_context.Set<Combustivel>(), "Id", "Tipo", anuncio.CombustivelId);
                ViewData["MarcaId"] = new SelectList(_context.Set<Marca>(), "Id", "Nome", anuncio.MarcaId);
                ViewData["ModeloId"] = new SelectList(_context.Set<Modelo>(), "Id", "Nome", anuncio.ModeloId);
                ViewData["TipoId"] = new SelectList(_context.Set<Tipo>(), "Id", "Nome", anuncio.TipoId);
                return View(anuncio);
            }

            // Verificar se o vendedor está aprovado
            if (vendedor.Estado != "Ativo")
            {
                TempData["Error"] = "A sua conta de vendedor ainda não foi aprovada pelo administrador. Aguarde a aprovação para poder criar anúncios.";
                return RedirectToAction("Perfil", "Utilizadores");
            }

            // Associar o vendedor ao anúncio
            anuncio.VendedorId = vendedor.Id;
            anuncio.NVisualizacoes = 0;

            // Remover erros de validação dos campos que não vêm do formulário
            ModelState.Remove("VendedorId");
            ModelState.Remove("NVisualizacoes");
            ModelState.Remove("Vendedor");
            ModelState.Remove("Marca");
            ModelState.Remove("Modelo");
            ModelState.Remove("Categoria");
            ModelState.Remove("Combustivel");
            ModelState.Remove("Tipo");
            ModelState.Remove("Imagens");

            if (ModelState.IsValid)
            {
                // Adicionar o anúncio à base de dados
                _context.Add(anuncio);
                await _context.SaveChangesAsync();

                // Processar upload de imagens (se houver)
                if (imagens != null && imagens.Count > 0)
                {
                    await ProcessarImagensAsync(anuncio.Id, imagens);
                }

                TempData["Success"] = "Anúncio criado com sucesso!";
                return RedirectToAction("Details", new { id = anuncio.Id });
            }

            // Log erros de validação
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Console.WriteLine("Erros de validação: " + string.Join(", ", errors));
            System.IO.File.WriteAllText("validation_errors.txt", "Erros: " + string.Join(", ", errors));

            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nome", anuncio.CategoriaId);
            ViewData["CombustivelId"] = new SelectList(_context.Set<Combustivel>(), "Id", "Tipo", anuncio.CombustivelId);
            ViewData["MarcaId"] = new SelectList(_context.Set<Marca>(), "Id", "Nome", anuncio.MarcaId);
            ViewData["ModeloId"] = new SelectList(_context.Set<Modelo>(), "Id", "Nome", anuncio.ModeloId);
            ViewData["TipoId"] = new SelectList(_context.Set<Tipo>(), "Id", "Nome", anuncio.TipoId);
            return View(anuncio);
        }

        // GET: Anuncios/Edit/5 (Mock view, sem BD durante a fase de UI)
        [Authorize(Roles = "Vendedor")]
        public IActionResult Edit(int? id)
        {
            // Durante a fase de mockups não consultamos a BD.
            // A view de edição é estática e não depende de ViewData/Model.
            return View();
        }

        // POST: Anuncios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Preco,Ano,Cor,Descricao,Quilometragem,Titulo,Caixa,VendedorId,MarcaId,ModeloId,CategoriaId,CombustivelId,TipoId")] Anuncio anuncio)
        {
            if (id != anuncio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anuncio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnuncioExists(anuncio.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Set<Categoria>(), "Id", "Nome", anuncio.CategoriaId);
            ViewData["CombustivelId"] = new SelectList(_context.Set<Combustivel>(), "Id", "Tipo", anuncio.CombustivelId);
            ViewData["MarcaId"] = new SelectList(_context.Set<Marca>(), "Id", "Nome", anuncio.MarcaId);
            ViewData["ModeloId"] = new SelectList(_context.Set<Modelo>(), "Id", "Nome", anuncio.ModeloId);
            ViewData["TipoId"] = new SelectList(_context.Set<Tipo>(), "Id", "Nome", anuncio.TipoId);
            // CORREÇÃO: Alterado "Discriminator" para "Nome"
            ViewData["VendedorId"] = new SelectList(_context.Set<Vendedor>(), "Id", "Nome", anuncio.VendedorId);
            return View(anuncio);
        }

        // GET: Anuncios/Delete/5
        [Authorize(Roles = "Vendedor,Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _context.Anuncios
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Tipo)
                .Include(a => a.Vendedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (anuncio == null)
            {
                return NotFound();
            }

            return View(anuncio);
        }

        // POST: Anuncios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anuncio = await _context.Anuncios.FindAsync(id);
            if (anuncio != null)
            {
                _context.Anuncios.Remove(anuncio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Anuncios/Compare
        public IActionResult Compare()
        {
            // For mockup purposes, we'll use a static view
            // In production, this would fetch data based on IDs from query string or localStorage
            return View();
        }

        private async Task ProcessarImagensAsync(int anuncioId, List<IFormFile> imagens)
        {
            // Validações
            if (imagens == null || imagens.Count == 0) return;

            // Máximo de 20 imagens
            if (imagens.Count > 20)
            {
                throw new InvalidOperationException("Máximo de 20 imagens permitidas por anúncio.");
            }

            // Criar diretório se não existir
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "anuncios", anuncioId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            foreach (var imagem in imagens)
            {
                // Validar formato
                string[] extensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
                string extensao = Path.GetExtension(imagem.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    continue; // Ignorar ficheiros não suportados
                }

                // Validar tamanho (máximo 10MB)
                if (imagem.Length > 10 * 1024 * 1024)
                {
                    continue; // Ignorar ficheiros muito grandes
                }

                // Gerar nome único
                string nomeUnico = $"{Guid.NewGuid()}{extensao}";
                string caminhoCompleto = Path.Combine(uploadsFolder, nomeUnico);

                // Guardar ficheiro
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await imagem.CopyToAsync(stream);
                }

                // Criar registo na base de dados
                var imagemDb = new Imagem
                {
                    AnuncioId = anuncioId,
                    ImagemCaminho = $"/images/anuncios/{anuncioId}/{nomeUnico}"
                };

                _context.Imagens.Add(imagemDb);
            }

            await _context.SaveChangesAsync();
        }

        // API endpoint para obter o TipoId de um Modelo
        [HttpGet]
        public async Task<IActionResult> GetModeloTipo(int modeloId)
        {
            var modelo = await _context.Set<Modelo>().FindAsync(modeloId);
            if (modelo == null)
            {
                return NotFound();
            }
            return Json(new { tipoId = modelo.TipoId });
        }

        // API endpoint para obter modelos filtrados por marca
        [HttpGet]
        public async Task<IActionResult> GetModelosByMarca(int marcaId)
        {
            var modelos = await _context.Set<Modelo>()
                .Where(m => m.MarcaId == marcaId)
                .OrderBy(m => m.Nome)
                .Select(m => new { id = m.Id, nome = m.Nome })
                .ToListAsync();

            return Json(modelos);
        }

        private bool AnuncioExists(int id)
        {
            return _context.Anuncios.Any(e => e.Id == id);
        }
    }
}

