import apiClient from './client';
import type { Comment } from '@/types/TopicInterfaces';
import type { RideShare } from '@/types/RideShareInterfaces';

export interface CreateRideShareData {
  from: string;
  to: string;
  departureTime: string;
  availableSeats: number;
  description?: string;
  stops?: string;
}

export interface UpdateRideShareData {
  id: string;
  from: string;
  to: string;
  departureTime: string;
  availableSeats: number;
  description?: string;
  stops?: string;
}

export interface RideShareCommentData {
  RideShareId: string;
  Content: string;
}

export interface ReservationData {
  RideShareId: string;
}

export const rideShareService = {
  async getMyRideShares(): Promise<RideShare[]> {
    const response = await apiClient.get<RideShare[]>('/api/rideshare/allOfLoggedIn');
    return response.data;
  },

  async getForeignRideShares(): Promise<RideShare[]> {
    const response = await apiClient.get<RideShare[]>('/api/rideshare/allExceptLoggedIn');
    return response.data;
  },

  async getRideShare(id: string): Promise<RideShare> {
    const response = await apiClient.get<RideShare>(`/api/rideshare/getone/${id}`);
    return response.data;
  },

  async createRideShare(data: CreateRideShareData): Promise<void> {
    await apiClient.post('/api/rideshare/addnew', data);
  },

  async updateRideShare(data: UpdateRideShareData): Promise<void> {
    await apiClient.put('/api/rideshare/update', data);
  },

  async deleteRideShare(id: string): Promise<void> {
    await apiClient.delete(`api/rideshare/delete/${id}`);
  },

  async cancelRideShare(id: string): Promise<void> {
    await apiClient.post(`api/rideshare/cancel/${id}`);
  },

  async uncancelRideShare(id: string): Promise<void> {
    await apiClient.post(`api/rideshare/uncancel/${id}`);
  },

  async getComments(rideShareId: string): Promise<Comment[]> {
    const response = await apiClient.get<Comment[]>(`/api/rideshare/comments?RideShareId=${rideShareId}`);
    return response.data;
  },

  async addComment(data: RideShareCommentData): Promise<Comment> {
    const response = await apiClient.post<Comment>('api/rideshare/CommentOnRideShare/', data);
    return response.data;
  },

  async addReservation(rideShareId: string): Promise<void> {
    await apiClient.post('/api/rideshare/addReservation', { RideShareId: rideShareId });
  },

  async removeReservation(rideShareId: string): Promise<void> {
    await apiClient.delete(`/api/rideshare/removeReservation/${rideShareId}`);
  },
};
