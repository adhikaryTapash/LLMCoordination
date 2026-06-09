namespace LLMCoordination.Application.Contracts;

public class TableResponse
{
    public int TotalRecords { get; set; }
    public List<TableColumn> Columns { get; set; } = [];
    public List<Dictionary<string, object>> Rows { get; set; } = [];
}
