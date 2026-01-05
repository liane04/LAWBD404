using System.Text.Encodings.Web;

namespace Marketplace.Services
{
    public static class EmailTemplates
    {
        // ðŸŽ¨ Cores
        private const string PrimaryColor = "#2563eb"; // Azul moderno (modern blue)
        private const string DarkColor = "#1e293b";   // Cinza azulado escuro (dark slate gray)
        private const string AppBrandName = "404 Ride"; // Nome da aplicação para o rodapé e logotipo
        private const string LogoUrl = "https://404ride.b-host.me/imagens/logo.png"; // Logotipo principal (versão azul)

        /// <summary>
        /// Gera o template de email para confirmaÃ§Ã£o de registo.
        /// </summary>
        /// <param name="appName">O nome da aplicaÃ§Ã£o (usado no corpo do email).</param>
        /// <param name="confirmLink">O URL de confirmaÃ§Ã£o.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string ConfirmEmail(string appName, string confirmLink)
        {
            // SeguranÃ§a: Codificar variÃ¡veis dinÃ¢micas para prevenir XSS.
            var safeLink = HtmlEncoder.Default.Encode(confirmLink);
            var safeApp = HtmlEncoder.Default.Encode(appName);

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Confirmar Email</h2>
                <p style='margin:0 0 16px;color:#334155'>Obrigado por se registar no {safeApp}.</p>
                <p style='margin:0 0 24px;color:#334155'>Clique no botÃ£o para confirmar o seu email.</p>
                <a href='{safeLink}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Confirmar Email</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Se o botÃ£o nÃ£o funcionar, copie e cole este link no seu navegador:<br><span style='word-break:break-all'>{safeLink}</span></p>
            ");
        }

        /// <summary>
        /// Gera o template de email para redefiniÃ§Ã£o de palavra-passe.
        /// </summary>
        /// <param name="appName">O nome da aplicaÃ§Ã£o (usado no corpo do email).</param>
        /// <param name="resetLink">O URL para redefiniÃ§Ã£o da palavra-passe.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string ResetPassword(string appName, string resetLink)
        {
            // SeguranÃ§a: Codificar variÃ¡veis dinÃ¢micas para prevenir XSS.
            var safeLink = HtmlEncoder.Default.Encode(resetLink);
            var safeApp = HtmlEncoder.Default.Encode(appName);

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Redefinir Palavra-passe</h2>
                <p style='margin:0 0 16px;color:#334155'>Recebemos um pedido para redefinir a sua palavra-passe no {safeApp}.</p>
                <p style='margin:0 0 24px;color:#334155'>Clique no botÃ£o para definir uma nova palavra-passe.</p>
                <a href='{safeLink}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Redefinir Palavra-passe</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Se nÃ£o solicitou esta alteraÃ§Ã£o, ignore este email.<br>Link direto: <span style='word-break:break-all'>{safeLink}</span></p>
            ");
        }

        /// <summary>
        /// Gera o template de email para notificaÃ§Ã£o de novo anÃºncio.
        /// </summary>
        /// <param name="anuncioTitulo">TÃ­tulo do anÃºncio.</param>
        /// <param name="anuncioUrl">URL do anÃºncio.</param>
        /// <param name="preco">PreÃ§o do veÃ­culo.</param>
        /// <param name="imagem">URL da imagem do veÃ­culo.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string NewListingAlert(string anuncioTitulo, string anuncioUrl, decimal preco, string? imagem = null)
        {
            var safeTitle = HtmlEncoder.Default.Encode(anuncioTitulo);
            var safeUrl = HtmlEncoder.Default.Encode(anuncioUrl);
            var safePreco = preco.ToString("N2");
            var imageHtml = !string.IsNullOrEmpty(imagem)
                ? $"<img src='{HtmlEncoder.Default.Encode(imagem)}' style='width:100%;max-width:500px;border-radius:8px;margin-bottom:16px' alt='VeÃ­culo'>"
                : "";

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>Novo AnÃºncio DisponÃ­vel!</h2>
                {imageHtml}
                <p style='margin:0 0 16px;color:#334155'>EncontrÃ¡mos um novo veÃ­culo que corresponde Ã s suas preferÃªncias.</p>
                <div style='background:#f8fafc;padding:16px;border-radius:8px;margin:0 0 24px'>
                    <h3 style='margin:0 0 8px;color:{DarkColor};font-size:18px'>{safeTitle}</h3>
                    <p style='margin:0;color:{PrimaryColor};font-size:24px;font-weight:700'>{safePreco} â‚¬</p>
                </div>
                <a href='{safeUrl}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Ver AnÃºncio</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Pode gerir as suas preferÃªncias de notificaÃ§Ã£o nas definiÃ§Ãµes da conta.</p>
            ");
        }

