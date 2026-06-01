import { UserButton } from '@clerk/clerk-react';
import { Search, Bell } from 'lucide-react';
import styles from './TopBar.module.css';

export interface TopBarProps {
  title: string;
  subtitle?: string;
  notifications?: boolean;
}

export default function TopBar({ title, subtitle, notifications = false }: TopBarProps) {
  return (
    <header className={styles.topbar}>
      <div className={styles.titles}>
        <h1 className={styles.title}>{title}</h1>
        {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
      </div>

      <div className={styles.actions}>
        <div className={styles.searchWrapper}>
          <span className={styles.searchIcon} aria-hidden="true">
            <Search size={16} strokeWidth={1.6} />
          </span>
          <input
            className={styles.searchInput}
            type="search"
            placeholder="Buscar..."
            aria-label="Buscar"
          />
        </div>

        <button
          className={styles.bellBtn}
          aria-label={notifications ? 'Notificaciones sin leer' : 'Notificaciones'}
          type="button"
        >
          <Bell size={16} strokeWidth={1.6} />
          {notifications && <span className={styles.bellDot} aria-hidden="true" />}
        </button>

        <UserButton
          appearance={{
            elements: {
              avatarBox: {
                width: 36,
                height: 36,
              },
            },
          }}
        />
      </div>
    </header>
  );
}
