#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# Script para substituir anúncios estáticos por listagem dinâmica

file_path = r"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\Views\Anuncios\Index.cshtml"
novo_codigo_path = r"C:\Users\bruno\Desktop\utad\a_1_semestre_3_ano\Laboratotio_web_bd\app\Marketplace\anuncios_dinamicos.txt"

# Ler o arquivo original
with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Ler o novo código dinâmico
with open(novo_codigo_path, 'r', encoding='utf-8') as f:
    novo_codigo = f.read()

# Manter linhas 1-228 (até antes de "<!-- Lista de Anúncios -->")
new_lines = lines[:228]  # Linhas 1-228 (índice 0-227)

# Adicionar o novo código dinâmico
new_lines.append(novo_codigo)

# Adicionar o resto do arquivo a partir da linha 559 (depois da paginação estática)
# Vamos manter a paginação por enquanto
new_lines.extend(lines[558:])  # Linhas 559 em diante (índice 558+)

# Escrever de volta
with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

removed = len(lines) - len(new_lines) + len(novo_codigo.split('\n'))
print(f"Arquivo atualizado com sucesso!")
print(f"Substituídos ~330 linhas de anúncios estáticos por código dinâmico.")
print(f"Listagem de anúncios agora é totalmente dinâmica!")
