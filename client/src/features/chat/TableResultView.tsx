import type { TableData } from './chatTypes';

interface TableResultViewProps {
  data: TableData;
}

function formatCell(value: unknown): string {
  if (value === null || value === undefined) return '';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

export function TableResultView({ data }: TableResultViewProps) {
  return (
    <div className="result-table-wrap">
      <p className="result-meta">{data.totalRecords} record(s)</p>
      <table className="result-table">
        <thead>
          <tr>
            {data.columns.map((col) => (
              <th key={col.key}>{col.label}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.rows.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {data.columns.map((col) => (
                <td key={col.key}>{formatCell(row[col.key])}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
