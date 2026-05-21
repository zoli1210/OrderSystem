create table if not exists knowledge_sources (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    url text null,
    source_type text not null,
    is_active boolean not null default true,
    refresh_interval_hours int null,
    last_ingested_at timestamptz null,
    last_ingestion_status text null,
    last_error text null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create index if not exists knowledge_sources_source_type_idx
on knowledge_sources (source_type);

create index if not exists knowledge_sources_is_active_idx
on knowledge_sources (is_active);