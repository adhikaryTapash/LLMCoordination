namespace LLMCoordination.Application.Contracts;

public class CardResponse
{
    public int TotalRecords { get; set; }
    public List<CardItem> Cards { get; set; } = [];
}
