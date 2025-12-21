using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Marketplace.Services
{
    // Verifica periodicamente filtros guardados ativos e cria notificações para novos anúncios correspondentes
    public class SavedFiltersNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Intervalo de execução (pode ser afinado no futuro ou movido para config)
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

        public SavedFiltersNotificationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // pequeno atraso inicial para evitar competir com migrações/seed
            try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); } catch { }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Buscar filtros ativos
                    var filtros = await db.FiltrosFavoritos
                        .AsNoTracking()
                        .Where(f => f.Ativo)
                        .ToListAsync(stoppingToken);

                    foreach (var f in filtros)
                    {
                        // Construir query aplicando os critérios guardados
                        var query = db.Anuncios
                            .Include(a => a.Marca)
                            .Include(a => a.Modelo)
                            .Include(a => a.Combustivel)
                            .AsQueryable();

                        if (f.MarcaId.HasValue) query = query.Where(a => a.MarcaId == f.MarcaId);
                        if (f.ModeloId.HasValue) query = query.Where(a => a.ModeloId == f.ModeloId);
                        if (f.TipoId.HasValue) query = query.Where(a => a.TipoId == f.TipoId);
                        if (f.CombustivelId.HasValue) query = query.Where(a => a.CombustivelId == f.CombustivelId);
                        if (f.PrecoMax.HasValue) query = query.Where(a => a.Preco <= f.PrecoMax);
                        if (f.AnoMin.HasValue) query = query.Where(a => a.Ano >= f.AnoMin);
                        if (f.AnoMax.HasValue) query = query.Where(a => a.Ano <= f.AnoMax);
                        if (f.KmMax.HasValue) query = query.Where(a => a.Quilometragem <= f.KmMax);
                        if (!string.IsNullOrWhiteSpace(f.Caixa)) query = query.Where(a => a.Caixa != null && a.Caixa.ToLower() == f.Caixa.ToLower());
                        if (!string.IsNullOrWhiteSpace(f.Localizacao)) query = query.Where(a => a.Localizacao != null && a.Localizacao.ToLower().Contains(f.Localizacao.ToLower()));

                        // Heurística: considerar apenas IDs acima do último notificado
                        if (f.MaxAnuncioIdNotificado > 0)
                        {
                            query = query.Where(a => a.Id > f.MaxAnuncioIdNotificado);
                        }

                        var novos = await query
                            .OrderBy(a => a.Id)
                            .Take(50) // evitar spam num único ciclo
                            .ToListAsync(stoppingToken);

                        if (novos.Count > 0)
                        {
                            // Criar uma notificação agregada (mensagem concisa)
                            var topTitulos = string.Join(
                                ", ",
                                novos.Take(3).Select(a => a.Titulo.Length > 30 ? a.Titulo.Substring(0, 30) + "…" : a.Titulo)
                            );

                            var conteudo = novos.Count == 1
                                ? $"Novo anúncio que corresponde ao seu filtro '{(f.Nome ?? "Pesquisa")}' — {topTitulos}"
                                : $"{novos.Count} novos anúncios para o filtro '{(f.Nome ?? "Pesquisa")}' — {topTitulos}";

                            var notif = new Notificacoes
                            {
                                Conteudo = conteudo,
                                Data = DateTime.UtcNow,
                                CompradorId = f.CompradorId,
                                FiltrosFavId = f.Id
                            };

                            // Atualizar estado do filtro e persistir notificação
                            // Necessário novo scope do filtro com tracking
                            var trackedFiltro = await db.FiltrosFavoritos.FirstAsync(x => x.Id == f.Id, stoppingToken);
                            trackedFiltro.LastCheckedAt = DateTime.UtcNow;
                            trackedFiltro.MaxAnuncioIdNotificado = Math.Max(trackedFiltro.MaxAnuncioIdNotificado, novos.Max(a => a.Id));

                            db.Notificacoes.Add(notif);

                            await db.SaveChangesAsync(stoppingToken);
                        }
                        else
                        {
                            // Apenas atualizar LastChecked se não houver novidades (opcional)
                            var trackedFiltro = await db.FiltrosFavoritos.FirstAsync(x => x.Id == f.Id, stoppingToken);
                            trackedFiltro.LastCheckedAt = DateTime.UtcNow;
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch
                {
                    // swallow e voltar a tentar no próximo ciclo; logs podem ser adicionados futuramente
                }

                try { await Task.Delay(Interval, stoppingToken); } catch { }
            }
        }
    }
}

