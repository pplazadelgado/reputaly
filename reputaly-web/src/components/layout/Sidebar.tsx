import { NavLink } from 'react-router-dom';
import { useOrganization } from '@clerk/clerk-react';
import {
  LayoutGrid,
  Star,
  Settings,
  Users,
  CreditCard,
  HelpCircle,
} from 'lucide-react';
import { Wordmark } from '../../assets/logo';
import { ProgressBar } from '../ui';
import styles from './Sidebar.module.css';
import { useBilling } from '../../context/BillingContext';

interface NavItem {
  to: string;
  icon: React.ReactNode;
  label: string;
  adminOnly?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { to: '/dashboard', icon: <LayoutGrid size={18} strokeWidth={1.6} />, label: 'Panel' },
  { to: '/reviews', icon: <Star size={18} strokeWidth={1.6} />, label: 'Reseñas' },
  { to: '/settings', icon: <Settings size={18} strokeWidth={1.6} />, label: 'Configuración', adminOnly: true },
  { to: '/team', icon: <Users size={18} strokeWidth={1.6} />, label: 'Equipo' },
  { to: '/billing', icon: <CreditCard size={18} strokeWidth={1.6} />, label: 'Facturación', adminOnly: true },
  { to: '/help', icon: <HelpCircle size={18} strokeWidth={1.6} />, label: 'Ayuda' },
];

export default function Sidebar() {
  const { membership } = useOrganization();
  const isAdmin = membership?.role === 'org:admin';
  const visibleItems = NAV_ITEMS.filter((item) => !item.adminOnly || isAdmin);
  const {status} = useBilling();

  return (
    <nav className={styles.sidebar}>
      <div className={styles.header}>
        <Wordmark size={28} />
      </div>

      <div className={styles.nav}>
        {visibleItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              [styles.navItem, isActive ? styles.active : ''].filter(Boolean).join(' ')
            }
          >
            <span className={styles.navIcon} aria-hidden="true">
              {item.icon}
            </span>
            {item.label}
          </NavLink>
        ))}
      </div>

      <div className={styles.footer}>
        <div className={styles.planCard}>
          <div className={styles.planLabel}>Respuestas IA</div>
          {status && status.monthlyAiReplies === -1 ? (
            <div className={styles.planMeta}>
              {status.aiRepliesUsed} usadas · Ilimitadas
            </div>
          ) : status ? (
            <>
              <ProgressBar
                value={status.aiRepliesUsed}
                max={status.monthlyAiReplies}
                color="navy"
              />
              <div className={styles.planMeta}>
                {status.aiRepliesUsed} / {status.monthlyAiReplies} este mes
              </div>
            </>
          ) : (
            <div className={styles.planMeta}>—</div>
          )}
        </div>
      </div>
    </nav>
  );
}
