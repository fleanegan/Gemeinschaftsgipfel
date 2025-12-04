import apiClient from './client';
import type { Comment, MyTopic, ForeignTopic, TopicCategory } from '@/types/TopicInterfaces';

export interface CreateTopicData {
  title: string;
  description: string;
  presentationTimeInMinutes: number;
  category: TopicCategory | string;
  material: string;
}

export interface UpdateTopicData {
  id: string;
  title: string;
  description: string;
  presentationTimeInMinutes: number;
  category: TopicCategory | string;
  material: string;
}

export interface CommentData {
  TopicId: string;
  Content: string;
}

export const topicService = {
  async getMyTopics(): Promise<MyTopic[]> {
    const response = await apiClient.get<MyTopic[]>('/api/topic/allOfLoggedIn');
    return response.data.map((topic: any) => ({
      ...topic,
      expanded: false,
      comments: [],
      isLoading: false,
    }));
  },

  async getForeignTopics(): Promise<ForeignTopic[]> {
    const response = await apiClient.get<ForeignTopic[]>('/api/topic/allExceptLoggedIn');
    return response.data.map((topic: any) => ({
      ...topic,
      expanded: false,
      comments: [],
      isLoading: false,
    }));
  },

  async getTopic(id: string): Promise<MyTopic | ForeignTopic> {
    const response = await apiClient.get(`/api/topic/getone/${id}`);
    return response.data;
  },

  async createTopic(data: CreateTopicData): Promise<void> {
    await apiClient.post('/api/topic/addnew', data);
  },

  async updateTopic(data: UpdateTopicData): Promise<void> {
    await apiClient.put('/api/topic/update', data);
  },

  async deleteTopic(id: string): Promise<void> {
    await apiClient.delete(`api/topic/delete/${id}`);
  },

  async getComments(topicId: string): Promise<Comment[]> {
    const response = await apiClient.get<Comment[]>(`/api/topic/comments?TopicId=${topicId}`);
    return response.data;
  },

  async addComment(data: CommentData): Promise<Comment> {
    const response = await apiClient.post<Comment>('api/topic/CommentOnTopic/', data);
    return response.data;
  },

  async addVote(topicId: string): Promise<void> {
    await apiClient.post('/api/topic/addVote', { TopicId: topicId });
  },

  async removeVote(topicId: string): Promise<void> {
    await apiClient.delete(`/api/topic/removeVote/${topicId}`);
  },

  async getLegalPresentationDurations(): Promise<number[]> {
    const response = await apiClient.get<number[]>('/api/topic/GetLegalPresentationDurations');
    return response.data;
  },
};
