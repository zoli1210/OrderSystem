create table if not exists knowledge_documents (
    id uuid primary key default gen_random_uuid(),
    source text not null,
    title text not null,
    url text null,
    content text not null,
    embedding extensions.vector(1536) not null,
    created_at timestamptz not null default now()
);

create index if not exists knowledge_documents_embedding_idx
on knowledge_documents
using ivfflat (embedding extensions.vector_cosine_ops)
with (lists = 100);