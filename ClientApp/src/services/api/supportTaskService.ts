import apiClient from './client';
import type { SupportTask } from '@/types/SupportTaskInterfaces';

export interface HelpData {
  SupportTaskId: string;
}

export const supportTaskService = {
  async getAllTasks(): Promise<SupportTask[]> {
    const response = await apiClient.get<SupportTask[]>('/api/supporttask/getall');
    return response.data;
  },

  async addHelp(taskId: string): Promise<void> {
    await apiClient.post('/api/supporttask/help', { SupportTaskId: taskId });
  },

  async removeHelp(taskId: string): Promise<void> {
    await apiClient.delete(`/api/supporttask/help/${taskId}`);
  },
};
