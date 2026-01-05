-- Script de correção para PostgreSQL
-- Fix: Alterar DataDestaque e DestaqueAte para timestamp with time zone
-- Este script preserva os dados existentes

BEGIN;

-- 1. Verificar tipos atuais
SELECT
    column_name,
    data_type,
    udt_name
FROM information_schema.columns
WHERE table_name = 'Anuncios'
  AND column_name IN ('DataDestaque', 'DestaqueAte');

-- 2. Alterar colunas para timestamp with time zone (se ainda não estiverem)
-- Nota: Se já estiverem como timestamptz, estas queries não farão nada

DO $$
BEGIN
    -- Alterar DataDestaque se necessário
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'Anuncios'
        AND column_name = 'DataDestaque'
        AND data_type != 'timestamp with time zone'
    ) THEN
        ALTER TABLE "Anuncios"
        ALTER COLUMN "DataDestaque"
        TYPE timestamp with time zone
        USING "DataDestaque" AT TIME ZONE 'UTC';

        RAISE NOTICE 'DataDestaque alterado para timestamp with time zone';
    ELSE
        RAISE NOTICE 'DataDestaque já é timestamp with time zone';
    END IF;

    -- Alterar DestaqueAte se necessário
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'Anuncios'
        AND column_name = 'DestaqueAte'
        AND data_type != 'timestamp with time zone'
    ) THEN
        ALTER TABLE "Anuncios"
        ALTER COLUMN "DestaqueAte"
        TYPE timestamp with time zone
        USING "DestaqueAte" AT TIME ZONE 'UTC';

        RAISE NOTICE 'DestaqueAte alterado para timestamp with time zone';
    ELSE
        RAISE NOTICE 'DestaqueAte já é timestamp with time zone';
    END IF;
END $$;

-- 3. Verificar resultado
SELECT
    column_name,
    data_type,
    udt_name
FROM information_schema.columns
WHERE table_name = 'Anuncios'
  AND column_name IN ('DataDestaque', 'DestaqueAte');

-- 4. Testar query que estava a dar erro
-- SELECT * FROM "Anuncios"
-- WHERE "Destacado" = true
-- AND "DestaqueAte" > NOW()
-- ORDER BY "Destacado" DESC, "DestaqueAte" DESC
-- LIMIT 5;

COMMIT;

-- Se algo correr mal, podes fazer ROLLBACK em vez de COMMIT
