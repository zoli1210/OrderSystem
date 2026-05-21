using OrderSystem.Modules.AI.DTOs;

namespace OrderSystem.Modules.AI.Services.Documents;

public interface IKnowledgeDocumentIngestionService
{
    Task CreateAsync(CreateKnowledgeDocumentRequest request, CancellationToken cancellationToken);
}
