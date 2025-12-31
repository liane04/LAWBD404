$file = 'C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Utilizadores\Registar.cshtml'

# Read as bytes and convert from Latin1 to UTF-8
$bytes = [System.IO.File]::ReadAllBytes($file)
$latin1 = [System.Text.Encoding]::GetEncoding('ISO-8859-1')
$content = $latin1.GetString($bytes)

# Now fix the garbled Portuguese characters
# These are the UTF-8 bytes interpreted as Latin1, so we need to fix them
$content = $content -replace 'Ã£', 'ã'  # ã
$content = $content -replace 'Ã§', 'ç'  # ç
$content = $content -replace 'Ã­', 'í'  # í
$content = $content -replace 'Ã¡', 'á'  # á
$content = $content -replace 'Ãº', 'ú'  # ú
$content = $content -replace 'Ãµ', 'õ'  # õ
$content = $content -replace 'Ã ', 'à'  # à
$content = $content -replace '&oacute;', 'ó'  # ó (HTML entity)

# Write back as UTF-8 with BOM
$utf8 = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($file, $content, $utf8)

Write-Host "Encoding conversion completed!"
