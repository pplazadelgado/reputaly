import TopBar from '../components/layout/TopBar';
import { Card } from '../components/ui';
import styles from './ComingSoon.module.css';

interface ComingSoonProps {
  title: string;
  subtitle?: string;
}

export default function ComingSoon({ title, subtitle }: ComingSoonProps) {
  return (
    <>
      <TopBar title={title} subtitle={subtitle} />
      <div className={styles.content}>
        <Card padding="var(--space-10)">
          <div className={styles.inner}>
            <div className={styles.icon} aria-hidden="true">🚧</div>
            <h2 className={styles.heading}>Próximamente</h2>
            <p className={styles.text}>
              Esta sección está en desarrollo. Estará disponible en una próxima actualización.
            </p>
          </div>
        </Card>
      </div>
    </>
  );
}
