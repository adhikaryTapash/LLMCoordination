import type { ReactNode } from 'react';

interface CardProps {
  title: string;
  subtitle?: string;
  children?: ReactNode;
  footer?: ReactNode;
}

export function Card({ title, subtitle, children, footer }: CardProps) {
  return (
    <article className="data-card">
      <header className="data-card-header">
        <h3 className="data-card-title">{title}</h3>
        {subtitle && <p className="data-card-subtitle">{subtitle}</p>}
      </header>
      {children && <div className="data-card-body">{children}</div>}
      {footer && <footer className="data-card-footer">{footer}</footer>}
    </article>
  );
}
