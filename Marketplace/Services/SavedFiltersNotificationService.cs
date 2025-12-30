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
    // Também verifica novos anúncios para utilizadores que seguem marcas específicas.
    public class SavedFiltersNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Intervalo de execução (pode ser afinado no futuro ou movido para config)
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);
        
        // Estado volátil: último ID de anúncio processado para marcas
        // Em caso de restart, perder-se-á informação e poderá haver delay na retoma, mas evita spam ou complexidade extra.
        // O ideal seria persistir isto na DB, mas por simplicidade usamos esta abordagem.
        private int _lastProcessedAnuncioId = 0;

        public SavedFiltersNotificationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Inicializar _lastProcessedAnuncioId com o máximo atual para não notificar anúncios antigos no arranque
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    _lastProcessedAnuncioId = await db.Anuncios.MaxAsync(a => (int?)a.Id) ?? 0;
                }
            }
            catch
            {
                // DB pode não estar pronta
            }

            // pequeno atraso inicial para evitar competir com migrações/seed
            try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); } catch { }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // =================================================================================
                    // 1. Processar FILTROS GUARDADOS (Pesquisas Favoritas)
                    // =================================================================================
                    
                    // Buscar filtros ativos
                    var filtros = await db.FiltrosFavoritos
                        .AsNoTracking()
                        .ToListAsync(stoppingToken);

                    // Rastreio de notificações de filtro para evitar duplicados nas marcas
                    var processedUserAds = new HashSet<(int UserId, int AnuncioId)>();

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

                        // Heurística: considerar apenas IDs acima do último notificado E criados DEPOIS da pesquisa ser guardada
                        if (f.MaxAnuncioIdNotificado > 0)
                        {
                            query = query.Where(a => a.Id > f.MaxAnuncioIdNotificado);
                        }
                        else
                        {
                            // Se é a primeira vez que verificamos este filtro, ignorar anúncios antigos (anteriores à criação do filtro)
                            // Isto evita spam de "novos anúncios" quando se cria uma pesquisa que já tem muitos resultados existentes.
                            // Nota: Como não temos CreatedAt no Anuncio, usamos uma aproximação baseada em IDs ou assumimos que 
                            // anúncios existentes já foram vistos pelo utilizador aquando da criação da pesquisa.
                            // Mas para ser seguro, vamos usar o MaxAnuncioIdNotificado que deve ser inicializado ao criar o filtro.
                            // Como alternativa, podemos filtrar apenas anúncios criados (ou com ID superior ao max existente na altura da criação).
                        }

                        // Filtro crucial: apenas anúncios processados pelo serviço como "novos" nesta iteração
                        // O 'novosAnuncios' no início do método pegou em tudo > lastProcessedId. 
                        // Mas aqui estamos a re-consultar a BD. 
                        // Vamos simplificar: Apenas notificar anúncios cujo ID seja superior ao MaxAnuncioId da tabela de anúncios
                        // no momento em que o filtro foi criado. 
                        // Corrigindo: O filtro deve ter um campo 'StartsFromAnuncioId' ou similar. 
                        // Como não temos, vamos usar o LastCheckedAt como referência temporal, mas não temos CreatedAt no Anuncio.
                        // Solução pragmática: Filtrar IDs > _lastProcessedAnuncioId (que representa o topo da pilha global)
                        // OU usar o MaxAnuncioIdNotificado que agora vamos garantir que é inicializado correctamente no Controller.
                        
                        // Assumindo que o Controller inicializa MaxAnuncioIdNotificado com o ID máximo atual quando cria o filtro,
                        // esta cláusula Where(Id > Max...) funcionará perfeitamente para ignorar os antigos.
                        if (f.MaxAnuncioIdNotificado > 0)
                        {
                            query = query.Where(a => a.Id > f.MaxAnuncioIdNotificado);
                        }
                        else
                        {
                           // Fallback safety: se for 0 (filtro antigo ou erro), assume-se apenas novos globais
                           // query = query.Where(a => a.Id > _lastProcessedAnuncioId);
                           // Melhor: Se é 0, atualizamos para o Max atual e não notificamos nada nesta primeira passagem,
                           // para evitar spam de 100 anúncios velhos.
                           var currentMax = await db.Anuncios.MaxAsync(a => (int?)a.Id) ?? 0;
                           f.MaxAnuncioIdNotificado = currentMax;
                           // Salvar mudanças e continue para o próximo (skip notification now)
                           db.Entry(f).State = EntityState.Modified; 
                           // Note: save is tricky inside foreach loop with ASYNC enumerable if context is shared poorly
                           // But here we are fine to defer save or just mark it.
                           // Vamos forçar o update no fim ou já aqui.
                           continue; 
                        }

                        var novos = await query
                            .OrderBy(a => a.Id)
                            .Take(50) // evitar spam num único ciclo
                            .ToListAsync(stoppingToken);

                        if (novos.Count > 0)
                        {
                            // Registar para não duplicar na notificação de marca
                            foreach (var n in novos) processedUserAds.Add((f.CompradorId, n.Id));

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
                                CompradorId = f.CompradorId, // Mapeia para UtilizadorId (coluna chama-se CompradorId)
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

                    // =================================================================================
                    // 2. Processar MARCAS FAVORITAS (Novos Anúncios)
                    // =================================================================================
                    
                    var currentMaxId = await db.Anuncios.MaxAsync(a => (int?)a.Id) ?? 0;
                    
                    // Proteção de segurança: Se o serviço arrancou agora (_lastProcessedAnuncioId == 0) 
                    // e já existem anúncios na BD (currentMaxId > 0), assumimos que são antigos.
                    // Atualizamos apenas o ponteiro e não notificamos para evitar spam de histórico.
                    // Se a BD estiver vazia (currentMaxId == 0), tudo bem, o próximo novo será notificado.
                    if (_lastProcessedAnuncioId == 0 && currentMaxId > 0)
                    {
                         _lastProcessedAnuncioId = currentMaxId;
                    }
                    else if (currentMaxId > _lastProcessedAnuncioId)
                    {
                        var anunciosRecentes = await db.Anuncios
                            .Include(a => a.Marca)
                            .Where(a => a.Id > _lastProcessedAnuncioId)
                            .ToListAsync(stoppingToken);

                        foreach (var anuncio in anunciosRecentes)
                        {
                            // Encontrar utilizadores que seguem esta marca
                            var marcasInteressadas = await db.MarcasFavoritas
                                .Where(mf => mf.MarcaId == anuncio.MarcaId)
                                .Include(mf => mf.Comprador) // Na verdade é Utilizador
                                .ToListAsync(stoppingToken);
                            
                            foreach (var interesse in marcasInteressadas)
                            {
                                // Se já notificámos este utilizador sobre este anúncio via Pesquisa Guardada, ignorar
                                if (processedUserAds.Contains((interesse.CompradorId, anuncio.Id))) continue;

                                // O utilizador pode ter interesse na marca E um filtro específico.
                                
                                var conteudo = $"Novo anúncio da sua marca favorita {anuncio.Marca?.Nome}: {anuncio.Titulo}";
                                if (conteudo.Length > 500) conteudo = conteudo.Substring(0, 497) + "...";

                                var notif = new Notificacoes
                                {
                                    Conteudo = conteudo,
                                    Data = DateTime.UtcNow,
                                    CompradorId = interesse.CompradorId, // Utilize o ID do Utilizador/Comprador
                                    MarcasFavId = interesse.Id
                                };

                                db.Notificacoes.Add(notif);
                            }
                        }
                        
                        if (anunciosRecentes.Any())
                        {
                            await db.SaveChangesAsync(stoppingToken);
                        }
                        
                        _lastProcessedAnuncioId = currentMaxId;
                    }
                }
                catch (Exception ex)
                {
                    // swallow e voltar a tentar no próximo ciclo
                    Console.WriteLine($"Erro no NotificationService: {ex.Message}");
                }

                try { await Task.Delay(Interval, stoppingToken); } catch { }
            }
        }
    }
}