        /// <summary>
        /// Gera o template de email para alerta de reduÃ§Ã£o de preÃ§o.
        /// </summary>
        /// <param name="anuncioTitulo">TÃ­tulo do anÃºncio.</param>
        /// <param name="anuncioUrl">URL do anÃºncio.</param>
        /// <param name="precoAntigo">PreÃ§o anterior.</param>
        /// <param name="precoNovo">PreÃ§o novo.</param>
        /// <param name="imagem">URL da imagem do veÃ­culo.</param>
        /// <returns>A string HTML completa do email.</returns>
        public static string PriceDropAlert(string anuncioTitulo, string anuncioUrl, decimal precoAntigo, decimal precoNovo, string? imagem = null)
        {
            var safeTitle = HtmlEncoder.Default.Encode(anuncioTitulo);
            var safeUrl = HtmlEncoder.Default.Encode(anuncioUrl);
            var desconto = precoAntigo - precoNovo;
            var percentagem = (desconto / precoAntigo * 100).ToString("N0");
            var imageHtml = !string.IsNullOrEmpty(imagem)
                ? $"<img src='{HtmlEncoder.Default.Encode(imagem)}' style='width:100%;max-width:500px;border-radius:8px;margin-bottom:16px' alt='VeÃ­culo'>"
                : "";

            return BaseWrapper($@"
                <h2 style='margin:0 0 16px;color:{DarkColor};font-weight:700'>ðŸŽ‰ ReduÃ§Ã£o de PreÃ§o!</h2>
                {imageHtml}
                <p style='margin:0 0 16px;color:#334155'>Um veÃ­culo que marcou como favorito teve uma reduÃ§Ã£o de preÃ§o.</p>
                <div style='background:#f8fafc;padding:16px;border-radius:8px;margin:0 0 24px'>
                    <h3 style='margin:0 0 12px;color:{DarkColor};font-size:18px'>{safeTitle}</h3>
                    <div style='display:flex;align-items:center;gap:12px;margin-bottom:8px'>
                        <span style='color:#94a3b8;text-decoration:line-through;font-size:18px'>{precoAntigo:N2} â‚¬</span>
                        <span style='background:#10b981;color:#fff;padding:4px 8px;border-radius:4px;font-size:12px;font-weight:600'>-{percentagem}%</span>
                    </div>
                    <p style='margin:0;color:{PrimaryColor};font-size:28px;font-weight:700'>{precoNovo:N2} â‚¬</p>
                    <p style='margin:8px 0 0;color:#22c55e;font-weight:600'>Poupa {desconto:N2} â‚¬!</p>
                </div>
                <a href='{safeUrl}' style='background:{PrimaryColor};color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;display:inline-block;font-weight:600'>Ver Oferta</a>
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Esta Ã© uma oportunidade limitada. NÃ£o perca!</p>
            ");
        }

        /// <summary>
        /// Gera o template de email para newsletter.
        /// </summary>
        /// <param name="titulo">TÃ­tulo da newsletter.</param>
        /// <param name="conteudo">ConteÃºdo da newsletter (HTML).</param>
        /// <param name="ctaTexto">Texto do botÃ£o de aÃ§Ã£o.</param>
        /// <param name="ctaUrl">URL do botÃ£o de aÃ§Ã£o.</param>
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
                <p style='margin:16px 0 0;color:#64748b;font-size:12px'>Pode cancelar a subscriÃ§Ã£o da newsletter a qualquer momento nas definiÃ§Ãµes da conta.</p>
            ");
        }

        /// <summary>
        /// Envolve o conteÃºdo HTML num wrapper de layout de email base (responsivo).
        /// </summary>
        /// <param name="innerHtml">O conteÃºdo central do email (jÃ¡ considerado seguro).</param>
        /// <returns>O HTML completo.</returns>
        private static string BaseWrapper(string innerHtml)
        {
            // O innerHtml Ã© considerado seguro porque Ã© definido pelo programador (string literal).
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
              <img src='{LogoUrl}' alt='{AppBrandName} logotipo' style='height:32px;width:auto;display:block' />
              <span style='line-height:1'>{AppBrandName}</span>
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

