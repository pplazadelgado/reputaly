import { NavLink } from 'react-router-dom';
import { OrganizationSwitcher } from '@clerk/clerk-react';
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

interface NavItem {
  to: string;
  icon: React.ReactNode;
  label: string;
  badge?: number;
}

const NAV_ITEMS: NavItem[] = [
  { to: '/dashboard', icon: <LayoutGrid size={18} strokeWidth={1.6} />, label: 'Panel' },
  { to: '/reviews', icon: <Star size={18} strokeWidth={1.6} />, label: 'Reseñas', badge: 8 },
  { to: '/settings', icon: <Settings size={18} strokeWidth={1.6} />, label: 'Configuración' },
  { to: '/team', icon: <Users size={18} strokeWidth={1.6} />, label: 'Equipo' },
  { to: '/billing', icon: <CreditCard size={18} strokeWidth={1.6} />, label: 'Facturación' },
  { to: '/help', icon: <HelpCircle size={18} strokeWidth={1.6} />, label: 'Ayuda' },
];

export default function Sidebar() {
  return (
    <nav className={styles.sidebar}>
      <div className={styles.header}>
        <Wordmark size={28} />
      </div>

      <div className={styles.orgSwitcher}>
        <p className={styles.orgLabel}>Negocio</p>
        <OrganizationSwitcher
          appearance={{
            elements: {
              rootBox: { width: '100%' },
              organizationSwitcherTrigger: {
                width: '100%',
                borderRadius: 'var(--radius)',
                border: '1px solid var(--slate-200)',
                padding: '6px 10px',
                fontSize: '13px',
                color: 'var(--slate-700)',
              },
            },
          }}
        />
      </div>

      <div className={styles.nav}>
        {NAV_ITEMS.map((item) => (
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
            {item.badge !== undefined && (
              <span className={styles.badge} aria-label={`${item.badge} pendientes`}>
                {item.badge}
              </span>
            )}
          </NavLink>
        ))}
      </div>

      <div className={styles.footer}>
        <div className={styles.planCard}>
          <div className={styles.planLabel}>Respuestas IA</div>
          <ProgressBar value={189} max={500} color="navy" />
          <div className={styles.planMeta}>189 / 500 este mes</div>
        </div>
      </div>
    </nav>
  );
}
