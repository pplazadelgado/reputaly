import { useState } from 'react';
import { X } from 'lucide-react';
import styles from './ChipInput.module.css';

export interface ChipInputProps {
  values: string[];
  onChange: (values: string[]) => void;
  placeholder?: string;
  id?: string;
}

export function ChipInput({ values, onChange, placeholder = 'Escribe y pulsa Enter…', id }: ChipInputProps) {
  const [input, setInput] = useState('');

 function commitInput() {
  const next = input.trim();
  if (next && !values.includes(next)) {
    onChange([...values, next]);
  }
  setInput('');
}

function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
  if ((e.key === 'Enter' || e.key === ',') && input.trim()) {
    e.preventDefault();
    commitInput();
  } else if (e.key === 'Backspace' && !input && values.length > 0) {
    onChange(values.slice(0, -1));
  }
}

  function remove(val: string) {
    onChange(values.filter((v) => v !== val));
  }

  return (
    <div className={styles.wrapper}>
      {values.map((v) => (
        <span key={v} className={styles.chip}>
          {v}
          <button
            type="button"
            className={styles.chipRemove}
            onClick={() => remove(v)}
            aria-label={`Eliminar ${v}`}
          >
            <X size={12} strokeWidth={2} />
          </button>
        </span>
      ))}
      <input
        id={id}
        className={styles.input}
        value={input}
        onChange={(e) => setInput(e.target.value)}
        onKeyDown={handleKeyDown}
        onBlur={commitInput}
        placeholder={values.length === 0 ? placeholder : ''}
        aria-label="Añadir palabra clave"
      />
    </div>
  );
}
