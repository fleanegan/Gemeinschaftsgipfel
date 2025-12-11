import apiClient from './client';
import type { Comment } from '@/types/TopicInterfaces';
import type { ForumEntry } from '@/types/ForumInterfaces';

export interface CreateForumEntryData {
  title: string;
  content: string;
}

export interface UpdateForumEntryData {
  id: string;
  title: string;
  content: string;
}

export interface ForumCommentData {
  ForumEntryId: string;
  Content: string;
}

export const forumService = {
  async getAllForumEntries(): Promise<ForumEntry[]> {
    const response = await apiClient.get<ForumEntry[]>('/api/forum/all');
    return response.data.map((entry: any) => ({
      ...entry,
      expanded: false,
      comments: [],
      isLoading: false,
    }));
  },

  async getForumEntry(id: string): Promise<ForumEntry> {
    const response = await apiClient.get(`/api/forum/getone/${id}`);
    return {
      ...response.data,
      expanded: false,
      comments: [],
      isLoading: false,
    };
  },

  async createForumEntry(data: CreateForumEntryData): Promise<void> {
    await apiClient.post('/api/forum/addnew', data);
  },

  async updateForumEntry(data: UpdateForumEntryData): Promise<void> {
    await apiClient.put('/api/forum/update', data);
  },

  async deleteForumEntry(id: string): Promise<void> {
    await apiClient.delete(`/api/forum/delete/${id}`);
  },

  async getComments(forumEntryId: string): Promise<Comment[]> {
    const response = await apiClient.get<Comment[]>('/api/forum/comments', {
      params: { ForumEntryId: forumEntryId }
    });
    return response.data;
  },

  async addComment(data: ForumCommentData): Promise<Comment> {
    const response = await apiClient.post<Comment>('/api/forum/CommentOnForumEntry', data);
    return response.data;
  }
};
