import React from 'react';
import styles from './Button.module.css';

type Variant = 'primary' | 'secondary' | 'ghost' | 'blue' | 'destructive';
type Size = 'sm' | 'md' | 'lg';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  icon?: React.ReactNode;
}

export function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  children,
  disabled,
  className,
  ...props
}: ButtonProps) {
  const classes = [
    styles.btn,
    styles[variant],
    styles[size],
    loading ? styles.loading : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <button className={classes} disabled={disabled ?? loading} aria-busy={loading} {...props}>
      {loading ? (
        <span className={styles.spinner} aria-hidden="true" />
      ) : icon ? (
        <span className={styles.icon} aria-hidden="true">
          {icon}
        </span>
      ) : null}
      {children}
    </button>
  );
}
