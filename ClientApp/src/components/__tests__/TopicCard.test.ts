import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import TopicCard from '@/components/TopicCard.vue';
import type { MyTopic, ForeignTopic } from '@/types/TopicInterfaces';

describe('TopicCard', () => {
  const mockMyTopic: MyTopic = {
    id: '1',
    title: 'Test Topic',
    description: 'Test Description',
    votes: 5,
    presentationTimeInMinutes: 15,
    expanded: false,
    comments: [],
    isLoading: false
  };

  const mockForeignTopic: ForeignTopic = {
    id: '2',
    title: 'Foreign Topic',
    description: 'Foreign Description',
    presenterUserName: 'John Doe',
    presentationTimeInMinutes: 20,
    didIVoteForThis: false,
    expanded: false,
    comments: [],
    isLoading: false
  };

  it('renders MyTopic correctly', () => {
    const wrapper = mount(TopicCard, {
      props: {
        topic: mockMyTopic,
        isMine: true
      }
    });

    // Topic title should always be visible
    expect(wrapper.text()).toContain('Test Topic');
  });

  it('renders ForeignTopic correctly', () => {
    const wrapper = mount(TopicCard, {
      props: {
        topic: mockForeignTopic,
        isMine: false
      }
    });

    // Topic title should always be visible
    expect(wrapper.text()).toContain('Foreign Topic');
  });

  it('formats dates using the formatDateTime utility', () => {
    const topicWithComment: MyTopic = {
      ...mockMyTopic,
      expanded: true,
      comments: [{
        id: 'c1',
        content: 'Test comment',
        creatorUserName: 'User',
        createdAt: '2025-11-26T14:30:00Z'
      }]
    };

    const wrapper = mount(TopicCard, {
      props: {
        topic: topicWithComment,
        isMine: true
      }
    });

    // Comment should be rendered with formatted date
    expect(wrapper.text()).toContain('Test comment');
  });
});
