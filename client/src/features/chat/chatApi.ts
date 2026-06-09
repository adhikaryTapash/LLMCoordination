import { httpClient } from '../../shared/api/httpClient';
import type { ApprovalRequest, ChatRequest, ChatResponse } from './chatTypes';

export async function sendMessage(request: ChatRequest): Promise<ChatResponse> {
  const { data } = await httpClient.post<ChatResponse>('/api/chat/message', request);
  return data;
}

export async function approveAction(request: ApprovalRequest): Promise<ChatResponse> {
  const { data } = await httpClient.post<ChatResponse>('/api/chat/approve', request);
  return data;
}
