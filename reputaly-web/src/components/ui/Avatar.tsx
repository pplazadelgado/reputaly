import styles from './Avatar.module.css';

export interface AvatarProps {
  name: string;
  src?: string;
  size?: number;
  className?: string;
}

function initials(name: string): string {
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
}

export function Avatar({ name, src, size = 36, className }: AvatarProps) {
  const fontSize = Math.round(size * 0.38);

  return (
    <span
      className={[styles.avatar, className ?? ''].filter(Boolean).join(' ')}
      style={{ width: size, height: size, fontSize }}
      aria-label={name}
      title={name}
    >
      {src ? (
        <img src={src} alt={name} className={styles.img} />
      ) : (
        initials(name)
      )}
    </span>
  );
}
