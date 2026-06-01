import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

let currentInterceptorId: number | null = null;

export function setupApiClient(getToken: () => Promise<string | null>) {
  if (currentInterceptorId !== null) {
    apiClient.interceptors.request.eject(currentInterceptorId);
  }
  currentInterceptorId = apiClient.interceptors.request.use(async (config) => {
    const token = await getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
}

export default apiClient;
