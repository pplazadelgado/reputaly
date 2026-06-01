import React from 'react';
import styles from './Field.module.css';

export interface FieldProps {
  label?: string;
  hint?: string;
  error?: string;
  children: React.ReactNode;
  className?: string;
}

export function Field({ label, hint, error, children, className }: FieldProps) {
  return (
    <div className={[styles.field, className ?? ''].filter(Boolean).join(' ')}>
      {label && <span className={styles.label}>{label}</span>}
      {children}
      {error && <span className={styles.errorMsg}>{error}</span>}
      {!error && hint && <span className={styles.hint}>{hint}</span>}
    </div>
  );
}

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: boolean;
  iconLeft?: React.ReactNode;
  suffix?: React.ReactNode;
}

export function Input({ error, iconLeft, suffix, className, ...props }: InputProps) {
  const inputClass = [
    styles.input,
    error ? styles.error : '',
    iconLeft ? styles.hasIcon : '',
    suffix ? styles.hasSuffix : '',
    className ?? '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={styles.wrapper}>
      {iconLeft && (
        <span className={styles.iconLeft} aria-hidden="true">
          {iconLeft}
        </span>
      )}
      <input className={inputClass} {...props} />
      {suffix && <span className={styles.suffix}>{suffix}</span>}
    </div>
  );
}

export interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: boolean;
  maxLength?: number;
}

export function Textarea({ error, maxLength, className, value, ...props }: TextareaProps) {
  const len = typeof value === 'string' ? value.length : 0;

  const textareaClass = [styles.textarea, error ? styles.error : '', className ?? '']
    .filter(Boolean)
    .join(' ');

  return (
    <>
      <textarea className={textareaClass} value={value} maxLength={maxLength} {...props} />
      {maxLength !== undefined && (
        <span
          className={[
            styles.charCount,
            len >= maxLength ? styles.over : len >= maxLength * 0.9 ? styles.near : '',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {len} / {maxLength}
        </span>
      )}
    </>
  );
}
