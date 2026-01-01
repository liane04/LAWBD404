using System.Text.Encodings.Web;

namespace Marketplace.Services
{
    public static class EmailTemplates
    {
        // 🎨 Cores
        private const string PrimaryColor = "#2563eb"; // Azul moderno (modern blue)
        private const string DarkColor = "#1e293b";   // Cinza azulado escuro (dark slate gray)
        private const string AppBrandName = "404 Ride"; // Nome da aplicação para o rodapé e logotipo

        /// <summary>
        /// Gera o template de email para confirmação de registo.
        /// </summary>
        /// <param name="appName">O nome da aplicação (usado no corpo do email).</param>
        /// <param name="confirmLink">O URL de confirmação.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string ConfirmEmail(string appName, string confirmLink)
        {
            // Segurança: Codificar variáveis dinâmicas para prevenir XSS.
            var safeLink = HtmlEncoder.Default.Encode(confirmLink);
            var safeApp = HtmlEncoder.Default.Encode(appName);

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Confirmar Email</h2>
                <p style='margin:0 0 16px;color:#334155'>Obrigado por se registar no {safeApp}.</p>
                <p style='margin:0 0 24px;color:#334155'>Clique no botão para confirmar o seu email.</p>
                <a href='{safeLink}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Confirmar Email</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Se o botão não funcionar, copie e cole este link no seu navegador:<br><span style='word-break:break-all'>{safeLink}</span></p>
            ");
        }

        /// <summary>
        /// Gera o template de email para redefinição de palavra-passe.
        /// </summary>
        /// <param name="appName">O nome da aplicação (usado no corpo do email).</param>
        /// <param name="resetLink">O URL para redefinição da palavra-passe.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string ResetPassword(string appName, string resetLink)
        {
            // Segurança: Codificar variáveis dinâmicas para prevenir XSS.
            var safeLink = HtmlEncoder.Default.Encode(resetLink);
            var safeApp = HtmlEncoder.Default.Encode(appName);

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Redefinir Palavra-passe</h2>
                <p style='margin:0 0 16px;color:#334155'>Recebemos um pedido para redefinir a sua palavra-passe no {safeApp}.</p>
                <p style='margin:0 0 24px;color:#334155'>Clique no botão para definir uma nova palavra-passe.</p>
                <a href='{safeLink}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Redefinir Palavra-passe</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Se não solicitou esta alteração, ignore este email.<br>Link direto: <span style='word-break:break-all'>{safeLink}</span></p>
            ");
        }

        /// <summary>
        /// Gera o template de email para notificação de novo anúncio.
        /// </summary>
        /// <param name="anuncioTitulo">Título do anúncio.</param>
        /// <param name="anuncioUrl">URL do anúncio.</param>
        /// <param name="preco">Preço do veículo.</param>
        /// <param name="imagem">URL da imagem do veículo.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string NewListingAlert(string anuncioTitulo, string anuncioUrl, decimal preco, string? imagem = null)
        {
            var safeTitle = HtmlEncoder.Default.Encode(anuncioTitulo);
            var safeUrl = HtmlEncoder.Default.Encode(anuncioUrl);
            var safePreco = preco.ToString("N2");
            var imageHtml = !string.IsNullOrEmpty(imagem)
                ? $"<img src='{HtmlEncoder.Default.Encode(imagem)}' style='width:100%;max-width:500px;border-radius:8px;margin-bottom:16px' alt='Veículo'>"
                : "";

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Novo Anúncio Disponível!</h2>
                {imageHtml}
                <p style='margin:0 0 16px;color:#334155'>Encontrámos um novo veículo que corresponde às suas preferências.</p>
                <div style='background:#f8fafc;padding:16px;border-radius:8px;margin:0 0 24px'>
                    <h3 style='margin:0 0 8px;color:{DarkColor};font-size:18px'>{safeTitle}</h3>
                    <p style='margin:0;color:{PrimaryColor};font-size:24px;font-weight:700'>{safePreco} €</p>
                </div>
                <a href='{safeUrl}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Ver Anúncio</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Pode gerir as suas preferências de notificação nas definições da conta.</p>
            ");
        }

