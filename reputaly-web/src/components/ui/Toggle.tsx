import styles from './Toggle.module.css';

export interface ToggleProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  id?: string;
}

export function Toggle({ checked, onChange, label, disabled = false, id }: ToggleProps) {
  return (
    <label className={[styles.wrapper, disabled ? styles.disabled : ''].filter(Boolean).join(' ')}>
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        id={id}
        className={[styles.track, checked ? styles.checked : ''].filter(Boolean).join(' ')}
        disabled={disabled}
        onClick={() => onChange(!checked)}
      >
        <span className={styles.thumb} aria-hidden="true" />
      </button>
      {label && <span className={styles.label}>{label}</span>}
    </label>
  );
}
