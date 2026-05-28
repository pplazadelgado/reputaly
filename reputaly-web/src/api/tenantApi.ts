import apiClient from "./apiClient";
import type { Tenant,TenantSettings } from "../types/tenant";

//Get /tenants/me
export async function getTenant ():Promise<Tenant> {
    const response = await apiClient.get<Tenant>('/tenants/me');
    return response.data;
}

//Put /tenants/me
export async function updateTenant(name:string):Promise<Tenant> {
    const response = await apiClient.put<Tenant>('/tenants/me', {name});
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