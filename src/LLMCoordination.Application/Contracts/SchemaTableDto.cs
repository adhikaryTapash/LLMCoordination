namespace LLMCoordination.Application.Contracts;

public class SchemaTableDto
{
    public string TableName { get; set; } = string.Empty;
    public List<SchemaColumnDto> Columns { get; set; } = [];
}

public class SchemaColumnDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
}
