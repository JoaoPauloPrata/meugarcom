-- Optional photo per menu item, stored in the public bucket menu-images.
-- Only the path is stored; the API derives the public URL from SUPABASE_URL.

alter table public.menu_items
  add column image_path text check (char_length(image_path) between 1 and 300);

-- Public read (customers see the photos in the chat). Writes only through the .NET API with the
-- service key, which bypasses RLS; no storage.objects policies are granted to anon/authenticated.
insert into storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
values ('menu-images', 'menu-images', true, 5242880, array['image/jpeg', 'image/png', 'image/webp'])
on conflict (id) do update
  set public = excluded.public,
      file_size_limit = excluded.file_size_limit,
      allowed_mime_types = excluded.allowed_mime_types;
