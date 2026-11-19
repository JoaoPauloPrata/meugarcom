-- MVP schema (modality 1: assistant only).
-- Hierarchy: accounts -> restaurants -> units -> (menus, dining_tables, conversations).
-- All access goes through the .NET API (role meugarcom_api). PostgREST roles (anon,
-- authenticated) have no grants on these tables: RLS is enabled with no policies (deny all).

create table public.accounts (
  id uuid primary key default gen_random_uuid(),
  owner_user_id uuid not null unique references auth.users (id) on delete cascade,
  name text not null check (char_length(btrim(name)) between 1 and 120),
  created_at timestamptz not null default now()
);

create table public.restaurants (
  id uuid primary key default gen_random_uuid(),
  account_id uuid not null references public.accounts (id) on delete cascade,
  name text not null check (char_length(btrim(name)) between 1 and 160),
  cuisine_type text check (cuisine_type is null or char_length(cuisine_type) <= 60),
  created_at timestamptz not null default now()
);
create index restaurants_account_id_idx on public.restaurants (account_id);

create table public.units (
  id uuid primary key default gen_random_uuid(),
  restaurant_id uuid not null references public.restaurants (id) on delete cascade,
  name text not null check (char_length(btrim(name)) between 1 and 120),
  city text check (city is null or char_length(city) <= 120),
  public_code text not null unique check (public_code ~ '^[a-z0-9]{6,16}$'),
  combo_budget_tolerance numeric(3, 2) not null default 0.20
    check (combo_budget_tolerance between 0 and 0.5),
  created_at timestamptz not null default now()
);
create index units_restaurant_id_idx on public.units (restaurant_id);

