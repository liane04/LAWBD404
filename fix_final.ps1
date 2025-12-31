$file = 'C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Utilizadores\Registar.cshtml'

# Read the file as UTF-8
$content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

# Replace the incorrectly encoded Portuguese characters with correct ones
$replacements = @{
    'padrÃ£o' = 'padrão'
    'InformaÃ§Ã£o' = 'Informação'
    'veÃ­culos' = 've��culos'
    'rÃ¡pido' = 'rápido'
    'anÃºncios' = 'anúncios'
    'NotificaÃ§Ãµes' = 'Notificações'
    'FormulÃ¡rio' = 'Formulário'
    'N&oacute;s' = 'Nós'
    'Junte-se Ã ' = 'Junte-se à'
    'comeÃ§ar' = 'começar'
    'pÃ¡gina' = 'página'
}

foreach ($old in $replacements.Keys) {
    $new = $replacements[$old]
    $content = $content.Replace($old, $new)
}

# Write back as UTF-8 with BOM
$utf8BOM = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($file, $content, $utf8BOM)

Write-Host "Fixed encoding issues!"
