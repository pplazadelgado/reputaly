import {createContext, useContext, useState, useEffect, useCallback} from 'react';
import type {ReactNode} from 'react';
import { getBillingStatus } from '../api/billingApi';
import type {BillingStatus} from '../types/billing';

// 1. La FORMA de lo que circula por la tubería.
//    No solo el dato, también el estado de carga y una función
//    para volver a pedirlo (refetch), que usaremos tras pagar.
interface BillingContextValue {
    status: BillingStatus | null;
    loading: boolean;
    refetch: () => Promise<void>;
}

// 2. La tubería en sí (vacía por defecto, se llena en el Provider).
const BillingContext = createContext<BillingContextValue | undefined>(undefined);

// 3. El DEPÓSITO: hace la llamada una vez, guarda el dato,
//    y lo provee a todos sus hijos.
export function BillingProvider({children}: {children: ReactNode}) {
    const [status, setStatus] = useState<BillingStatus | null>(null);
    const [loading, setLoading] = useState(true);

    // useCallback memoriza la función para no recrearla en cada render.
  // La explicación detallada va más abajo; por ahora, es la función
  // que pide el estado de billing al backend.
    const refetch = useCallback(async () => {
        setLoading(true);
        try{
            const data = await getBillingStatus();
            setStatus(data);
        }catch{
            // En el Provider no mostramos toast: cada componente decide
            // cómo reaccionar a status === null si le importa.
            setStatus(null);
        } finally{
            setLoading(false);
        }
    }, []);

    // Al montar el Provider (una vez), pedimos el estado.
    useEffect(() => {
        refetch();
    } , [refetch]);

    return (
        <BillingContext.Provider value={{status, loading, refetch}}>
            {children}
        </BillingContext.Provider>
    );
}
// 4. El GRIFO: hook que cualquier componente usa para leer el dato.
export function useBilling(): BillingContextValue {
  const context = useContext(BillingContext);
  if (context === undefined) {
    throw new Error('useBilling debe usarse dentro de un BillingProvider');
  }
  return context;
}
