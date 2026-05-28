import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ClerkProvider, useAuth } from '@clerk/clerk-react';
import App from './App';
import { setupApiClient } from './api/apiClient';
import './index.css';

const publishableKey = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY;

if (!publishableKey) {
  throw new Error('Falta VITE_CLERK_PUBLISHABLE_KEY en .env.local');
}

// Componente intermedio que configura axios con el token de Clerk
// Necesita estar dentro de ClerkProvider para poder usar useAuth
function AppWithAuth() {
  const { getToken } = useAuth();

  // Configuramos el interceptor con la función getToken de Clerk
  // El template 'reputaly-backend' incluye el org_id que necesita el backend
  setupApiClient(() => getToken({ template: 'reputaly-backend' }));

  return <App />;
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ClerkProvider publishableKey={publishableKey}>
      <AppWithAuth />
    </ClerkProvider>
  </StrictMode>
);