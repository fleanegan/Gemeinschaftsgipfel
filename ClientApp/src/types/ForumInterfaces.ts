import type { Comment } from './TopicInterfaces';

export interface ForumEntry {
  id: string;
  title: string;
  content: string;
  creatorUserName: string;
  createdAt: string;
  expanded: boolean;
  comments: Comment[];
  isLoading: boolean;
}
