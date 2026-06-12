using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Application.AI.Documents;

public interface IKnowledgeDocumentIngestionService
{
    Task CreateAsync(CreateKnowledgeDocumentRequest request, CancellationToken cancellationToken);
}
