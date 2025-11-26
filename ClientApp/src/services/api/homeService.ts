import apiClient from './client';

export const homeService = {
  async getInfo(): Promise<string> {
    const response = await apiClient.get<string>('/api/home/getinfo');
    return response.data;
  },

  async getImpressum(): Promise<string> {
    const response = await apiClient.get<string>('/api/home/getimpressum');
    return response.data;
  },
};
