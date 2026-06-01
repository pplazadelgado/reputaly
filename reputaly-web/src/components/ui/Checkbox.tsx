import { Check } from 'lucide-react';
import styles from './Checkbox.module.css';

export interface CheckboxProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  id?: string;
}

export function Checkbox({ checked, onChange, label, disabled = false, id }: CheckboxProps) {
  return (
    <label className={[styles.wrapper, disabled ? styles.disabled : ''].filter(Boolean).join(' ')}>
      <input
        type="checkbox"
        className={styles.nativeInput}
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        disabled={disabled}
        id={id}
      />
      <span className={[styles.box, checked ? styles.checked : ''].filter(Boolean).join(' ')}>
        {checked && (
          <span className={styles.checkmark}>
            <Check size={12} strokeWidth={2.5} />
          </span>
        )}
      </span>
      {label && <span className={styles.label}>{label}</span>}
    </label>
  );
}
