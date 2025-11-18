#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# Script para limpar o Index.cshtml removendo código estático da seção de ValidarVendedores

file_path = r"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Administrador\Index.cshtml"

# Ler o arquivo
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Manter linhas 1-434 e depois pular para 715
new_lines = lines[:434]  # Linhas 1-434 (índice 0-433)
new_lines.extend(lines[714:])  # Linhas 715 em diante (índice 714+)

# Escrever de volta
with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Arquivo limpo com sucesso!")
print(f"Removidas {len(lines) - len(new_lines)} linhas de código estático.")
