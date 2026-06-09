import { useEffect, useState } from 'react';
import { httpClient } from '../../shared/api/httpClient';
import { Loading } from '../../shared/components/Loading';
import { Button } from '../../shared/components/Button';

export interface AuditLogEntry {
  id: string;
  tenantId: string;
  userId: string;
  conversationId: string | null;
  agentName: string;
  skillName: string | null;
  toolName: string | null;
  status: string;
  executionMs: number | null;
  createdAt: string;
}

export function AuditLogPage() {
  const [logs, setLogs] = useState<AuditLogEntry[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const fetchLogs = async () => {
      setLoading(true);
      setError(null);
      try {
        const { data } = await httpClient.get<AuditLogEntry[]>('/api/audit/logs', {
          params: { page, pageSize: 50 },
        });
        if (!cancelled) {
          setLogs(data);
        }
      } catch {
        if (!cancelled) {
          setError('Unable to load audit logs. Admin role may be required.');
          setLogs([]);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void fetchLogs();
    return () => {
      cancelled = true;
    };
  }, [page]);

  return (
    <div className="audit-page">
      <header className="page-header">
        <h1>Audit Logs</h1>
        <p className="page-subtitle">Agent execution history for your tenant.</p>
      </header>

      {loading && <Loading label="Loading audit logs…" />}
      {error && <p className="form-error">{error}</p>}

      {!loading && !error && (
        <>
          <div className="audit-table-wrap">
            <table className="result-table">
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Agent</th>
                  <th>Skill</th>
                  <th>Tool</th>
                  <th>Status</th>
                  <th>Ms</th>
                </tr>
              </thead>
              <tbody>
                {logs.length === 0 ? (
                  <tr>
                    <td colSpan={6}>No audit logs found.</td>
                  </tr>
                ) : (
                  logs.map((log) => (
                    <tr key={log.id}>
                      <td>{new Date(log.createdAt).toLocaleString()}</td>
                      <td>{log.agentName}</td>
                      <td>{log.skillName ?? '—'}</td>
                      <td>{log.toolName ?? '—'}</td>
                      <td>{log.status}</td>
                      <td>{log.executionMs ?? '—'}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <div className="pagination">
            <Button
              variant="secondary"
              disabled={page <= 1}
              onClick={() => setPage((current) => Math.max(1, current - 1))}
            >
              Previous
            </Button>
            <span className="pagination-label">Page {page}</span>
            <Button
              variant="secondary"
              disabled={logs.length < 50}
              onClick={() => setPage((current) => current + 1)}
            >
              Next
            </Button>
          </div>
        </>
      )}
    </div>
  );
}
