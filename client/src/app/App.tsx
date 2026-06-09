import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Button } from '../shared/components/Button';
import { useAuthStore } from '../features/auth/authStore';

const navItems = [
  { to: '/chat', label: 'Chat' },
  { to: '/tenant', label: 'Tenant' },
  { to: '/swagger', label: 'Swagger' },
  { to: '/mcp', label: 'MCP' },
  { to: '/skills', label: 'Skills' },
  { to: '/audit', label: 'Audit' },
];

export function App() {
  const navigate = useNavigate();
  const fullName = useAuthStore((state) => state.fullName);
  const role = useAuthStore((state) => state.role);
  const logout = useAuthStore((state) => state.logout);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="brand-mark">LC</span>
          <div>
            <strong>LLM Coordination</strong>
            <p>Tenant workspace</p>
          </div>
        </div>

        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `nav-link${isActive ? ' nav-link-active' : ''}`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <div className="topbar-user">
            <span>{fullName ?? 'User'}</span>
            {role && <span className="topbar-role">{role}</span>}
          </div>
          <Button variant="ghost" onClick={handleLogout}>
            Log out
          </Button>
        </header>

        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
