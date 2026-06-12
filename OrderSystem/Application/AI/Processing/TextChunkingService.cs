using System.Text;
using System.Text.RegularExpressions;

namespace OrderSystem.Application.AI.Processing;

public class TextChunkingService : ITextChunkingService
{
    private const int MaxChunkLength = 1800;

    public IReadOnlyList<string> SplitIntoChunks(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var chunks = new List<string>();

        var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");

        var currentChunk = new StringBuilder();

        foreach (var sentence in sentences)
        {
            if (string.IsNullOrWhiteSpace(sentence))
            {
                continue;
            }

            if (currentChunk.Length + sentence.Length > MaxChunkLength)
            {
                AddCurrentChunkIfNotEmpty(chunks, currentChunk);
            }

            currentChunk.Append(sentence);
            currentChunk.Append(' ');
        }

        AddCurrentChunkIfNotEmpty(chunks, currentChunk);

        return chunks;
    }

    private static void AddCurrentChunkIfNotEmpty(List<string> chunks, StringBuilder currentChunk)
    {
        if (currentChunk.Length == 0)
        {
            return;
        }

        chunks.Add(currentChunk.ToString().Trim());
        currentChunk.Clear();
    }
}
