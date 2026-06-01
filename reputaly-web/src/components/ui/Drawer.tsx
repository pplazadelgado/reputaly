import React from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';
import styles from './Drawer.module.css';

export interface DrawerProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

export function Drawer({ open, onClose, title, children, footer }: DrawerProps) {
  if (!open) return null;

  return createPortal(
    <>
      <div className={styles.overlay} onClick={onClose} aria-hidden="true" />
      <aside className={styles.drawer} role="dialog" aria-modal="true" aria-label={title}>
        {title && (
          <div className={styles.header}>
            <span className={styles.title}>{title}</span>
            <button className={styles.closeBtn} onClick={onClose} aria-label="Cerrar panel">
              <X size={18} strokeWidth={1.6} />
            </button>
          </div>
        )}
        <div className={styles.body}>{children}</div>
        {footer && <div className={styles.footer}>{footer}</div>}
      </aside>
    </>,
    document.body,
  );
}
