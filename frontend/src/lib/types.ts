export interface Child {
  id: string;
  name: string;
  age: number;
  pointsBalance: number;
  monthlyGoalPoints: number;
  monthlyGoalRewardId: string | null;
  createdAt: string;
}

export interface Activity {
  id: string;
  childId: string;
  title: string;
  description: string;
  points: number;
  isCompleted: boolean;
  completedAt: string | null;
  createdAt: string;
}

export interface Reward {
  id: string;
  name: string;
  requiredPoints: number;
  isActive: boolean;
  createdAt: string;
}

export type PointType = "Earned" | "Spent";

export interface HistoryItem {
  id: string;
  childId: string;
  type: PointType;
  points: number;
  referenceId: string;
  description: string;
  createdAt: string;
}

export interface CreateChildInput {
  name: string;
  age: number;
  monthlyGoalPoints?: number;
  monthlyGoalRewardId?: string | null;
}

export interface UpdateGoalInput {
  monthlyGoalPoints: number;
  monthlyGoalRewardId?: string | null;
}

export interface CreateActivityInput {
  childId: string;
  title: string;
  description?: string;
  points: number;
}

export interface CreateRewardInput {
  name: string;
  requiredPoints: number;
}

export interface RedeemRewardInput {
  childId: string;
  rewardId: string;
}