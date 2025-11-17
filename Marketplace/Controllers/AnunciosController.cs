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
using Microsoft.AspNetCore.Identity;

namespace Marketplace.Controllers
{
    public class AnunciosController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnunciosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<bool> IsVendedorAtivoAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return false;
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
            if (vendedor == null) return true; // Sem registo de Vendedor no domínio, não bloquear aqui
            return string.Equals(vendedor.Estado, "Ativo", StringComparison.OrdinalIgnoreCase);
        }

        // GET: Anuncios
        /// <summary>
        /// Lista todos os anúncios com suporte para filtros e pesquisa.
        /// Filtra por: marca, modelo, preço, ano, quilometragem, categoria, combustível, caixa, localização.
        /// </summary>
        public async Task<IActionResult> Index(
            string marca,
            string modelo,
            decimal? precoMax,
            int? anoMin,
            int? anoMax,
            int? kmMin,
            int? kmMax,
            string categoria,
            string combustivel,
            string caixa,
            string localizacao,
            string ordenar = "relevancia")
        {
            // Query base com todas as relações necessárias
            var query = _db.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Tipo)
                .Include(a => a.Vendedor)
                .Include(a => a.Imagens)
                .AsQueryable();

            // Aplicar filtros (se fornecidos)
            if (!string.IsNullOrWhiteSpace(marca))
            {
                query = query.Where(a => a.Marca.Nome.Contains(marca));
            }

            if (!string.IsNullOrWhiteSpace(modelo))
            {
                query = query.Where(a => a.Modelo.Nome.Contains(modelo));
            }

            if (precoMax.HasValue && precoMax > 0)
            {
                query = query.Where(a => a.Preco <= precoMax.Value);
            }

            if (anoMin.HasValue)
            {
                query = query.Where(a => a.Ano >= anoMin.Value);
            }

            if (anoMax.HasValue)
            {
                query = query.Where(a => a.Ano <= anoMax.Value);
            }

            if (kmMin.HasValue)
            {
                query = query.Where(a => a.Quilometragem >= kmMin.Value);
            }

            if (kmMax.HasValue)
            {
                query = query.Where(a => a.Quilometragem <= kmMax.Value);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(a => a.Categoria.Nome.Contains(categoria));
            }

            if (!string.IsNullOrWhiteSpace(combustivel))
            {
                query = query.Where(a => a.Combustivel.Tipo.Contains(combustivel));
            }

            if (!string.IsNullOrWhiteSpace(caixa))
            {
                query = query.Where(a => a.Caixa != null && a.Caixa.Contains(caixa));
            }

            if (!string.IsNullOrWhiteSpace(localizacao))
            {
                query = query.Where(a => a.Localizacao != null && a.Localizacao.Contains(localizacao));
            }

            // Aplicar ordenação
            query = ordenar switch
            {
                "preco-asc" => query.OrderBy(a => a.Preco),
                "preco-desc" => query.OrderByDescending(a => a.Preco),
                "ano-desc" => query.OrderByDescending(a => a.Ano),
                "km-asc" => query.OrderBy(a => a.Quilometragem),
                _ => query.OrderByDescending(a => a.Id) // Relevância = mais recentes primeiro
            };

            // Executar query e retornar resultados
            var anuncios = await query.ToListAsync();

            // Passar dados para a view via ViewBag para popular os filtros
            ViewBag.Marcas = await _db.Marcas.OrderBy(m => m.Nome).ToListAsync();
            ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nome).ToListAsync();
            ViewBag.Combustiveis = await _db.Combustiveis.OrderBy(c => c.Tipo).ToListAsync();

            return View(anuncios);
        }

        // GET: Anuncios/Details/5
        /// <summary>
        /// Exibe os detalhes completos de um anúncio específico.
        /// Inclui: imagens, especificações técnicas, extras, informação do vendedor.
        /// Incrementa o contador de visualizações.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Carregar anúncio com todas as relações necessárias
            var anuncio = await _db.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Tipo)
                .Include(a => a.Vendedor)
                .Include(a => a.Imagens)
                .Include(a => a.AnuncioExtras)
                    .ThenInclude(ae => ae.Extra)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anuncio == null)
            {
                return NotFound();
            }

            // Incrementar visualizações (apenas se o utilizador não for o vendedor)
            var utilizadorAtual = await _userManager.GetUserAsync(User);
            if (utilizadorAtual == null || anuncio.VendedorId != anuncio.Vendedor?.Id)
            {
                anuncio.NVisualizacoes = anuncio.NVisualizacoes + 1;
                _db.Update(anuncio);
                await _db.SaveChangesAsync();
            }

            // Carregar anúncios semelhantes (mesma marca ou categoria, excluindo o atual)
            var anunciosSemelhantes = await _db.Anuncios
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .Where(a => a.Id != id &&
                       (a.MarcaId == anuncio.MarcaId || a.CategoriaId == anuncio.CategoriaId))
                .OrderBy(a => Guid.NewGuid()) // Ordem aleatória
                .Take(4)
                .ToListAsync();

            ViewBag.AnunciosSemelhantes = anunciosSemelhantes;

            return View(anuncio);
        }

        // GET: Anuncios/Create
        /// <summary>
        /// Exibe o formulário de criação de um novo anúncio.
        /// Apenas vendedores ativos podem aceder.
        /// </summary>
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Create()
        {
            // Bloquear vendedores pendentes/rejeitados
            var ativo = await IsVendedorAtivoAsync();
            if (!ativo)
            {
                TempData["AnuncioErro"] = "A sua conta de vendedor ainda não foi aprovada. Aguarde validação do administrador.";
                return RedirectToAction("Perfil", "Utilizadores");
            }

            // Popular dropdowns com dados da BD
            ViewData["CategoriaId"] = new SelectList(await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome");
            ViewData["CombustivelId"] = new SelectList(await _db.Combustiveis.OrderBy(c => c.Tipo).ToListAsync(), "Id", "Tipo");
            ViewData["MarcaId"] = new SelectList(await _db.Marcas.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome");
            ViewData["ModeloId"] = new SelectList(await _db.Modelos.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome");
            ViewData["TipoId"] = new SelectList(await _db.Tipos.OrderBy(t => t.Nome).ToListAsync(), "Id", "Nome");
            ViewData["VendedorId"] = new SelectList(await _db.Vendedores.OrderBy(v => v.Nome).ToListAsync(), "Id", "Nome");

            return View();
        }

        // POST: Anuncios/Create
        /// <summary>
        /// Processa a criação de um novo anúncio.
        /// Valida o vendedor e guarda o anúncio na BD.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Create([Bind("Id,Preco,Ano,Cor,Descricao,Quilometragem,Titulo,Caixa,Localizacao,Portas,Lugares,Potencia,Cilindrada,ValorSinal,VendedorId,MarcaId,ModeloId,CategoriaId,CombustivelId,TipoId")] Anuncio anuncio)
        {
            if (!await IsVendedorAtivoAsync())
            {
                TempData["AnuncioErro"] = "A sua conta de vendedor ainda não foi aprovada. Aguarde validação do administrador.";
                return RedirectToAction("Perfil", "Utilizadores");
            }

            if (ModelState.IsValid)
            {
                // Inicializar campos automáticos
                anuncio.NVisualizacoes = 0;

                _db.Add(anuncio);
                await _db.SaveChangesAsync();

                TempData["AnuncioSucesso"] = "Anúncio criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            // Recarregar dropdowns em caso de erro
            ViewData["CategoriaId"] = new SelectList(await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", anuncio.CategoriaId);
            ViewData["CombustivelId"] = new SelectList(await _db.Combustiveis.OrderBy(c => c.Tipo).ToListAsync(), "Id", "Tipo", anuncio.CombustivelId);
            ViewData["MarcaId"] = new SelectList(await _db.Marcas.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.MarcaId);
            ViewData["ModeloId"] = new SelectList(await _db.Modelos.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.ModeloId);
            ViewData["TipoId"] = new SelectList(await _db.Tipos.OrderBy(t => t.Nome).ToListAsync(), "Id", "Nome", anuncio.TipoId);
            ViewData["VendedorId"] = new SelectList(await _db.Vendedores.OrderBy(v => v.Nome).ToListAsync(), "Id", "Nome", anuncio.VendedorId);

            return View(anuncio);
        }

        // GET: Anuncios/Edit/5
        /// <summary>
        /// Exibe o formulário de edição de um anúncio existente.
        /// Apenas o vendedor proprietário ou administrador podem editar.
        /// </summary>
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador é o proprietário do anúncio
            var utilizadorAtual = await _userManager.GetUserAsync(User);
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == utilizadorAtual.Id);

            if (vendedor != null && anuncio.VendedorId != vendedor.Id && !User.IsInRole("Administrador"))
            {
                TempData["AnuncioErro"] = "Não tem permissão para editar este anúncio.";
                return RedirectToAction(nameof(Index));
            }

            // Popular dropdowns
            ViewData["CategoriaId"] = new SelectList(await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", anuncio.CategoriaId);
            ViewData["CombustivelId"] = new SelectList(await _db.Combustiveis.OrderBy(c => c.Tipo).ToListAsync(), "Id", "Tipo", anuncio.CombustivelId);
            ViewData["MarcaId"] = new SelectList(await _db.Marcas.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.MarcaId);
            ViewData["ModeloId"] = new SelectList(await _db.Modelos.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.ModeloId);
            ViewData["TipoId"] = new SelectList(await _db.Tipos.OrderBy(t => t.Nome).ToListAsync(), "Id", "Nome", anuncio.TipoId);
            ViewData["VendedorId"] = new SelectList(await _db.Vendedores.OrderBy(v => v.Nome).ToListAsync(), "Id", "Nome", anuncio.VendedorId);

            return View(anuncio);
        }

        // POST: Anuncios/Edit/5
        /// <summary>
        /// Processa a edição de um anúncio existente.
        /// Valida permissões e atualiza os dados na BD.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Preco,Ano,Cor,Descricao,Quilometragem,Titulo,Caixa,Localizacao,Portas,Lugares,Potencia,Cilindrada,ValorSinal,NVisualizacoes,DataExpiracao,VendedorId,MarcaId,ModeloId,CategoriaId,CombustivelId,TipoId")] Anuncio anuncio)
        {
            if (id != anuncio.Id)
            {
                return NotFound();
            }

            // Verificar permissões
            var utilizadorAtual = await _userManager.GetUserAsync(User);
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == utilizadorAtual.Id);

            if (vendedor != null && anuncio.VendedorId != vendedor.Id && !User.IsInRole("Administrador"))
            {
                TempData["AnuncioErro"] = "Não tem permissão para editar este anúncio.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(anuncio);
                    await _db.SaveChangesAsync();

                    TempData["AnuncioSucesso"] = "Anúncio atualizado com sucesso!";
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

            // Recarregar dropdowns em caso de erro
            ViewData["CategoriaId"] = new SelectList(await _db.Categorias.OrderBy(c => c.Nome).ToListAsync(), "Id", "Nome", anuncio.CategoriaId);
            ViewData["CombustivelId"] = new SelectList(await _db.Combustiveis.OrderBy(c => c.Tipo).ToListAsync(), "Id", "Tipo", anuncio.CombustivelId);
            ViewData["MarcaId"] = new SelectList(await _db.Marcas.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.MarcaId);
            ViewData["ModeloId"] = new SelectList(await _db.Modelos.OrderBy(m => m.Nome).ToListAsync(), "Id", "Nome", anuncio.ModeloId);
            ViewData["TipoId"] = new SelectList(await _db.Tipos.OrderBy(t => t.Nome).ToListAsync(), "Id", "Nome", anuncio.TipoId);
            ViewData["VendedorId"] = new SelectList(await _db.Vendedores.OrderBy(v => v.Nome).ToListAsync(), "Id", "Nome", anuncio.VendedorId);

            return View(anuncio);
        }

        // GET: Anuncios/Delete/5
        /// <summary>
        /// Exibe a confirmação de eliminação de um anúncio.
        /// Apenas o vendedor proprietário ou administrador podem eliminar.
        /// </summary>
        [Authorize(Roles = "Vendedor,Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anuncio = await _db.Anuncios
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

            // Verificar permissões
            var utilizadorAtual = await _userManager.GetUserAsync(User);
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == utilizadorAtual.Id);

            if (vendedor != null && anuncio.VendedorId != vendedor.Id && !User.IsInRole("Administrador"))
            {
                TempData["AnuncioErro"] = "Não tem permissão para eliminar este anúncio.";
                return RedirectToAction(nameof(Index));
            }

            return View(anuncio);
        }

        // POST: Anuncios/Delete/5
        /// <summary>
        /// Processa a eliminação definitiva de um anúncio.
        /// Remove o anúncio e todas as suas relações da BD.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Vendedor,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio != null)
            {
                // Verificar permissões
                var utilizadorAtual = await _userManager.GetUserAsync(User);
                var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == utilizadorAtual.Id);

                if (vendedor != null && anuncio.VendedorId != vendedor.Id && !User.IsInRole("Administrador"))
                {
                    TempData["AnuncioErro"] = "Não tem permissão para eliminar este anúncio.";
                    return RedirectToAction(nameof(Index));
                }

                _db.Anuncios.Remove(anuncio);
                await _db.SaveChangesAsync();

                TempData["AnuncioSucesso"] = "Anúncio eliminado com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Anuncios/Compare
        public IActionResult Compare()
        {
            // For mockup purposes, we'll use a static view
            // In production, this would fetch data based on IDs from query string or localStorage
            return View();
        }

        /// <summary>
        /// Verifica se um anúncio existe na BD.
        /// </summary>
        private bool AnuncioExists(int id)
        {
            return _db.Anuncios.Any(e => e.Id == id);
        }
    }
}

