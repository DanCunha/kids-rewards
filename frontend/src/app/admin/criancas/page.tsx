"use client";

import { useCallback, useEffect, useState } from "react";
import { PlusCircle, Save, Star } from "lucide-react";
import { createChild, getChildren, updateChildGoal } from "@/lib/api";
import type { Child } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

export default function AdminChildren() {
  const [children, setChildren] = useState<Child[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [age, setAge] = useState(10);
  const [goal, setGoal] = useState(200);
  const [submitting, setSubmitting] = useState(false);

  const [goalEdits, setGoalEdits] = useState<Record<string, number>>({});

  const refresh = useCallback(async () => {
    try {
      setChildren(await getChildren());
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar crianças.");
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
    try {
      const child = await createChild({ name, age, monthlyGoalPoints: goal });
      setChildren((prev) => [...prev, child]);
      setName("");
      setAge(10);
      setGoal(200);
      setFeedback(`Criança "${child.name}" cadastrada com sucesso!`);
    } catch (err) {
      setFeedback(null);
      setError(err instanceof Error ? err.message : "Erro ao cadastrar.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleSaveGoal = async (childId: string) => {
    const value = goalEdits[childId];
    if (value === undefined) return;
    setFeedback(null);
    setError(null);
    try {
      await updateChildGoal(childId, { monthlyGoalPoints: value, monthlyGoalRewardId: null });
      setChildren((prev) =>
        prev.map((c) => (c.id === childId ? { ...c, monthlyGoalPoints: value } : c))
      );
      setFeedback("Meta mensal atualizada!");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao atualizar meta.");
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
        <h1 className="text-2xl font-bold text-slate-800">Crianças</h1>
        <p className="text-xs text-brand-muted">Cadastre e defina as metas mensais.</p>
      </header>

      {error && <ErrorMessage message={error} />}
      {feedback && (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
          {feedback}
        </div>
      )}

      {/* Formulário de criação */}
      <form
        onSubmit={handleCreate}
        className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm"
      >
        <h2 className="mb-4 text-sm font-bold uppercase tracking-wider text-slate-500">
          Nova criança
        </h2>
        <div className="grid gap-4 sm:grid-cols-3">
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">Nome</label>
            <input
              required
              minLength={2}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ex.: Lucas"
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">Idade</label>
            <input
              type="number"
              min={1}
              max={18}
              value={age}
              onChange={(e) => setAge(Number(e.target.value))}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">
              Meta mensal (pts)
            </label>
            <input
              type="number"
              min={0}
              value={goal}
              onChange={(e) => setGoal(Number(e.target.value))}
              className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
        </div>
        <button
          type="submit"
          disabled={submitting}
          className="mt-4 inline-flex items-center gap-2 rounded-xl bg-brand-sidebar px-5 py-2.5 text-sm font-bold text-white transition-colors hover:bg-brand-sidebar-hover disabled:opacity-60"
        >
          <PlusCircle className="h-4 w-4" /> {submitting ? "Salvando..." : "Cadastrar"}
        </button>
      </form>

      {/* Lista */}
      <div className="space-y-3">
        {children.length === 0 ? (
          <p className="py-8 text-center text-sm text-brand-muted">Nenhuma criança cadastrada.</p>
        ) : (
          children.map((child) => (
            <div
              key={child.id}
              className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex items-center gap-3">
                  <div className="flex h-11 w-11 items-center justify-center rounded-full bg-gradient-to-br from-amber-400 to-orange-400 text-xl">
                    👦
                  </div>
                  <div>
                    <p className="font-bold text-slate-800">
                      {child.name} <span className="font-normal text-brand-muted">• {child.age} anos</span>
                    </p>
                    <div className="mt-0.5 flex items-center gap-1.5">
                      <Star className="h-4 w-4 fill-amber-400 text-amber-500" />
                      <span className="text-sm font-black text-slate-700">{child.pointsBalance} pts</span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <input
                    type="number"
                    min={0}
                    value={goalEdits[child.id] ?? child.monthlyGoalPoints}
                    onChange={(e) =>
                      setGoalEdits((prev) => ({ ...prev, [child.id]: Number(e.target.value) }))
                    }
                    className="w-24 rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar"
                  />
                  <button
                    onClick={() => handleSaveGoal(child.id)}
                    className="inline-flex items-center gap-1.5 rounded-xl bg-slate-800 px-3 py-2 text-xs font-bold text-white transition-colors hover:bg-slate-700"
                  >
                    <Save className="h-3.5 w-3.5" /> Meta
                  </button>
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}