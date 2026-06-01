import React from 'react';
import styles from './Card.module.css';

export interface CardProps {
  children: React.ReactNode;
  padding?: string;
  className?: string;
  style?: React.CSSProperties;
}

export function Card({ children, padding = 'var(--space-6)', className, style }: CardProps) {
  return (
    <div
      className={[styles.card, className ?? ''].filter(Boolean).join(' ')}
      style={{ padding, ...style }}
    >
      {children}
    </div>
  );
}
