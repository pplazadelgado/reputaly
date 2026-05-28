import axios from 'axios';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

let interceptorId: number | null = null;

export function setupApiClient(getToken: () => Promise<string | null>) {
  // Si ya hay un interceptor registrado, lo eliminamos antes de añadir uno nuevo
  interceptorId = apiClient.interceptors.request.use(async (config) => {
    const token = await getToken();
    console.log('Token obtenido:', token);
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

  interceptorId = apiClient.interceptors.request.use(async (config) => {
    const token = await getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
}

export default apiClient;