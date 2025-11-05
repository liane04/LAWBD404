using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Marketplace.Services
{
    public static class ImageUploadHelper
    {
        // Extensões permitidas para imagens de perfil
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        // Tamanho máximo: 5 MB para imagens de perfil
        private const long MaxFileSize = 5 * 1024 * 1024;

        /// <summary>
        /// Faz upload de uma imagem de perfil e retorna o caminho relativo
        /// </summary>
        /// <param name="file">Ficheiro de imagem enviado</param>
        /// <param name="webRootPath">Caminho raiz da aplicação (wwwroot)</param>
        /// <param name="userId">ID do utilizador (para nome único do ficheiro)</param>
        /// <returns>Caminho relativo da imagem (ex: /images/perfil/user_123.jpg) ou null se houver erro</returns>
        public static async Task<string?> UploadProfileImage(IFormFile file, string webRootPath, int userId)
        {
            // Validações
            if (file == null || file.Length == 0)
                return null;

            // Verificar extensão
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return null;

            // Verificar tamanho
            if (file.Length > MaxFileSize)
                return null;

            try
            {
                // Pasta de destino
                var uploadsFolder = Path.Combine(webRootPath, "images", "perfil");

                // Criar pasta se não existir
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Nome único do ficheiro: user_{userId}_{timestamp}{extension}
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var uniqueFileName = $"user_{userId}_{timestamp}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Guardar ficheiro
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar caminho relativo (para guardar na BD)
                return $"/images/perfil/{uniqueFileName}";
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Remove uma imagem de perfil antiga
        /// </summary>
        /// <param name="imagePath">Caminho relativo da imagem (ex: /images/perfil/user_123.jpg)</param>
        /// <param name="webRootPath">Caminho raiz da aplicação (wwwroot)</param>
        public static void DeleteProfileImage(string? imagePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                // Converter caminho relativo em absoluto
                var fullPath = Path.Combine(webRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                // Verificar se ficheiro existe e apagar
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // Ignorar erros ao apagar (ficheiro pode já não existir)
            }
        }

        /// <summary>
        /// Retorna o caminho da imagem de perfil padrão
        /// </summary>
        public static string GetDefaultProfileImage()
        {
            return "/images/default-avatar.png";
        }

        /// <summary>
        /// Valida se um ficheiro é uma imagem válida para perfil
        /// </summary>
        public static bool IsValidProfileImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Nenhum ficheiro foi selecionado.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = $"Formato não permitido. Use apenas: {string.Join(", ", AllowedExtensions)}";
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                errorMessage = $"O ficheiro é muito grande. Tamanho máximo: {MaxFileSize / (1024 * 1024)} MB";
                return false;
            }

            return true;
        }
    }
}
