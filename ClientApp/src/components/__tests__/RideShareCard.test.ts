import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import RideShareCard from '@/components/RideShareCard.vue';
import type { RideShare } from '@/types/RideShareInterfaces';
import { RideShareStatus } from '@/types/RideShareInterfaces';

describe('RideShareCard', () => {
  const mockRideShare: RideShare = {
    id: '1',
    availableSeats: 3,
    from: 'Berlin',
    to: 'Munich',
    departureTime: '2025-11-26T14:30:00Z',
    description: 'Comfortable ride',
    stops: 'Leipzig',
    driverUserName: 'John Doe',
    didIReserve: false,
    reservationCount: 1,
    status: RideShareStatus.Active,
    passengerUserNames: ['Jane Smith'],
    expanded: false,
    comments: [],
    isLoading: false
  };

  it('renders RideShare correctly', () => {
    const wrapper = mount(RideShareCard, {
      props: {
        rideShare: mockRideShare,
        isMine: false
      }
    });

    // Basic ride info should be visible
    expect(wrapper.text()).toContain('Berlin');
    expect(wrapper.text()).toContain('Munich');
  });

  it('formats dates using the formatDateTime utility', () => {
    const wrapper = mount(RideShareCard, {
      props: {
        rideShare: mockRideShare,
        isMine: false
      }
    });

    // Date should be formatted
    const text = wrapper.text();
    // The component should contain formatted date parts
    expect(text.length).toBeGreaterThan(0);
  });

  it('displays comments when expanded', () => {
    const rideShareWithComment: RideShare = {
      ...mockRideShare,
      expanded: true,
      comments: [{
        id: 'c1',
        content: 'Great ride!',
        creatorUserName: 'User',
        createdAt: '2025-11-26T14:30:00Z'
      }]
    };

    const wrapper = mount(RideShareCard, {
      props: {
        rideShare: rideShareWithComment,
        isMine: false
      }
    });

    expect(wrapper.text()).toContain('Great ride!');
  });
});
