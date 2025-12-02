export interface Comment {
  id: string;
  content: string;
  creatorUserName: string;
  createdAt: string;
}

// TODO: Remove dummy value when backend provides category/material
export type TopicCategory = 'Workshop' | 'Vortrag' | 'Sport' | 'Diskussion' | 'Sonstiges';

export interface MyTopic {
  id: string;
  title: string;
  description: string;
  votes: number;
  presentationTimeInMinutes: number;
  category: TopicCategory;      // TODO: Remove dummy value when backend provides category
  material: string;             // TODO: Remove dummy value when backend provides material
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
  category: TopicCategory;      // TODO: Remove dummy value when backend provides category
  material: string;             // TODO: Remove dummy value when backend provides material
  didIVoteForThis: boolean;
  expanded: boolean;
  comments: Comment[];
  isLoading: boolean;
}
