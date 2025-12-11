import type { Comment } from './TopicInterfaces';

export enum RideShareStatus {
  Active,
  Canceled,
  Completed
}

export interface RideShare {
  id: string;
  availableSeats: number;
  from: string;
  to: string;
  departureTime: string;
  description?: string;
  stops?: string;
  driverUserName: string;
  didIReserve: boolean;
  reservationCount: number;
  status: RideShareStatus | string; // Can be enum number or string from backend
  passengerUserNames: string[];
  expanded: boolean;
  comments: Comment[];
  isLoading: boolean;
}
