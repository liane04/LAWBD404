import re

# Ler o ficheiro
with open('Views/Utilizadores/Perfil.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Corrigir a linha 324 corrompida
# Remover os caracteres estranhos 'n' e '\!--'
content = re.sub(
    r'@await Html\.PartialAsync\("_MeusAnunciosList", ViewBag\.MeusAnuncios as IEnumerable<Marketplace\.Models\.Anuncio>\)n\s+</div>nn\s+<\\!-- Estado Vazio',
    r'@await Html.PartialAsync("_MeusAnunciosList", ViewBag.MeusAnuncios as IEnumerable<Marketplace.Models.Anuncio>)\n\n                        <!-- Estado Vazio',
    content
)

# Escrever o ficheiro corrigido
with open('Views/Utilizadores/Perfil.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)

print("Ficheiro corrigido com sucesso!")
