namespace LLMCoordination.Application.Contracts;

public class CardItem
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<CardField> Fields { get; set; } = [];
    public List<CardAction> Actions { get; set; } = [];
}
