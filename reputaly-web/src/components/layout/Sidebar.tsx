import { NavLink } from "react-router-dom";
import { OrganizationSwitcher } from '@clerk/clerk-react';

// NavLink es como una <a> normal pero añade una clase CSS cuando
// la ruta esta activa. Util para resaltar el menu actual
export default function Sidebar(){
    return(
        <nav style={{
      width: '240px',
      minHeight: '100vh',
      backgroundColor: '#1e293b',
      padding: '24px 16px',
      display: 'flex',
      flexDirection: 'column',
      gap: '8px'
    }}>
      <div style={{ color: '#f1f5f9', fontWeight: 700, fontSize: '20px', marginBottom: '32px' }}>
        Reputaly
      </div>

      <NavLink to="/dashboard" style={navStyle}>
        <div style={{ marginBottom: '24px' }}>
          <OrganizationSwitcher />
        </div>
        Dashboard
      </NavLink>

      <NavLink to="/settings" style={navStyle}>
        Configuración
      </NavLink>
    </nav>
    );
}

// Fucion que devuelve estilos segun si el enlace esta activo
function navStyle({isActive} : {isActive: boolean}){
    return{
        color: isActive ? '#ffffff' : '#94a3b8',
    textDecoration: 'none',
    padding: '10px 12px',
    borderRadius: '6px',
    backgroundColor: isActive ? '#334155' : 'transparent',
    fontWeight: isActive ? 600 : 400,
    };
}