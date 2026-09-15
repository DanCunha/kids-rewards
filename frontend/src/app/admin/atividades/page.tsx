"use client";

import { useCallback, useEffect, useState } from "react";
import { CheckCircle2, PlusCircle } from "lucide-react";
import { completeActivity, createActivity, getActivitiesByChild, getChildren } from "@/lib/api";
import type { Activity, Child } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

export default function AdminActivities() {
  const [children, setChildren] = useState<Child[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [activities, setActivities] = useState<Activity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [points, setPoints] = useState(10);
  const [submitting, setSubmitting] = useState(false);

  const loadChildren = useCallback(async () => {
    try {
      const kids = await getChildren();
      setChildren(kids);
      if (kids.length > 0) setSelectedId((prev) => prev || kids[0].id);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar crianças.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadChildren();
  }, [loadChildren]);

  const loadActivities = useCallback(async (childId: string) => {
    if (!childId) return;
    try {
      setActivities(await getActivitiesByChild(childId));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar atividades.");
    }
  }, []);

  useEffect(() => {
    if (selectedId) loadActivities(selectedId);
  }, [selectedId, loadActivities]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedId) return;
    setSubmitting(true);
    setFeedback(null);
    setError(null);
    try {
      const activity = await createActivity({
        childId: selectedId,
        title,
        description,
        points,
      });
      setActivities((prev) => [activity, ...prev]);
      setTitle("");
      setDescription("");
      setPoints(10);
      setFeedback(`Atividade "${activity.title}" criada!`);
    } catch (err) {
      setFeedback(null);
      setError(err instanceof Error ? err.message : "Erro ao criar atividade.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleComplete = async (activityId: string) => {
    setFeedback(null);
    setError(null);
    try {
      await completeActivity(activityId);
      setActivities((prev) =>
        prev.map((a) => (a.id === activityId ? { ...a, isCompleted: true } : a))
      );
      setFeedback("Atividade concluída! Pontos adicionados ao saldo.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao concluir atividade.");
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
        <h1 className="text-2xl font-bold text-slate-800">Atividades</h1>
        <p className="text-xs text-brand-muted">Crie tarefas e acompanhe a conclusão.</p>
      </header>

      {error && <ErrorMessage message={error} />}
      {feedback && (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
          {feedback}
        </div>
      )}

      {children.length === 0 ? (
        <p className="rounded-2xl border border-slate-100 bg-white p-8 text-center text-sm text-brand-muted">
          Cadastre uma criança antes de criar atividades.
        </p>
      ) : (
        <>
          {/* Formulário */}
          <form
            onSubmit={handleCreate}
            className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm"
          >
            <h2 className="mb-4 text-sm font-bold uppercase tracking-wider text-slate-500">
              Nova atividade
            </h2>
            <div className="grid gap-4 sm:grid-cols-2">
              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-600">Criança</label>
                <select
                  value={selectedId}
                  onChange={(e) => setSelectedId(e.target.value)}
                  className="w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm outline-none focus:border-brand-sidebar"
                >
                  {children.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-600">
                  Pontos
                </label>
                <input
                  type="number"
                  min={1}
                  value={points}
                  onChange={(e) => setPoints(Number(e.target.value))}
                  className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-600">Título</label>
                <input
                  required
                  minLength={3}
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="Ex.: Arrumar a cama"
                  className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-semibold text-slate-600">
                  Descrição
                </label>
                <input
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Opcional"
                  className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-brand-sidebar"
                />
              </div>
            </div>
            <button
              type="submit"
              disabled={submitting}
              className="mt-4 inline-flex items-center gap-2 rounded-xl bg-emerald-600 px-5 py-2.5 text-sm font-bold text-white transition-colors hover:bg-emerald-700 disabled:opacity-60"
            >
              <PlusCircle className="h-4 w-4" /> {submitting ? "Salvando..." : "Criar atividade"}
            </button>
          </form>

          {/* Lista */}
          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-bold uppercase tracking-wider text-slate-500">
                Tarefas
              </h2>
              <select
                value={selectedId}
                onChange={(e) => setSelectedId(e.target.value)}
                className="rounded-xl border border-slate-200 bg-white px-3 py-1.5 text-xs font-semibold outline-none"
              >
                {children.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            {activities.length === 0 ? (
              <p className="rounded-2xl border border-slate-100 bg-white p-6 text-center text-sm text-brand-muted">
                Nenhuma atividade para esta criança.
              </p>
            ) : (
              activities.map((act) => (
                <div
                  key={act.id}
                  className={`flex items-center justify-between rounded-2xl border p-4 shadow-sm ${
                    act.isCompleted
                      ? "border-emerald-200 bg-emerald-50"
                      : "border-slate-100 bg-white"
                  }`}
                >
                  <div>
                    <p
                      className={`font-bold ${
                        act.isCompleted ? "text-emerald-800 line-through" : "text-slate-800"
                      }`}
                    >
                      {act.title}
                    </p>
                    {act.description && (
                      <p className="text-xs text-slate-500">{act.description}</p>
                    )}
                    <span className="mt-1.5 inline-flex rounded-md border border-amber-200 bg-amber-50 px-2 py-0.5 text-xs font-black text-amber-600">
                      +{act.points} pts
                    </span>
                  </div>
                  {act.isCompleted ? (
                    <span className="inline-flex items-center gap-1 text-xs font-bold text-emerald-600">
                      <CheckCircle2 className="h-4 w-4" /> Feito!
                    </span>
                  ) : (
                    <button
                      onClick={() => handleComplete(act.id)}
                      className="inline-flex h-10 items-center gap-1.5 rounded-xl bg-gradient-to-r from-emerald-500 to-teal-600 px-4 text-xs font-bold text-white shadow-md transition-transform active:scale-95"
                    >
                      <CheckCircle2 className="h-4 w-4" /> Concluir
                    </button>
                  )}
                </div>
              ))
            )}
          </div>
        </>
      )}
    </div>
  );
}