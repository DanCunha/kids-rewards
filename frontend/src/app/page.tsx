"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { Award, CheckCircle2, Gift, History, Star } from "lucide-react";
import {
  completeActivity,
  getActivitiesByChild,
  getChild,
  getChildren,
  getHistoryByChild,
  getRewardsActive,
  redeemReward,
} from "@/lib/api";
import type { Activity, Child, HistoryItem, Reward } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

type Tab = "activities" | "rewards" | "history";

function isCurrentMonth(iso: string) {
  const d = new Date(iso);
  const now = new Date();
  return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth();
}

function formatHistoryDate(iso: string) {
  return new Date(iso).toLocaleString("pt-BR", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });
}

export default function KidsDashboard() {
  const [children, setChildren] = useState<Child[]>([]);
  const [childId, setChildId] = useState<string | null>(null);
  const [child, setChild] = useState<Child | null>(null);
  const [activities, setActivities] = useState<Activity[]>([]);
  const [rewards, setRewards] = useState<Reward[]>([]);
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [tab, setTab] = useState<Tab>("activities");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const refresh = useCallback(async (id: string) => {
    const [c, acts, rws, hist] = await Promise.all([
      getChild(id),
      getActivitiesByChild(id),
      getRewardsActive(),
      getHistoryByChild(id),
    ]);
    setChild(c);
    setActivities(acts);
    setRewards(rws);
    setHistory(hist);
  }, []);

  useEffect(() => {
    (async () => {
      try {
        const kids = await getChildren();
        setChildren(kids);
        if (kids.length > 0) {
          const firstId = kids[0].id;
          setChildId(firstId);
          await refresh(firstId);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erro ao carregar dados.");
      } finally {
        setLoading(false);
      }
    })();
  }, [refresh]);

  const handleChildChange = async (id: string) => {
    setChildId(id);
    setLoading(true);
    setError(null);
    try {
      await refresh(id);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar dados.");
    } finally {
      setLoading(false);
    }
  };

  const showToast = (message: string) => {
    setToast(message);
    setTimeout(() => setToast(null), 2500);
  };

  const handleComplete = async (activityId: string) => {
    setBusyId(activityId);
    setError(null);
    try {
      await completeActivity(activityId);
      if (childId) await refresh(childId);
      showToast("Tarefa concluída! +pontos 🎉");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Não foi possível concluir.");
    } finally {
      setBusyId(null);
    }
  };

  const handleRedeem = async (reward: Reward) => {
    if (!child || child.pointsBalance < reward.requiredPoints) {
      showToast("Pontos insuficientes para este prêmio!");
      return;
    }
    setBusyId(reward.id);
    setError(null);
    try {
      await redeemReward({ childId: child.id, rewardId: reward.id });
      if (childId) await refresh(childId);
      showToast(`Parabéns! Você resgatou: ${reward.name} 🎉`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Não foi possível resgatar.");
    } finally {
      setBusyId(null);
    }
  };

  const monthlyEarned = useMemo(
    () =>
      history
        .filter((h) => h.type === "Earned" && isCurrentMonth(h.createdAt))
        .reduce((acc, h) => acc + h.points, 0),
    [history]
  );

  const progress = child?.monthlyGoalPoints
    ? Math.min(100, Math.round((monthlyEarned / child.monthlyGoalPoints) * 100))
    : 0;

  if (loading) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-slate-50">
        <div className="flex items-center gap-2 text-sm font-medium text-slate-500">
          <Spinner /> Carregando seu painel...
        </div>
      </main>
    );
  }

  if (children.length === 0) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-slate-50 p-6">
        <p className="text-center text-sm font-medium text-slate-500">
          Nenhuma criança cadastrada. Peça a um responsável para cadastrar você! ⭐
        </p>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-slate-50 pb-20 text-slate-800">
      {/* Header */}
      <header className="sticky top-0 z-10 rounded-b-3xl bg-gradient-to-r from-amber-400 via-orange-400 to-yellow-500 p-4 shadow-md">
        <div className="mx-auto flex max-w-md items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white text-2xl shadow">
              👦
            </div>
            <div>
              <h1 className="text-xl font-extrabold text-white">Olá, {child?.name}!</h1>
              <p className="text-xs font-medium text-amber-100">
                {child?.age} anos • Campeão de Tarefas
              </p>
            </div>
          </div>
          <div className="flex items-center gap-1.5 rounded-2xl bg-white/95 px-3.5 py-2 shadow-inner">
            <Star className="h-6 w-6 animate-pulse fill-yellow-400 text-yellow-500" />
            <span className="text-xl font-black text-slate-900">{child?.pointsBalance}</span>
            <span className="text-xs font-bold text-slate-500">pts</span>
          </div>
        </div>

        {/* Meta mensal */}
        <div className="mx-auto mt-4 max-w-md rounded-xl bg-black/10 p-3 backdrop-blur-sm">
          <div className="mb-1.5 flex items-center justify-between text-xs font-bold text-white">
            <span className="flex items-center gap-1">
              <Award className="h-4 w-4" /> Meta Mensal
            </span>
            <span>
              {monthlyEarned} / {child?.monthlyGoalPoints} pts ({progress}%)
            </span>
          </div>
          <div className="h-3 w-full overflow-hidden rounded-full bg-white/30">
            <div
              className="h-full rounded-full bg-emerald-400 transition-all duration-500 ease-out"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>

        {/* Seletor de criança */}
        {children.length > 1 && (
          <div className="mx-auto mt-3 max-w-md">
            <select
              value={childId ?? ""}
              onChange={(e) => handleChildChange(e.target.value)}
              className="w-full rounded-xl border-none bg-white/95 px-3 py-2 text-xs font-bold text-slate-700 shadow outline-none"
            >
              {children.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
        )}
      </header>

      {/* Conteúdo */}
      <section className="mx-auto mt-4 max-w-md px-4">
        {error && (
          <div className="mb-4">
            <ErrorMessage message={error} />
          </div>
        )}

        {/* Abas */}
        <div className="mb-4 flex rounded-2xl bg-slate-200 p-1 font-bold text-slate-600 shadow-inner">
          {(
            [
              { id: "activities", label: "Tarefas", icon: CheckCircle2 },
              { id: "rewards", label: "Prêmios", icon: Gift },
              { id: "history", label: "Histórico", icon: History },
            ] as const
          ).map((item) => {
            const Icon = item.icon;
            return (
              <button
                key={item.id}
                onClick={() => setTab(item.id)}
                className={`flex flex-1 items-center justify-center gap-1.5 rounded-xl py-2.5 text-xs transition-all ${
                  tab === item.id ? "bg-white text-orange-600 shadow-sm" : ""
                }`}
              >
                <Icon className="h-4 w-4" /> {item.label}
              </button>
            );
          })}
        </div>

        {/* Tarefas */}
        {tab === "activities" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold uppercase tracking-wider text-slate-500">
              Suas Tarefas de Hoje
            </h2>
            {activities.length === 0 && (
              <p className="rounded-2xl border border-slate-200 bg-white p-6 text-center text-xs text-slate-500">
                Nenhuma tarefa por enquanto.
              </p>
            )}
            {activities.map((act) => (
              <div
                key={act.id}
                className={`flex items-center justify-between rounded-2xl border p-4 transition-all ${
                  act.isCompleted
                    ? "border-emerald-200 bg-emerald-50 opacity-75"
                    : "border-slate-200 bg-white shadow-sm hover:shadow-md"
                }`}
              >
                <div>
                  <h3
                    className={`font-bold ${
                      act.isCompleted ? "text-emerald-800 line-through" : "text-slate-800"
                    }`}
                  >
                    {act.title}
                  </h3>
                  {act.description && (
                    <p className="mt-0.5 text-xs text-slate-500">{act.description}</p>
                  )}
                  <span className="mt-2 inline-flex items-center gap-1 rounded-md border border-amber-200 bg-amber-50 px-2 py-0.5 text-xs font-black text-amber-600">
                    +{act.points} pts
                  </span>
                </div>

                {act.isCompleted ? (
                  <span className="inline-flex items-center gap-1 rounded-xl bg-emerald-100 px-3 py-1.5 text-xs font-bold text-emerald-600">
                    <CheckCircle2 className="h-4 w-4" /> Feito!
                  </span>
                ) : (
                  <button
                    onClick={() => handleComplete(act.id)}
                    disabled={busyId === act.id}
                    className="inline-flex h-12 items-center justify-center gap-1 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-600 px-4 font-bold text-white shadow-md transition-transform active:scale-95 disabled:opacity-60"
                  >
                    {busyId === act.id ? <Spinner className="h-4 w-4" /> : "Concluir"}
                  </button>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Prêmios */}
        {tab === "rewards" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold uppercase tracking-wider text-slate-500">
              Troque Seus Pontos
            </h2>
            {rewards.length === 0 && (
              <p className="rounded-2xl border border-slate-200 bg-white p-6 text-center text-xs text-slate-500">
                Nenhum prêmio disponível.
              </p>
            )}
            {rewards.map((reward) => {
              const canAfford = (child?.pointsBalance ?? 0) >= reward.requiredPoints;
              return (
                <div
                  key={reward.id}
                  className="flex items-center justify-between rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"
                >
                  <div>
                    <h3 className="font-bold text-slate-800">{reward.name}</h3>
                    <span className="mt-1 inline-flex items-center gap-1 rounded-md border border-purple-200 bg-purple-50 px-2 py-0.5 text-xs font-black text-purple-600">
                      <Gift className="h-3 w-3" /> {reward.requiredPoints} pts
                    </span>
                  </div>
                  <button
                    onClick={() => handleRedeem(reward)}
                    disabled={!canAfford || busyId === reward.id}
                    className={`h-11 rounded-xl px-4 text-xs font-bold shadow-md transition-all ${
                      canAfford
                        ? "bg-gradient-to-r from-purple-600 to-indigo-600 text-white active:scale-95"
                        : "cursor-not-allowed bg-slate-200 text-slate-400 shadow-none"
                    }`}
                  >
                    {busyId === reward.id ? <Spinner className="h-4 w-4" /> : canAfford ? "Resgatar" : "Faltam Pontos"}
                  </button>
                </div>
              );
            })}
          </div>
        )}

        {/* Histórico */}
        {tab === "history" && (
          <div className="space-y-3">
            <h2 className="text-sm font-bold uppercase tracking-wider text-slate-500">
              Extrato de Pontos
            </h2>
            {history.length === 0 && (
              <p className="rounded-2xl border border-slate-200 bg-white p-6 text-center text-xs text-slate-500">
                Nenhuma movimentação ainda.
              </p>
            )}
            {history.map((item) => (
              <div
                key={item.id}
                className="flex items-center justify-between rounded-xl border border-slate-200 bg-white p-3.5 text-xs"
              >
                <div>
                  <p className="font-bold text-slate-800">{item.description}</p>
                  <p className="mt-0.5 text-[10px] text-slate-400">
                    {formatHistoryDate(item.createdAt)}
                  </p>
                </div>
                <span
                  className={`rounded-lg px-2.5 py-1 font-black text-sm ${
                    item.type === "Earned"
                      ? "bg-emerald-100 text-emerald-700"
                      : "bg-rose-100 text-rose-700"
                  }`}
                >
                  {item.type === "Earned" ? `+${item.points}` : `-${item.points}`}
                </span>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Toast */}
      {toast && (
        <div className="fixed inset-x-0 bottom-6 z-30 flex justify-center px-4">
          <div className="animate-pulse rounded-2xl bg-slate-900 px-5 py-3 text-sm font-bold text-white shadow-lg">
            {toast}
          </div>
        </div>
      )}
    </main>
  );
}