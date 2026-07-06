import { Routes, Route, Navigate } from 'react-router-dom';
import { SignedIn, SignedOut, useOrganization } from '@clerk/clerk-react';
import type React from 'react';
import AppLayout from '../components/layout/AppLayout';
import Dashboard from '../Pages/DashBoard';
import Settings from '../Pages/Settings';
import NotFound from '../Pages/NotFound';
import ComingSoon from '../Pages/ComingSoon';
import Reviews from '../Pages/Reviews';
import Billing from '../Pages/Billing';
import Team from '../Pages/Team';
import SignInPage from '../Pages/SignIn';
import SignUpPage from '../Pages/SignUp';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  return (
    <>
      <SignedIn>{children}</SignedIn>
      <SignedOut>
          <Navigate to="/sign-in" replace />
      </SignedOut>
    </>
  );
}

function AdminRoute({ children }: {children : React.ReactNode}){
  const { membership, isLoaded} = useOrganization();

  if (!isLoaded) return null; // aun cargando la membership
  if(membership?.role !== 'org:admin') {
    return <Navigate to ="/dashboard" replace />
  }
  return <>{children}</>
}

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/sign-in/*" element={<SignInPage />} />
      <Route path="/sign-up/*" element={<SignUpPage />} />

      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/settings" element={<AdminRoute><Settings /></AdminRoute>} />
        <Route path="/reviews" element={<Reviews />} />
        <Route path="/team" element={<Team />} />
        <Route path="/billing" element={<Billing />} />
        <Route path="/help" element={<ComingSoon title="Ayuda" subtitle="Documentación, tutoriales y soporte." />} />
      </Route>

      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