create table public.menus (
  id uuid primary key default gen_random_uuid(),
  unit_id uuid not null unique references public.units (id) on delete cascade,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table public.menu_categories (
  id uuid primary key default gen_random_uuid(),
  menu_id uuid not null references public.menus (id) on delete cascade,
  name text not null check (char_length(btrim(name)) between 1 and 80),
  position integer not null default 0,
  created_at timestamptz not null default now()
);
create index menu_categories_menu_id_idx on public.menu_categories (menu_id);

create table public.menu_items (
  id uuid primary key default gen_random_uuid(),
  menu_id uuid not null references public.menus (id) on delete cascade,
  category_id uuid not null references public.menu_categories (id) on delete cascade,
  name text not null check (char_length(btrim(name)) between 1 and 120),
  description text check (description is null or char_length(description) <= 1000),
  ingredients text check (ingredients is null or char_length(ingredients) <= 1000),
  price numeric(10, 2) not null check (price >= 0),
  serves_people smallint check (serves_people is null or serves_people between 1 and 50),
  portion text check (portion is null or char_length(portion) <= 60),
  is_spicy boolean not null default false,
  -- Dietary flags: set only by a person in the menu settings, never by the import.
  is_vegan boolean not null default false,
  is_lactose_free boolean not null default false,
  is_gluten_free boolean not null default false,
  is_available boolean not null default true,
  position integer not null default 0,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
create index menu_items_menu_id_idx on public.menu_items (menu_id);
create index menu_items_category_id_idx on public.menu_items (category_id);

create table public.dining_tables (
  id uuid primary key default gen_random_uuid(),
  unit_id uuid not null references public.units (id) on delete cascade,
  number integer not null check (number between 1 and 9999),
  public_code text not null unique check (public_code ~ '^[a-z0-9]{6,16}$'),
  created_at timestamptz not null default now(),
  unique (unit_id, number)
);

create table public.conversations (
  id uuid primary key default gen_random_uuid(),
  unit_id uuid not null references public.units (id) on delete cascade,
  dining_table_id uuid references public.dining_tables (id) on delete set null,
  language text check (language is null or char_length(language) <= 16),
  status text not null default 'active' check (status in ('active', 'closed')),
  customer_message_count integer not null default 0,
  abuse_count integer not null default 0,
  started_at timestamptz not null default now(),
  last_message_at timestamptz
);
create index conversations_unit_id_idx on public.conversations (unit_id, started_at desc);
create index conversations_dining_table_id_idx on public.conversations (dining_table_id);

create table public.conversation_messages (
  id uuid primary key default gen_random_uuid(),
  conversation_id uuid not null references public.conversations (id) on delete cascade,
  role text not null check (role in ('customer', 'assistant')),
  content text not null check (char_length(content) <= 4000),
  referenced_item_ids uuid[] not null default '{}',
  combo jsonb,
  outcome text check (outcome is null or outcome in ('answered', 'blocked', 'abuse', 'safe_fallback')),
  provider text,
  model text,
  created_at timestamptz not null default now()
);
create index conversation_messages_conversation_id_idx
  on public.conversation_messages (conversation_id, created_at);

-- Default AI provider/model (restaurant_id null = global default). Never hardcoded in code.
create table public.ai_model_settings (
  id uuid primary key default gen_random_uuid(),
  restaurant_id uuid unique references public.restaurants (id) on delete cascade,
  primary_provider text not null check (primary_provider in ('openai', 'anthropic')),
  primary_model text not null,
  primary_effort text,
  fallback_provider text check (fallback_provider is null or fallback_provider in ('openai', 'anthropic')),
  fallback_model text,
  fallback_effort text,
  classifier_provider text check (classifier_provider is null or classifier_provider in ('openai', 'anthropic')),
  classifier_model text,
  temperature numeric(3, 2) check (temperature is null or temperature between 0 and 2),
  max_output_tokens integer not null default 700 check (max_output_tokens between 100 and 4000),
  updated_at timestamptz not null default now()
);
create unique index ai_model_settings_single_global_idx
  on public.ai_model_settings ((restaurant_id is null)) where restaurant_id is null;

create table public.menu_imports (
  id uuid primary key default gen_random_uuid(),
  unit_id uuid not null references public.units (id) on delete cascade,
  file_name text not null check (char_length(file_name) <= 255),
  status text not null check (status in ('draft', 'published', 'discarded')),
  draft jsonb not null default '[]'::jsonb,
  item_count integer not null default 0,
  created_at timestamptz not null default now(),
  published_at timestamptz
);
create index menu_imports_unit_id_idx on public.menu_imports (unit_id, created_at desc);

-- Blocked attempts and guardrail failures, without message content (no personal data).
create table public.guardrail_events (
  id uuid primary key default gen_random_uuid(),
  unit_id uuid not null references public.units (id) on delete cascade,
  conversation_id uuid references public.conversations (id) on delete set null,
  kind text not null check (char_length(kind) <= 60),
  created_at timestamptz not null default now()
);
create index guardrail_events_unit_id_idx on public.guardrail_events (unit_id, created_at desc);
create index guardrail_events_conversation_id_idx on public.guardrail_events (conversation_id);

-- Lock PostgREST out: everything goes through the API.
do $$
declare t text;
begin
  foreach t in array array[
    'accounts', 'restaurants', 'units', 'menus', 'menu_categories', 'menu_items',
    'dining_tables', 'conversations', 'conversation_messages', 'ai_model_settings',
    'menu_imports', 'guardrail_events'
  ] loop
    execute format('alter table public.%I enable row level security', t);
    execute format('revoke all on public.%I from anon, authenticated', t);
  end loop;
end $$;

-- Least-privilege role used by the .NET API (password is set outside of migrations).
do $$
begin
  if not exists (select 1 from pg_roles where rolname = 'meugarcom_api') then
    create role meugarcom_api login bypassrls;
  end if;
end $$;

grant usage on schema public to meugarcom_api;
grant select, insert, update, delete on
  public.accounts, public.restaurants, public.units, public.menus, public.menu_categories,
  public.menu_items, public.dining_tables, public.conversations, public.conversation_messages,
  public.menu_imports, public.guardrail_events
  to meugarcom_api;
grant select on public.ai_model_settings to meugarcom_api;

insert into public.ai_model_settings (
  restaurant_id, primary_provider, primary_model, primary_effort,
  fallback_provider, fallback_model, fallback_effort,
  classifier_provider, classifier_model, temperature, max_output_tokens
) values (
  null, 'openai', 'gpt-5.4-mini', 'none',
  'anthropic', 'claude-opus-5-5', 'low',
  'openai', 'gpt-5.4-nano', 0.2, 700
);
