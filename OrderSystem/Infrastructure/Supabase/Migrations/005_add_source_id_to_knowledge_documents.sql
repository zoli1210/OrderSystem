alter table knowledge_documents
add column if not exists source_id uuid null;

alter table knowledge_documents
add constraint fk_knowledge_documents_source
foreign key (source_id)
references knowledge_sources(id)
on delete set null;

create index if not exists knowledge_documents_source_id_idx
on knowledge_documents (source_id);