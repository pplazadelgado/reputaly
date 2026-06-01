import React, { createContext, useCallback, useContext, useState } from 'react';
import { createPortal } from 'react-dom';
import { X } from 'lucide-react';
import styles from './Toast.module.css';

export type ToastVariant = 'success' | 'error' | 'warning' | 'info';

interface ToastItem {
  id: string;
  variant: ToastVariant;
  message: string;
}

interface ToastContextValue {
  addToast: (message: string, variant?: ToastVariant) => void;
}

const ToastCtx = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const addToast = useCallback((message: string, variant: ToastVariant = 'info') => {
    const id = Math.random().toString(36).slice(2);
    setToasts((prev) => [...prev, { id, variant, message }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4500);
  }, []);

  const dismiss = (id: string) => setToasts((prev) => prev.filter((t) => t.id !== id));

  return (
    <ToastCtx.Provider value={{ addToast }}>
      {children}
      {createPortal(
        <div className={styles.container} aria-live="polite" aria-atomic="false">
          {toasts.map((t) => (
            <div key={t.id} className={[styles.toast, styles[t.variant]].join(' ')} role="alert">
              <div className={styles.bar} aria-hidden="true" />
              <div className={styles.body}>
                <p className={styles.message}>{t.message}</p>
              </div>
              <button
                className={styles.close}
                onClick={() => dismiss(t.id)}
                aria-label="Cerrar notificación"
              >
                <X size={14} strokeWidth={2} />
              </button>
            </div>
          ))}
        </div>,
        document.body,
      )}
    </ToastCtx.Provider>
  );
}

export function useToast(): ToastContextValue {
  const ctx = useContext(ToastCtx);
  if (!ctx) throw new Error('useToast debe usarse dentro de ToastProvider');
  return ctx;
}
