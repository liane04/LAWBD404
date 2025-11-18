#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# Script para substituir conteúdo estático de Gerir Utilizadores pelo componente dinâmico

file_path = r"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Administrador\Index.cshtml"

# Ler o arquivo
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Manter linhas 1-436 (até a abertura de gerir-utilizadores-content)
new_lines = lines[:436]  # Linhas 1-436 (índice 0-435)

# Adicionar o componente
new_lines.append("                @await Component.InvokeAsync(\"GerirUtilizadores\")\n")
new_lines.append("            </div>\n")
new_lines.append("\n")

# Adicionar o resto do arquivo a partir da linha 796
new_lines.extend(lines[795:])  # Linhas 796 em diante (índice 795+)

# Escrever de volta
with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

removed = len(lines) - len(new_lines) + 3  # +3 porque adicionamos 3 linhas
print(f"Arquivo atualizado com sucesso!")
print(f"Removidas aproximadamente {removed} linhas de código estático.")
print(f"Componente GerirUtilizadores integrado.")
