namespace OrderSystem.Application.AI.Processing;

public interface ITextChunkingService
{
    IReadOnlyList<string> SplitIntoChunks(string text);
}
