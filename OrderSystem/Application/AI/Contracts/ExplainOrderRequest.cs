namespace OrderSystem.Modules.AI.DTOs;

public class ExplainOrderRequest
{
    public string Question { get; set; }

    public int? MatchCount { get; set; }
}
