-- Tabela da lista de espera do Meu Garçom
-- (aplicada no projeto pwgqtlbzkancphqqcqkf como migration "create_waitlist")
create table if not exists public.waitlist (
  id uuid primary key default gen_random_uuid(),
  created_at timestamptz not null default now(),
  nome text not null check (char_length(btrim(nome)) between 1 and 120),
  email text not null unique check (
    char_length(email) <= 254
    and email = lower(email)
    and email ~ '^[^\s@]+@[^\s@]+\.[^\s@]+$'
  ),
  whatsapp text check (whatsapp is null or char_length(whatsapp) <= 20),
  restaurante text not null check (char_length(btrim(restaurante)) between 1 and 160),
  cidade text check (cidade is null or char_length(cidade) <= 120),
  unidades text check (unidades is null or unidades in ('1', '2-5', '6+')),
  origem text check (origem is null or char_length(origem) <= 60)
);

alter table public.waitlist enable row level security;

-- Visitantes anônimos podem apenas inserir (não podem ler, editar nem apagar)
revoke all on public.waitlist from anon, authenticated;
grant insert on public.waitlist to anon;

create policy "waitlist_insert_anon"
  on public.waitlist for insert
  to anon
  with check (created_at >= now() - interval '1 minute');
