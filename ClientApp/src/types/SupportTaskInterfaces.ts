export interface SupportTask {
  id: string;
  title: string;
  description: string;
  supporterUserNames: string[];
  duration: string;
  requiredSupporters: number;
  showSupporter: boolean;
}
