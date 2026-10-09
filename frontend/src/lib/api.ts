import type {
  Activity,
  Child,
  CreateActivityInput,
  CreateChildInput,
  CreateRewardInput,
  ForgotPasswordInput,
  HistoryItem,
  LoginInput,
  RedeemRewardInput,
  RegisterInput,
  ResetPasswordInput,
  Reward,
  UpdateGoalInput,
  User,
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

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      ...init,
      headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
      credentials: "include",
      cache: "no-store",
    });
  } catch {
    throw new ApiError("Não foi possível conectar ao servidor.", 0);
  }

  if (!res.ok) {
    if (res.status === 401 && !path.startsWith("/auth/") && typeof window !== "undefined") {
      window.dispatchEvent(new Event("auth:unauthorized"));
    }

    let message = `Erro ${res.status}`;
    try {
      const body = await res.json();
      if (body?.message) message = body.message;
      else if (body?.title) message = body.title;
    } catch {
      // corpo não-JSON: mantém mensagem genérica
    }
    throw new ApiError(message, res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

// Auth
export const login = (input: LoginInput) =>
  request<User>("/auth/login", { method: "POST", body: JSON.stringify(input) });
export const register = (input: RegisterInput) =>
  request<User>("/auth/register", { method: "POST", body: JSON.stringify(input) });
export const logout = () => request<void>("/auth/logout", { method: "POST" });
export const forgotPassword = (input: ForgotPasswordInput) =>
  request<{ message: string }>("/auth/forgot-password", { method: "POST", body: JSON.stringify(input) });
export const resetPassword = (input: ResetPasswordInput) =>
  request<{ message: string }>("/auth/reset-password", { method: "POST", body: JSON.stringify(input) });

export const getMe = async (): Promise<User | null> => {
  try {
    return await request<User>("/auth/me");
  } catch (err) {
    if (err instanceof ApiError && err.status === 401) return null;
    throw err;
  }
};

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
