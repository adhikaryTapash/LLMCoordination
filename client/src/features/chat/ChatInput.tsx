import { useState, type FormEvent } from 'react';
import { Button } from '../../shared/components/Button';
import type { ViewType } from '../../shared/types/common';

interface ChatInputProps {
  onSend: (message: string, viewPreference?: ViewType) => void;
  disabled?: boolean;
}

export function ChatInput({ onSend, disabled = false }: ChatInputProps) {
  const [message, setMessage] = useState('');
  const [viewPreference, setViewPreference] = useState<ViewType | ''>('');

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    const trimmed = message.trim();
    if (!trimmed) return;
    onSend(trimmed, viewPreference || undefined);
    setMessage('');
  };

  return (
    <form className="chat-input" onSubmit={handleSubmit}>
      <div className="chat-input-row">
        <textarea
          className="chat-textarea"
          value={message}
          onChange={(event) => setMessage(event.target.value)}
          placeholder="Ask something…"
          rows={2}
          disabled={disabled}
        />
        <div className="chat-input-actions">
          <select
            className="chat-select"
            value={viewPreference}
            onChange={(event) => setViewPreference(event.target.value as ViewType | '')}
            disabled={disabled}
            aria-label="View preference"
          >
            <option value="">Auto view</option>
            <option value="card">Card</option>
            <option value="table">Table</option>
            <option value="chart">Chart</option>
            <option value="text">Text</option>
          </select>
          <Button type="submit" loading={disabled}>
            Send
          </Button>
        </div>
      </div>
    </form>
  );
}
