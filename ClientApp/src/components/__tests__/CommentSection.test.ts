import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import CommentSection from '../CommentSection.vue';
import type { Comment } from '@/types/TopicInterfaces';
import axios from 'axios';

vi.mock('axios');

describe('CommentSection', () => {
  const mockComments: Comment[] = [
    {
      id: '1',
      content: 'First comment',
      creatorUserName: 'User1',
      createdAt: '2025-01-01T10:00:00'
    },
    {
      id: '2',
      content: 'Second comment',
      creatorUserName: 'User2',
      createdAt: '2025-01-01T11:00:00'
    }
  ];

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders all comments', () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    expect(wrapper.findAll('.comment-item')).toHaveLength(2);
    expect(wrapper.text()).toContain('User1');
    expect(wrapper.text()).toContain('First comment');
    expect(wrapper.text()).toContain('User2');
    expect(wrapper.text()).toContain('Second comment');
  });

  it('renders empty comments list', () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: [],
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    expect(wrapper.findAll('.comment-item')).toHaveLength(0);
    expect(wrapper.find('.comments-title').text()).toBe('Kommentare:');
  });

  it('formats comment timestamps correctly', () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    const timestamps = wrapper.findAll('.comment-timestamp');
    expect(timestamps[0].text()).toMatch(/\d{1,2}\.\s\w+\s\d{4}/); // German date format
  });

  it('renders input field and send button', () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    expect(wrapper.find('.comment-input').exists()).toBe(true);
    expect(wrapper.find('.comment-send-button').exists()).toBe(true);
    expect(wrapper.find('.comment-send-button').text()).toBe('Senden');
  });

  it('updates input field when typing', async () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    const input = wrapper.find('.comment-input');
    await input.setValue('New comment text');
    
    expect((input.element as HTMLInputElement).value).toBe('New comment text');
  });

  it('sends comment with correct topic endpoint and data', async () => {
    const mockResponse = { data: { id: '3', content: 'New comment', creatorUserName: 'User3', createdAt: '2025-01-01T12:00:00' } };
    vi.mocked(axios.post).mockResolvedValue(mockResponse);

    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'topic-123',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    await wrapper.find('.comment-input').setValue('New comment');
    await wrapper.find('.comment-send-button').trigger('click');

    expect(axios.post).toHaveBeenCalledWith('api/topic/CommentOnTopic/', {
      TopicId: 'topic-123',
      Content: 'New comment'
    });
  });

  it('sends comment with correct rideshare endpoint and data', async () => {
    const mockResponse = { data: { id: '3', content: 'New comment', creatorUserName: 'User3', createdAt: '2025-01-01T12:00:00' } };
    vi.mocked(axios.post).mockResolvedValue(mockResponse);

    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'ride-456',
        apiEndpoint: 'api/rideshare/CommentOnRideShare/'
      }
    });

    await wrapper.find('.comment-input').setValue('New rideshare comment');
    await wrapper.find('.comment-send-button').trigger('click');

    expect(axios.post).toHaveBeenCalledWith('api/rideshare/CommentOnRideShare/', {
      RideShareId: 'ride-456',
      Content: 'New rideshare comment'
    });
  });

  it('emits comment-sent event with correct payload', async () => {
    const mockResponse = { data: { id: '3', content: 'New comment', creatorUserName: 'User3', createdAt: '2025-01-01T12:00:00' } };
    vi.mocked(axios.post).mockResolvedValue(mockResponse);

    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    await wrapper.find('.comment-input').setValue('New comment');
    await wrapper.find('.comment-send-button').trigger('click');
    await wrapper.vm.$nextTick();

    expect(wrapper.emitted('comment-sent')).toBeTruthy();
    expect(wrapper.emitted('comment-sent')![0]).toEqual([{
      itemId: 'test-id',
      comment: mockResponse.data,
      content: 'New comment'
    }]);
  });

  it('clears input after sending comment', async () => {
    const mockResponse = { data: { id: '3', content: 'New comment', creatorUserName: 'User3', createdAt: '2025-01-01T12:00:00' } };
    vi.mocked(axios.post).mockResolvedValue(mockResponse);

    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    const input = wrapper.find('.comment-input');
    await input.setValue('New comment');
    await wrapper.find('.comment-send-button').trigger('click');
    await wrapper.vm.$nextTick();

    expect((input.element as HTMLInputElement).value).toBe('');
  });

  it('does not send empty comments', async () => {
    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    await wrapper.find('.comment-send-button').trigger('click');

    expect(axios.post).not.toHaveBeenCalled();
    expect(wrapper.emitted('comment-sent')).toBeFalsy();
  });

  it('handles API errors gracefully', async () => {
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.mocked(axios.post).mockRejectedValue(new Error('Network error'));

    const wrapper = mount(CommentSection, {
      props: {
        comments: mockComments,
        itemId: 'test-id',
        apiEndpoint: 'api/topic/CommentOnTopic/'
      }
    });

    await wrapper.find('.comment-input').setValue('New comment');
    await wrapper.find('.comment-send-button').trigger('click');
    await wrapper.vm.$nextTick();

    expect(consoleErrorSpy).toHaveBeenCalled();
    expect(wrapper.emitted('comment-sent')).toBeFalsy();
    
    consoleErrorSpy.mockRestore();
  });
});
