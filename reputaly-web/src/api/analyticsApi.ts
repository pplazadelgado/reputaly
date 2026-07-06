import apiClient from "./apiClient"
import type { Analytics, AnalyticsFilters } from "../types/analytics"

// GET /analytics
export async function getAnalytics(filters: AnalyticsFilters= {}): Promise<Analytics> {
    const response = await apiClient.get<Analytics>('/analytics',{
        params:{
            from :filters.from,
            to: filters.to,
            locationId: filters.locationId,
        },
    });
    return response.data;
}
    
