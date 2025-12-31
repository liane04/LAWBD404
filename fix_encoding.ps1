$file = 'C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Utilizadores\Registar.cshtml'
$content = Get-Content $file -Raw -Encoding UTF8

# Fix all encoding issues
$content = $content -replace 'padrÃ£o', 'padrão'
$content = $content -replace 'InformaÃ§Ã£o', 'Informação'
$content = $content -replace 'veÃ­culos', 'veículos'
$content = $content -replace 'rÃ¡pido', 'rápido'
$content = $content -replace 'anÃºncios', 'anúncios'
$content = $content -replace 'NotificaÃ§Ãµes', 'Notificações'
$content = $content -replace 'FormulÃ¡rio', 'Formulário'
$content = $content -replace 'Junte-se Ã ', 'Junte-se à'
$content = $content -replace 'comeÃ§ar', 'começar'
$content = $content -replace 'Voltar Ã  pÃ¡gina', 'Voltar à página'

# Write back with UTF-8 BOM
$utf8 = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($file, $content, $utf8)

Write-Host "Encoding fixed successfully!"
