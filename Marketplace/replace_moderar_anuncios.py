#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# Script para substituir conteúdo estático de Moderar Anúncios pelo componente dinâmico

file_path = r"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Administrador\Index.cshtml"

# Ler o arquivo
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Manter linhas 1-441 (até a abertura de moderar-anuncios-content)
new_lines = lines[:441]  # Linhas 1-441 (índice 0-440)

# Adicionar o componente
new_lines.append("                @await Component.InvokeAsync(\"ModerarAnuncios\")\n")
new_lines.append("            </div>\n")
new_lines.append("        </div>\n")
new_lines.append("    </div>\n")
new_lines.append("</div>\n")
new_lines.append("\n")

# Adicionar o resto do arquivo a partir da linha 1766
new_lines.extend(lines[1765:])  # Linhas 1766 em diante (índice 1765+)

# Escrever de volta
with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

removed = len(lines) - len(new_lines) + 6  # +6 porque adicionamos 6 linhas
print(f"Arquivo atualizado com sucesso!")
print(f"Removidas aproximadamente {removed} linhas de código estático.")
print(f"Componente ModerarAnuncios integrado.")
