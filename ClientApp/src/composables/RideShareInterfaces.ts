export enum RideShareStatus {
  Active,
  Canceled,
  Completed
}

export interface RideShareComment {
  id: string;
  content: string;
  creatorUserName: string;
  createdAt: string;
}

export interface RideShare {
  id: string;
  title: string;
  availableSeats: number;
  from: string;
  to: string;
  departureTime: string;
  description?: string;
  stops?: string;
  driverUserName: string;
  didIReserve: boolean;
  reservationCount: number;
  status: RideShareStatus;
  passengerUserNames: string[];
  expanded: boolean;
  comments: RideShareComment[];
  isLoading: boolean;
}
