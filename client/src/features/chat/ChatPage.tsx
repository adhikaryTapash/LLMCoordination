import { useRef, useState } from 'react';
import { Loading } from '../../shared/components/Loading';
import { sendMessage } from './chatApi';
import { ChatInput } from './ChatInput';
import { ChatMessage } from './ChatMessage';
import type { ChatMessageItem, ChatResponse } from './chatTypes';
import type { ViewType } from '../../shared/types/common';

function createId(): string {
  return crypto.randomUUID();
}

export function ChatPage() {
  const [messages, setMessages] = useState<ChatMessageItem[]>([]);
  const [conversationId, setConversationId] = useState<string | undefined>();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const appendMessages = (userText: string, response: ChatResponse) => {
    setConversationId(response.conversationId);
    setMessages((prev) => [
      ...prev,
      { id: createId(), role: 'user', text: userText },
      {
        id: response.messageId || createId(),
        role: 'assistant',
        text: response.answer,
        response,
        pendingUserMessage: userText,
      },
    ]);
    requestAnimationFrame(() => {
      listRef.current?.scrollTo({ top: listRef.current.scrollHeight, behavior: 'smooth' });
    });
  };

  const handleSend = async (text: string, viewPreference?: ViewType) => {
    setLoading(true);
    setError(null);
    try {
      const response = await sendMessage({
        conversationId,
        message: text,
        viewPreference,
      });
      appendMessages(text, response);
    } catch {
      setError('Failed to send message. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleApprovalComplete = (response: ChatResponse, userMessage: string) => {
    setMessages((prev) => {
      const updated = prev.map((item) => {
        if (item.role === 'assistant' && item.pendingUserMessage === userMessage && item.response?.status === 'approval_required') {
          return { ...item, response: { ...item.response, status: 'resolved' } };
        }
        return item;
      });
      return [
        ...updated,
        {
          id: response.messageId || createId(),
          role: 'assistant' as const,
          text: response.answer,
          response,
          pendingUserMessage: userMessage,
        },
      ];
    });
  };

  return (
    <div className="chat-page">
      <header className="page-header">
        <h1>Chat</h1>
        <p className="page-subtitle">Ask questions and get structured responses from your tenant agents.</p>
      </header>

      <div className="chat-panel">
        <div className="chat-messages" ref={listRef}>
          {messages.length === 0 && !loading && (
            <div className="chat-empty">
              <p>Try: &quot;Give me the list of patients in card view&quot;</p>
            </div>
          )}
          {messages.map((message) => (
            <ChatMessage
              key={message.id}
              message={message}
              onApprovalComplete={handleApprovalComplete}
            />
          ))}
          {loading && <Loading label="Thinking…" />}
        </div>

        {error && <p className="form-error">{error}</p>}

        <ChatInput onSend={(text, pref) => void handleSend(text, pref)} disabled={loading} />
      </div>
    </div>
  );
}
