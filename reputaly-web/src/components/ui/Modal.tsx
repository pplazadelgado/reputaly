import React from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';
import styles from './Modal.module.css';
import { Button } from './Button';

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}

export function Modal({ open, onClose, title, children, footer }: ModalProps) {
  if (!open) return null;

  return createPortal(
    <div className={styles.overlay} onClick={onClose} role="dialog" aria-modal="true" aria-label={title}>
      <div className={styles.modal} onClick={(e) => e.stopPropagation()}>
        {title && (
          <div className={styles.header}>
            <span className={styles.title}>{title}</span>
            <button className={styles.closeBtn} onClick={onClose} aria-label="Cerrar">
              <X size={18} strokeWidth={1.6} />
            </button>
          </div>
        )}
        <div className={styles.body}>{children}</div>
        {footer && <div className={styles.footer}>{footer}</div>}
      </div>
    </div>,
    document.body,
  );
}

export interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  destructive?: boolean;
  confirmLabel?: string;
  cancelLabel?: string;
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  destructive = false,
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
}: ConfirmDialogProps) {
  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="secondary" size="sm" onClick={onClose}>
            {cancelLabel}
          </Button>
          <Button
            variant={destructive ? 'destructive' : 'primary'}
            size="sm"
            onClick={() => {
              onConfirm();
              onClose();
            }}
          >
            {confirmLabel}
          </Button>
        </>
      }
    >
      <p style={{ fontSize: 14, color: 'var(--slate-700)', lineHeight: 1.5 }}>{message}</p>
    </Modal>
  );
}
