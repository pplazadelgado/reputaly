import { BrowserRouter,Routes,Route,Navigate } from "react-router-dom";
import { SignedIn, SignedOut, RedirectToSignIn } from "@clerk/clerk-react";
import AppLayout from "../components/layout/AppLayout";
import Dashboard from "../Pages/DashBoard";
import Settings from "../Pages/Settings";
import NotFound from "../Pages/NotFound";
import type React from "react";

// ProtectedRoute: si hay sesion se muestra el contenido,
// si no hay sesion redirige al login de Clerk
function ProtectedRoute({children} : {children : React.ReactNode}){
    return(
        <>
            <SignedIn>{children}</SignedIn>
            <SignedOut><RedirectToSignIn/></SignedOut>
        </>
    );
}

export default function AppRouter(){
    return(
        <BrowserRouter>
            <Routes>
                {/* Ruta raíz: redirige a /dashboard */}
                <Route path="/" element={<Navigate to="/dashboard" replace />} />

                {/* Rutas protegidas: todas dentro del layout */}
                <Route
                element={
                    <ProtectedRoute>
                    <AppLayout />
                    </ProtectedRoute>
                }
                >
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/settings" element={<Settings />} />
                </Route>

                <Route path="*" element={<NotFound />} />
            </Routes>
        </BrowserRouter>
    );
}