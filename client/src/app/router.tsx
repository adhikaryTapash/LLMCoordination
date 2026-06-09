import { Navigate, createBrowserRouter } from 'react-router-dom';
import { App } from './App';
import { LoginPage } from '../features/auth/LoginPage';
import { useAuthStore } from '../features/auth/authStore';
import { ChatPage } from '../features/chat/ChatPage';
import { TenantDashboard } from '../features/tenant/TenantDashboard';
import { SwaggerUploadPage } from '../features/swagger/SwaggerUploadPage';
import { McpServerPage } from '../features/mcp/McpServerPage';
import { SkillRegistryPage } from '../features/skills/SkillRegistryPage';
import { AuditLogPage } from '../features/audit/AuditLogPage';
import type { ReactNode } from 'react';

function ProtectedRoute({ children }: { children: ReactNode }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated());
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <App />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <Navigate to="/chat" replace /> },
      { path: 'chat', element: <ChatPage /> },
      { path: 'tenant', element: <TenantDashboard /> },
      { path: 'swagger', element: <SwaggerUploadPage /> },
      { path: 'mcp', element: <McpServerPage /> },
      { path: 'skills', element: <SkillRegistryPage /> },
      { path: 'audit', element: <AuditLogPage /> },
    ],
  },
]);
