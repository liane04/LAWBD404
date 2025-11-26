using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;

namespace Marketplace.Tests.Manual
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true,
                AllowAutoRedirect = false // We want to check redirects manually
            };

            using var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5184")
            };

            Console.WriteLine("1. Navigating to Login...");
            var loginPage = await client.GetAsync("/Utilizadores/Login");
            var loginHtml = await loginPage.Content.ReadAsStringAsync();
            var loginToken = GetRequestVerificationToken(loginHtml);

            Console.WriteLine("2. Logging in...");
            var loginContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email", "vendedor@email.com"),
                new KeyValuePair<string, string>("Password", "Vende123"),
                new KeyValuePair<string, string>("__RequestVerificationToken", loginToken)
            });

            var loginResult = await client.PostAsync("/Utilizadores/Login", loginContent);
            if (loginResult.StatusCode == HttpStatusCode.Found)
            {
                Console.WriteLine("Login successful (Redirected).");
            }
            else
            {
                Console.WriteLine($"Login failed. Status: {loginResult.StatusCode}");
                var html = await loginResult.Content.ReadAsStringAsync();
                Console.WriteLine("Response HTML:");
                Console.WriteLine(html);
                return;
            }

            Console.WriteLine("3. Navigating to Create Ad...");
            var createPage = await client.GetAsync("/Anuncios/Create");
            if (createPage.StatusCode != HttpStatusCode.OK)
            {
                 Console.WriteLine($"Failed to access Create page. Status: {createPage.StatusCode}");
                 if (createPage.StatusCode == HttpStatusCode.Found)
                    Console.WriteLine($"Redirected to: {createPage.Headers.Location}");
                 return;
            }
            var createHtml = await createPage.Content.ReadAsStringAsync();
            var createToken = GetRequestVerificationToken(createHtml);

            Console.WriteLine("4. Submitting Ad...");
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(createToken), "__RequestVerificationToken");
            form.Add(new StringContent("BMW 320d Automated Test"), "Titulo");
            form.Add(new StringContent("25000"), "Preco");
            form.Add(new StringContent("500"), "ValorSinal");
            form.Add(new StringContent("Teste automatico via C#"), "Descricao");
            form.Add(new StringContent("1"), "CategoriaId");
            form.Add(new StringContent("3"), "MarcaId");
            form.Add(new StringContent("20"), "ModeloId");
            form.Add(new StringContent("2020"), "Ano");
            form.Add(new StringContent("50000"), "Quilometragem");
            form.Add(new StringContent("1"), "CombustivelId");
            form.Add(new StringContent("Automática"), "Caixa");
            form.Add(new StringContent("Lisboa"), "Localizacao");

            var createResult = await client.PostAsync("/Anuncios/Create", form);

            if (createResult.StatusCode == HttpStatusCode.Found)
            {
                Console.WriteLine($"Ad creation successful! Redirected to: {createResult.Headers.Location}");
            }
            else
            {
                Console.WriteLine($"Ad creation failed. Status: {createResult.StatusCode}");
                var responseHtml = await createResult.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(responseHtml);
                
                // Try to find validation summary
                var summaryErrors = doc.DocumentNode.SelectNodes("//div[contains(@class, 'validation-summary-errors')]//li");
                if (summaryErrors != null)
                {
                    Console.WriteLine("Summary Errors:");
                    foreach (var error in summaryErrors)
                        Console.WriteLine($"- {error.InnerText.Trim()}");
                }

                // Try to find field errors
                var fieldErrors = doc.DocumentNode.SelectNodes("//span[contains(@class, 'field-validation-error')]");
                if (fieldErrors != null)
                {
                    Console.WriteLine("Field Errors:");
                    foreach (var error in fieldErrors)
                        Console.WriteLine($"- {error.InnerText.Trim()}");
                }
            }
        }

        static string GetRequestVerificationToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");
            return node?.GetAttributeValue("value", "") ?? "";
        }
    }
}
