create or replace function match_knowledge_documents (
    query_embedding extensions.vector(1536),
    match_count int default 5
)
returns table (
    id uuid,
    source text,
    title text,
    url text,
    content text,
    similarity float
)
language sql stable
as $$
    select
        knowledge_documents.id,
        knowledge_documents.source,
        knowledge_documents.title,
        knowledge_documents.url,
        knowledge_documents.content,
        1 - (knowledge_documents.embedding <=> query_embedding) as similarity
    from knowledge_documents
    order by knowledge_documents.embedding <=> query_embedding
    limit match_count;
$$;