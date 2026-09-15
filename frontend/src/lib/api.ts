import type {
  Activity,
  Child,
  CreateActivityInput,
  CreateChildInput,
  CreateRewardInput,
  HistoryItem,
  RedeemRewardInput,
  Reward,
  UpdateGoalInput,
} from "./types";

function resolveBaseUrl(): string {
  if (process.env.NEXT_PUBLIC_API_URL) {
    return process.env.NEXT_PUBLIC_API_URL;
  }
  if (typeof window !== "undefined" && window.location.hostname !== "localhost") {
    return `${window.location.protocol}//${window.location.hostname}:5183/api`;
  }
  return "http://localhost:5183/api";
}

const BASE_URL = resolveBaseUrl();

export class ApiError extends Error {}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      ...init,
      headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
      cache: "no-store",
    });
  } catch {
    throw new ApiError("Não foi possível conectar ao servidor.");
  }

  if (!res.ok) {
    let message = `Erro ${res.status}`;
    try {
      const body = await res.json();
      if (body?.message) message = body.message;
      else if (body?.title) message = body.title;
    } catch {
      // corpo não-JSON: mantém mensagem genérica
    }
    throw new ApiError(message);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

// Children
export const getChildren = () => request<Child[]>("/children");
export const getChild = (id: string) => request<Child>(`/children/${id}`);
export const createChild = (input: CreateChildInput) =>
  request<Child>("/children", { method: "POST", body: JSON.stringify(input) });
export const updateChildGoal = (id: string, input: UpdateGoalInput) =>
  request<void>(`/children/${id}/goal`, { method: "PUT", body: JSON.stringify(input) });

// Activities
export const getActivitiesByChild = (childId: string) =>
  request<Activity[]>(`/activities/child/${childId}`);
export const createActivity = (input: CreateActivityInput) =>
  request<Activity>("/activities", { method: "POST", body: JSON.stringify(input) });
export const completeActivity = (id: string) =>
  request<void>(`/activities/${id}/complete`, { method: "PATCH" });

// Rewards
export const getRewardsActive = () => request<Reward[]>("/rewards");
export const getRewardsAll = () => request<Reward[]>("/rewards/all");
export const createReward = (input: CreateRewardInput) =>
  request<Reward>("/rewards", { method: "POST", body: JSON.stringify(input) });
export const redeemReward = (input: RedeemRewardInput) =>
  request<void>("/rewards/redeem", { method: "POST", body: JSON.stringify(input) });
export const setRewardActive = (id: string, isActive: boolean) =>
  request<void>(`/rewards/${id}`, { method: "PATCH", body: JSON.stringify({ isActive }) });

// PointsHistory
export const getHistoryByChild = (childId: string) =>
  request<HistoryItem[]>(`/pointshistory/child/${childId}`);