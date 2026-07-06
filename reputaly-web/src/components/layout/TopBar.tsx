import { UserButton } from '@clerk/clerk-react';
import styles from './TopBar.module.css';

export interface TopBarProps {
  title: string;
  subtitle?: string;
}

export default function TopBar({ title, subtitle }: TopBarProps) {
  return (
    <header className={styles.topbar}>
      <div className={styles.titles}>
        <h1 className={styles.title}>{title}</h1>
        {subtitle && <p className={styles.subtitle}>{subtitle}</p>}
      </div>

      <div className={styles.actions}>
        <UserButton
          appearance={{
            elements: {
              avatarBox: {
                width: 36,
                height: 36,
              },
              userButtonPopoverActionButton__manageAccount: {
                display: 'none',
              },
              userButtonPopoverActionButton__addAccount: {
                display: 'none',
              },
            },
          }}
        />
      </div>
    </header>
  );
}
