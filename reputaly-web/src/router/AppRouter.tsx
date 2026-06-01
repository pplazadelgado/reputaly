import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { SignedIn, SignedOut, RedirectToSignIn } from '@clerk/clerk-react';
import type React from 'react';
import AppLayout from '../components/layout/AppLayout';
import Dashboard from '../Pages/DashBoard';
import Settings from '../Pages/Settings';
import NotFound from '../Pages/NotFound';
import ComingSoon from '../Pages/ComingSoon';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  return (
    <>
      <SignedIn>{children}</SignedIn>
      <SignedOut>
        <RedirectToSignIn />
      </SignedOut>
    </>
  );
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />

        <Route
          element={
            <ProtectedRoute>
              <AppLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/settings" element={<Settings />} />
          <Route path="/reviews" element={<ComingSoon title="Reseñas" subtitle="Gestiona y responde a las reseñas de tus clientes." />} />
          <Route path="/team" element={<ComingSoon title="Equipo" subtitle="Invita a tu equipo y gestiona permisos." />} />
          <Route path="/billing" element={<ComingSoon title="Facturación" subtitle="Gestiona tu suscripción y métodos de pago." />} />
          <Route path="/help" element={<ComingSoon title="Ayuda" subtitle="Documentación, tutoriales y soporte." />} />
        </Route>

        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}
