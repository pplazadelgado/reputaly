import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import { BillingProvider } from '../../context/BillingContext';
import styles from './AppLayout.module.css';

export default function AppLayout() {
  return (
    <BillingProvider>
      <div className={styles.layout}>
        <Sidebar />
        <div className={styles.main}>
          <Outlet />
        </div>
      </div>
    </BillingProvider>
  );
}