        /// <summary>
        /// Gera o template de email para alerta de redução de preço.
        /// </summary>
        /// <param name="anuncioTitulo">Título do anúncio.</param>
        /// <param name="anuncioUrl">URL do anúncio.</param>
        /// <param name="precoAntigo">Preço anterior.</param>
        /// <param name="precoNovo">Preço novo.</param>
        /// <param name="imagem">URL da imagem do veículo.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string PriceDropAlert(string anuncioTitulo, string anuncioUrl, decimal precoAntigo, decimal precoNovo, string? imagem = null)
        {
            var safeTitle = HtmlEncoder.Default.Encode(anuncioTitulo);
            var safeUrl = HtmlEncoder.Default.Encode(anuncioUrl);
            var desconto = precoAntigo - precoNovo;
            var percentagem = (desconto / precoAntigo * 100).ToString("N0");
            var imageHtml = !string.IsNullOrEmpty(imagem)
                ? $"<img src='{HtmlEncoder.Default.Encode(imagem)}' style='width:100%;max-width:500px;border-radius:8px;margin-bottom:16px' alt='Veículo'>"
                : "";

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>🎉 Redução de Preço!</h2>
                {imageHtml}
                <p style='margin:0 0 16px;color:#334155'>Um veículo que marcou como favorito teve uma redução de preço.</p>
                <div style='background:#f8fafc;padding:16px;border-radius:8px;margin:0 0 24px'>
                    <h3 style='margin:0 0 12px;color:{DarkColor};font-size:18px'>{safeTitle}</h3>
                    <div style='display:flex;align-items:center;gap:12px;margin-bottom:8px'>
                        <span style='color:#94a3b8;text-decoration:line-through;font-size:18px'>{precoAntigo:N2} €</span>
                        <span style='background:#10b981;color:#fff;padding:4px 8px;border-radius:4px;font-size:12px;font-weight:600'>-{percentagem}%</span>
                    </div>
                    <p style='margin:0;color:{PrimaryColor};font-size:28px;font-weight:700'>{precoNovo:N2} €</p>
                    <p style='margin:8px 0 0;color:#22c55e;font-weight:600'>Poupa {desconto:N2} €!</p>
                </div>
                <a href='{safeUrl}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Ver Oferta</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Esta é uma oportunidade limitada. Não perca!</p>
            ");
        }

        /// <summary>
        /// Gera o template de email para newsletter.
        /// </summary>
        /// <param name="titulo">Título da newsletter.</param>
        /// <param name="conteudo">Conteúdo da newsletter (HTML).</param>
        /// <param name="ctaTexto">Texto do botão de ação.</param>
        /// <param name="ctaUrl">URL do botão de ação.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string Newsletter(string titulo, string conteudo, string ctaTexto, string ctaUrl)
        {
            var safeTitle = HtmlEncoder.Default.Encode(titulo);
            var safeCta = HtmlEncoder.Default.Encode(ctaTexto);
            var safeUrl = HtmlEncoder.Default.Encode(ctaUrl);

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>{safeTitle}</h2>
                <div style='color:#334155;line-height:1.6;margin-bottom:24px'>
                    {conteudo}
                </div>
                <a href='{safeUrl}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>{safeCta}</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Pode cancelar a subscrição da newsletter a qualquer momento nas definições da conta.</p>
            ");
        }

        /// <summary>
        /// Envolve o conteúdo HTML num wrapper de layout de email base (responsivo).
        /// </summary>
        /// <param name="innerHtml">O conteúdo central do email (já considerado seguro).</param>
        /// <returns>O HTML completo.</returns>
        private static string BaseWrapper(string innerHtml)
        {
            // O innerHtml é considerado seguro porque é definido pelo programador (string literal).
            return $@"<!doctype html>
<html lang='pt'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'>
<title>{AppBrandName}</title></head>
<body style='margin:0;background:#f1f5f9'>
  <table role='presentation' cellpadding='0' cellspacing='0' width='100%'>
    <tr><td style='padding:24px'>
      <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;box-shadow:0 2px 8px rgba(0,0,0,.04)'>
        <tr>
          <td style='padding:24px 24px 8px'>
            <div style='display:flex;align-items:center;gap:8px;color:{DarkColor};font-weight:800;font-size:18px'>
              <span style='display:inline-block;width:24px;height:24px;background:{PrimaryColor};border-radius:6px'></span>
              <span>{AppBrandName}</span>
            </div>
          </td>
        </tr>
        <tr>
          <td style='padding:8px 24px 24px'>
            {innerHtml}
          </td>
        </tr>
        <tr>
          <td style='padding:16px 24px;color:#94a3b8;font-size:12px'>
            &copy; {System.DateTime.Now:yyyy} {AppBrandName}. Todos os direitos reservados.
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body></html>";
        }
    }
}