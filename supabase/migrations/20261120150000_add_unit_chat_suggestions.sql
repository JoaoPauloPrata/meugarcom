-- Welcome suggestions of the customer chat, configurable per unit.
-- Array of {"text": string, "icon": string}; the API validates text length and icon keys.
-- Existing units get the suggestions that used to be hardcoded in the chat.

alter table public.units
  add column chat_suggestions jsonb not null default '[]'::jsonb
    check (jsonb_typeof(chat_suggestions) = 'array' and jsonb_array_length(chat_suggestions) <= 3);

update public.units
set chat_suggestions = '[
  {"text": "O que você recomenda?", "icon": "chat"},
  {"text": "Quais opções são sem glúten?", "icon": "wheatOff"},
  {"text": "Que vinho combina com os pratos principais?", "icon": "wine"}
]'::jsonb;
