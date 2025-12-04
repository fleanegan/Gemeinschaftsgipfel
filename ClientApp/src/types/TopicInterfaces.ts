export interface Comment {
  id: string;
  content: string;
  creatorUserName: string;
  createdAt: string;
}

export type TopicCategory = 'Workshop' | 'Vortrag' | 'Sport' | 'Diskussion' | 'Sonstiges';

export interface MyTopic {
  id: string;
  title: string;
  description: string;
  votes: number;
  presentationTimeInMinutes: number;
  category: TopicCategory;
  material: string;
  expanded: boolean;
  comments: Comment[];
  isLoading: boolean;
}

export interface ForeignTopic {
  id: string;
  title: string;
  description: string;
  presenterUserName: string;
  presentationTimeInMinutes: number;
  category: TopicCategory;
  material: string;
  didIVoteForThis: boolean;
  expanded: boolean;
  comments: Comment[];
  isLoading: boolean;
}
