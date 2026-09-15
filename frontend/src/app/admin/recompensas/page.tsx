"use client";

import { useCallback, useEffect, useState } from "react";
import { Gift, PlusCircle } from "lucide-react";
import { createReward, getRewardsAll, setRewardActive } from "@/lib/api";
import type { Reward } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

export default function AdminRewards() {
  const [rewards, setRewards] = useState<Reward[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [requiredPoints, setRequiredPoints] = useState(50);
  const [submitting, setSubmitting] = useState(false);

  const refresh = useCallback(async () => {
    try {
      setRewards(await getRewardsAll());
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar recompensas.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setFeedback(null);
    setError(null);
    try {
      const reward = await createReward({ name, requiredPoints });
      setRewards((prev) => [...prev, reward]);
      setName("");
      setRequiredPoints(50);
      setFeedback(`Recompensa "${reward.name}" cadastrada!`);
    } catch (err) {
      setFeedback(null);
      setError(err instanceof Error ? err.message : "Erro ao cadastrar recompensa.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggle = async (reward: Reward) => {
    setFeedback(null);
    setError(null);
    try {
      await setRewardActive(reward.id, !reward.isActive);
      setRewards((prev) =>
        prev.map((r) => (r.id === reward.id ? { ...r, isActive: !r.isActive } : r))
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao alterar status.");
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center gap-2 py-24 text-sm font-medium text-brand-muted">
        <Spinner /> Carregando...
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <header>
        <h1 className="text-2xl font-bold text-slate-800">Recompensas</h1>
        <p className="text-xs text-brand-muted">Catálogo de prêmios para a troca de pontos.</p>
      </header>

      {error && <ErrorMessage message={error} />}
      {feedback && (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
          {feedback}
        </div>
      )}

      {/* Formulário */}
      <form
        onSubmit={handleCreate}
        className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm"
      >
        <h2 className="mb-4 text-sm font-bold uppercase tracking-wider text-slate-500">
          Nova recompensa
        </h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">Nome</label>
            <input
              required
              minLength={2}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ex.: 30 min de Video Game"
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">
              Pontos necessários
            </label>
            <input
              type="number"
              min={1}
              value={requiredPoints}
              onChange={(e) => setRequiredPoints(Number(e.target.value))}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar"
            />
          </div>
        </div>
        <button
          type="submit"
          disabled={submitting}
          className="mt-4 inline-flex items-center gap-2 rounded-xl bg-purple-600 px-5 py-2.5 text-sm font-bold text-white transition-colors hover:bg-purple-700 disabled:opacity-60"
        >
          <PlusCircle className="h-4 w-4" /> {submitting ? "Salvando..." : "Cadastrar"}
        </button>
      </form>

      {/* Lista */}
      <div className="space-y-3">
        {rewards.length === 0 ? (
          <p className="py-8 text-center text-sm text-brand-muted">Nenhuma recompensa cadastrada.</p>
        ) : (
          rewards.map((reward) => (
            <div
              key={reward.id}
              className={`flex items-center justify-between rounded-2xl border p-4 shadow-sm ${
                reward.isActive ? "border-slate-100 bg-white" : "border-slate-200 bg-slate-50 opacity-75"
              }`}
            >
              <div>
                <p className="font-bold text-slate-800">{reward.name}</p>
                <span className="mt-1 inline-flex items-center gap-1 rounded-md border border-purple-200 bg-purple-50 px-2 py-0.5 text-xs font-black text-purple-600">
                  <Gift className="h-3 w-3" /> {reward.requiredPoints} pts
                </span>
              </div>
              <div className="flex items-center gap-3">
                <span
                  className={`rounded-full px-2.5 py-0.5 text-[10px] font-bold ${
                    reward.isActive ? "bg-emerald-100 text-emerald-700" : "bg-slate-200 text-slate-500"
                  }`}
                >
                  {reward.isActive ? "Ativa" : "Inativa"}
                </span>
                <button
                  onClick={() => handleToggle(reward)}
                  className={`rounded-xl px-3 py-2 text-xs font-bold transition-colors ${
                    reward.isActive
                      ? "bg-rose-500 text-white hover:bg-rose-600"
                      : "bg-emerald-600 text-white hover:bg-emerald-700"
                  }`}
                >
                  {reward.isActive ? "Desativar" : "Ativar"}
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}