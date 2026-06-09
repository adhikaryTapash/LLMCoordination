import { useState } from 'react';
import { Button } from '../../shared/components/Button';
import { approveAction } from './chatApi';
import { CardResultView } from './CardResultView';
import { ChartResultView } from './ChartResultView';
import { TableResultView } from './TableResultView';
import {
  isApprovalData,
  isCardData,
  isTableData,
  type ChatMessageItem,
  type ChatResponse,
} from './chatTypes';

interface ChatMessageProps {
  message: ChatMessageItem;
  onApprovalComplete: (response: ChatResponse, userMessage: string) => void;
}

function ResultView({ response }: { response: ChatResponse }) {
  const { viewType, data, answer } = response;

  if (viewType === 'text' || !data) {
    return <p className="message-text">{answer}</p>;
  }

  return (
    <>
      <p className="message-text">{answer}</p>
      {viewType === 'card' && isCardData(data) && <CardResultView data={data} />}
      {viewType === 'table' && isTableData(data) && <TableResultView data={data} />}
      {viewType === 'chart' && <ChartResultView />}
    </>
  );
}

export function ChatMessage({ message, onApprovalComplete }: ChatMessageProps) {
  const [approving, setApproving] = useState(false);
  const isUser = message.role === 'user';
  const response = message.response;

  const handleApproval = async (approved: boolean) => {
    if (!response || !message.pendingUserMessage) return;

    const approvalToken = isApprovalData(response.data)
      ? response.data.approvalToken ?? ''
      : '';

    setApproving(true);
    try {
      const result = await approveAction({
        conversationId: response.conversationId,
        approvalToken,
        message: message.pendingUserMessage,
        approved,
      });
      onApprovalComplete(result, message.pendingUserMessage);
    } finally {
      setApproving(false);
    }
  };

  return (
    <article className={`chat-message ${isUser ? 'chat-message-user' : 'chat-message-assistant'}`}>
      <div className="chat-message-bubble">
        {isUser ? (
          <p className="message-text">{message.text}</p>
        ) : response ? (
          <>
            <ResultView response={response} />
            {response.status === 'approval_required' && message.pendingUserMessage && (
              <div className="approval-actions">
                <p className="approval-note">This action requires your approval.</p>
                <div className="approval-buttons">
                  <Button
                    variant="primary"
                    loading={approving}
                    onClick={() => void handleApproval(true)}
                  >
                    Approve
                  </Button>
                  <Button
                    variant="danger"
                    disabled={approving}
                    onClick={() => void handleApproval(false)}
                  >
                    Deny
                  </Button>
                </div>
              </div>
            )}
            {response.intent && (
              <p className="message-meta">
                {response.intent} · {response.skill} · {response.status}
              </p>
            )}
          </>
        ) : (
          <p className="message-text">{message.text}</p>
        )}
      </div>
    </article>
  );
}
