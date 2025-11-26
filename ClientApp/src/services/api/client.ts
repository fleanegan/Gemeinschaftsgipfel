import axios, { AxiosInstance } from 'axios';
import { useAuthStore } from '@/store/auth';
import { useDataStore } from '@/store/data';
import router from '@/router';

const apiClient: AxiosInstance = axios.create();

apiClient.interceptors.request.use(
  (config) => {
    const token = useAuthStore().token;
    useDataStore().startLoading();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    useDataStore().stopLoading();
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  async (response) => {
    useDataStore().stopLoading();
    return response;
  },
  (error) => {
    useDataStore().stopLoading();
    const isComingFromLogin = error.request.responseURL.includes('auth/login');
    if (error.response && error.response.status === 401 && !isComingFromLogin) {
      router.push('/login');
    } else {
      return Promise.reject(error);
    }
  }
);

export default apiClient;
