import type { ViewType } from '../../shared/types/common';

export interface ChatRequest {
  conversationId?: string;
  message: string;
  viewPreference?: ViewType;
}

export interface CardField {
  label: string;
  value: string;
}

export interface CardAction {
  label: string;
  action: string;
  entityId: string;
}

export interface CardItem {
  title: string;
  subtitle: string;
  fields: CardField[];
  actions: CardAction[];
}

export interface CardData {
  totalRecords: number;
  cards: CardItem[];
}

export interface TableColumn {
  key: string;
  label: string;
}

export interface TableData {
  totalRecords: number;
  columns: TableColumn[];
  rows: Record<string, unknown>[];
}

export interface ApprovalData {
  approvalToken?: string;
  action?: string;
}

export interface ChatResponse {
  conversationId: string;
  messageId: string;
  intent: string;
  skill: string;
  viewType: ViewType;
  status: string;
  answer: string;
  data?: CardData | TableData | ApprovalData | unknown;
}

export interface ApprovalRequest {
  conversationId?: string;
  approvalToken: string;
  message: string;
  approved: boolean;
}

export interface ChatMessageItem {
  id: string;
  role: 'user' | 'assistant';
  text: string;
  response?: ChatResponse;
  pendingUserMessage?: string;
}

export function isCardData(data: unknown): data is CardData {
  return (
    typeof data === 'object' &&
    data !== null &&
    'cards' in data &&
    Array.isArray((data as CardData).cards)
  );
}

export function isTableData(data: unknown): data is TableData {
  return (
    typeof data === 'object' &&
    data !== null &&
    'columns' in data &&
    'rows' in data &&
    Array.isArray((data as TableData).columns) &&
    Array.isArray((data as TableData).rows)
  );
}

export function isApprovalData(data: unknown): data is ApprovalData {
  return typeof data === 'object' && data !== null && 'approvalToken' in data;
}
