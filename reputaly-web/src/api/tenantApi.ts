import apiClient from "./apiClient";
import type { Tenant,TenantSettings } from "../types/tenant";

//Get /tenants/me
export async function getTenant ():Promise<Tenant> {
    const response = await apiClient.get<Tenant>('/tenants/me');
    return response.data;
}

//Put /tenants/me
export async function updateTenant(name: string, vertical: string | null): Promise<Tenant> {
    const response = await apiClient.put<Tenant>('/tenants/me', { name, vertical });
    return response.data;
}

//GET /tenants/me/settings
export async function getSettings():Promise<TenantSettings> {
    const response = await apiClient.get<TenantSettings>('/tenants/me/settings');
    return response.data;
}

// PUT /tenants/me/settings
export async function updateSettings(settings:TenantSettings): Promise<TenantSettings> {
    const response = await apiClient.put<TenantSettings>('/tenants/me/settings', settings);
    return response.data;
}

// GET /locations
export interface Location {
  id: string;
  name: string;
  googleLocationId: string | null;
  googleAccountEmail: string | null;
  isGoogleConnected: boolean;
  isActive: boolean;
}

export async function getLocations(): Promise<Location[]> {
  const response = await apiClient.get<Location[]>('/locations');
  return response.data;
}