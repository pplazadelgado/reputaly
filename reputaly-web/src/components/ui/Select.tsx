import { ChevronDown } from 'lucide-react';
import styles from './Select.module.css';

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps {
  options: SelectOption[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  id?: string;
  name?: string;
}

export function Select({
  options,
  value,
  onChange,
  placeholder,
  disabled = false,
  error = false,
  id,
  name,
}: SelectProps) {
  return (
    <div className={styles.wrapper}>
      <select
        id={id}
        name={name}
        className={[styles.select, error ? styles.error : ''].filter(Boolean).join(' ')}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={disabled}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      <span className={styles.chevron} aria-hidden="true">
        <ChevronDown size={16} strokeWidth={1.6} />
      </span>
    </div>
  );
}
