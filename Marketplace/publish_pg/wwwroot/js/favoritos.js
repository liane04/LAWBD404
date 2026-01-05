// favoritos.js - Sistema dinâmico de favoritos
// Carregado em todas as páginas que precisam de funcionalidade de favoritos

(function () {
    'use strict';

    // Verificar se o utilizador está autenticado como Comprador
    let userRole = null;
    let favoritosIds = new Set();

    // Inicialização
    document.addEventListener('DOMContentLoaded', function () {
        // Detectar role do utilizador (pode ser passado via data attribute no body)
        const bodyElement = document.querySelector('body');
        if (bodyElement) {
            userRole = bodyElement.getAttribute('data-user-role');
        }

        // Carregar favoritos do utilizador se estiver autenticado
        if (userRole) {
            carregarFavoritos();
        }

        // Inicializar todos os botões de favorito
        inicializarBotoesFavorito();
    });

    // Carrega todos os IDs de favoritos do utilizador
    async function carregarFavoritos() {
        try {
            const response = await fetch('/Favoritos/GetAll', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.favoritos) {
                    favoritosIds = new Set(data.favoritos);
                    atualizarEstadoBotoes();
                }
            }
        } catch (error) {
            console.error('Erro ao carregar favoritos:', error);
        }
    }

    // Inicializa event listeners para todos os botões de favorito
    function inicializarBotoesFavorito() {
        // Botões principais (cards de listagem e detalhes)
        document.querySelectorAll('.btn-favorito, .btn-favorito-small, .btn-outline-danger[onclick*="toggleFavorito"]').forEach(btn => {
            // Remove onclick inline se existir
            btn.removeAttribute('onclick');

            // Adiciona event listener
            btn.addEventListener('click', async function (e) {
                // Verifica autenticação
                /* Role Check Removed - Available to all authenticated users
                if (userRole !== 'Comprador') {
                     // ...
                }
                */
                // Se não estiver logado (userRole null ou undefined assumindo que backend injecta corretamente), o backend rejeita.
                // Mas idealmente deveríamos verificar se está logado genérico.
                // Simplificação: Se userRole existir (qualquer que seja), tenta. Se falhar, o backend trata.
                if (!userRole) {
                    mostrarToast('Precisa de estar autenticado para adicionar favoritos', 'warning');
                    setTimeout(() => {
                        window.location.href = '/Utilizadores/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    }, 1500);
                    return;
                }

                // Obter ID do anúncio
                const anuncioId = parseInt(this.getAttribute('data-anuncio-id'));
                if (!anuncioId) {
                    console.error('ID do anúncio não encontrado no botão');
                    return;
                }

                // Toggle favorito
                await toggleFavorito(anuncioId, this);
            });
        });
    }

    // Toggle favorito via AJAX
    async function toggleFavorito(anuncioId, button) {
        try {
            // Desabilitar botão durante request
            button.disabled = true;

            const response = await fetch('/Favoritos/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify(anuncioId)
            });

            const data = await response.json();

            if (data.success) {
                // Atualizar conjunto local
                if (data.adicionado) {
                    favoritosIds.add(anuncioId);
                } else {
                    favoritosIds.delete(anuncioId);
                }

                // Atualizar UI do botão
                atualizarBotao(button, data.adicionado);

                // Mostrar mensagem
                mostrarToast(data.message, 'success');

                // Atualizar contador na navegação se existir
                atualizarContadorNav(data.totalFavoritos);

                // Se estamos na página de favoritos e removemos, remover o card
                if (!data.adicionado && window.location.pathname.includes('/Favoritos')) {
                    const card = button.closest('.col-md-6, .col-lg-4, .col');
                    if (card) {
                        card.remove();

                        // Verificar se não há mais favoritos
                        const remainingCards = document.querySelectorAll('.col-md-6, .col-lg-4, .col').length;
                        if (remainingCards === 0) {
                            location.reload(); // Mostrar empty state
                        }
                    }
                }
            } else {
                mostrarToast(data.message || 'Erro ao atualizar favorito', 'error');
            }
        } catch (error) {
            console.error('Erro ao toggle favorito:', error);
            mostrarToast('Erro ao comunicar com o servidor', 'error');
        } finally {
            button.disabled = false;
        }
    }

    // Atualiza a aparência de um botão de favorito
    function atualizarBotao(button, isFavorito) {
        const icon = button.querySelector('i');
        if (!icon) return;

        if (isFavorito) {
            // Adicionar aos favoritos
            icon.classList.remove('bi-heart');
            icon.classList.add('bi-heart-fill');
            button.classList.add('active');

            // Se o botão tiver classes específicas
            if (button.classList.contains('btn-outline-danger')) {
                button.classList.remove('btn-outline-danger');
                button.classList.add('btn-danger');
            }
        } else {
            // Remover dos favoritos
            icon.classList.remove('bi-heart-fill');
            icon.classList.add('bi-heart');
            button.classList.remove('active');

            // Se o botão tiver classes específicas
            if (button.classList.contains('btn-danger')) {
                button.classList.remove('btn-danger');
                button.classList.add('btn-outline-danger');
            }
        }
    }

    // Atualiza o estado de todos os botões baseado nos favoritos carregados
    function atualizarEstadoBotoes() {
        document.querySelectorAll('[data-anuncio-id]').forEach(btn => {
            const anuncioId = parseInt(btn.getAttribute('data-anuncio-id'));
            if (favoritosIds.has(anuncioId)) {
                atualizarBotao(btn, true);
            }
        });
    }

    // Atualiza o contador de favoritos na navegação
    function atualizarContadorNav(total) {
        const contadorElement = document.querySelector('#favoritos-count');
        if (contadorElement) {
            contadorElement.textContent = total;

            // Mostrar/esconder badge se necessário
            if (total > 0) {
                contadorElement.classList.remove('d-none');
            } else {
                contadorElement.classList.add('d-none');
            }
        }
    }

    // Mostra toast de notificação
    function mostrarToast(message, type = 'info') {
        // Verificar se já existe container de toasts
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        // Criar toast
        const toast = document.createElement('div');
        const bgClass = type === 'success' ? 'bg-success' : type === 'warning' ? 'bg-warning' : 'bg-danger';
        const iconClass = type === 'success' ? 'bi-check-circle' : type === 'warning' ? 'bi-exclamation-triangle' : 'bi-exclamation-circle';

        toast.className = `toast align-items-center text-white ${bgClass} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${iconClass} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        toastContainer.appendChild(toast);

        // Inicializar e mostrar toast
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();

        // Remover toast do DOM após esconder
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    // Expor funções globalmente se necessário
    window.FavoritosApp = {
        carregarFavoritos: carregarFavoritos,
        toggleFavorito: toggleFavorito,
        atualizarEstadoBotoes: atualizarEstadoBotoes
    };
})();
