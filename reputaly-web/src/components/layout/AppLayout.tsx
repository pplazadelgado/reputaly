import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import styles from './AppLayout.module.css';

export default function AppLayout() {
  return (
    <div className={styles.layout}>
      <Sidebar />
      <div className={styles.main}>
        <Outlet />
      </div>
    </div>
  );
}
