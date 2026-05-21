namespace OrderSystem.Modules.AI.Services.Shared;

public interface ITextChunkingService
{
    IReadOnlyList<string> SplitIntoChunks(string text);
}
