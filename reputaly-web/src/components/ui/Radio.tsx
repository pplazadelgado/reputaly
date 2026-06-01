import styles from './Radio.module.css';

export interface RadioProps {
  checked: boolean;
  onChange: (value: string) => void;
  value: string;
  label?: string;
  disabled?: boolean;
  name?: string;
  id?: string;
}

export function Radio({ checked, onChange, value, label, disabled = false, name, id }: RadioProps) {
  return (
    <label className={[styles.wrapper, disabled ? styles.disabled : ''].filter(Boolean).join(' ')}>
      <input
        type="radio"
        className={styles.nativeInput}
        checked={checked}
        onChange={() => onChange(value)}
        disabled={disabled}
        name={name}
        value={value}
        id={id}
      />
      <span className={[styles.circle, checked ? styles.checked : ''].filter(Boolean).join(' ')}>
        {checked && <span className={styles.dot} aria-hidden="true" />}
      </span>
      {label && <span className={styles.label}>{label}</span>}
    </label>
  );
}
