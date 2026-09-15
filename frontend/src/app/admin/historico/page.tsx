"use client";

import { useCallback, useEffect, useState } from "react";
import { getChildren, getHistoryByChild } from "@/lib/api";
import type { Child, HistoryItem } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

function formatDate(iso: string) {
  return new Date(iso).toLocaleString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function AdminHistory() {
  const [children, setChildren] = useState<Child[]>([]);
  const [selectedId, setSelectedId] = useState("");
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

  const loadHistory = useCallback(async (childId: string) => {
    if (!childId) return;
    try {
      setHistory(await getHistoryByChild(childId));
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar histórico.");
    }
  }, []);

  useEffect(() => {
    if (selectedId) loadHistory(selectedId);
  }, [selectedId, loadHistory]);

  const totalEarned = history.filter((h) => h.type === "Earned").reduce((a, h) => a + h.points, 0);
  const totalSpent = history.filter((h) => h.type === "Spent").reduce((a, h) => a + h.points, 0);

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
        <h1 className="text-2xl font-bold text-slate-800">Histórico</h1>
        <p className="text-xs text-brand-muted">Extrato imutável de pontos por criança.</p>
      </header>

      {error && <ErrorMessage message={error} />}

      {children.length === 0 ? (
        <p className="rounded-2xl border border-slate-100 bg-white p-8 text-center text-sm text-brand-muted">
          Cadastre uma criança para ver o histórico.
        </p>
      ) : (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <select
              value={selectedId}
              onChange={(e) => setSelectedId(e.target.value)}
              className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold outline-none focus:border-brand-sidebar"
            >
              {children.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            <div className="flex gap-2 text-xs">
              <span className="rounded-full bg-emerald-100 px-3 py-1 font-bold text-emerald-700">
                Ganhou: +{totalEarned}
              </span>
              <span className="rounded-full bg-rose-100 px-3 py-1 font-bold text-rose-700">
                Gastou: -{totalSpent}
              </span>
            </div>
          </div>

          <div className="space-y-3">
            {history.length === 0 ? (
              <p className="rounded-2xl border border-slate-100 bg-white p-6 text-center text-sm text-brand-muted">
                Nenhuma operação registrada para esta criança.
              </p>
            ) : (
              history.map((item) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between rounded-xl border border-slate-100 bg-white p-3.5 text-xs"
                >
                  <div>
                    <p className="font-bold text-slate-800">{item.description}</p>
                    <p className="mt-0.5 text-[10px] text-slate-400">{formatDate(item.createdAt)}</p>
                  </div>
                  <span
                    className={`rounded-lg px-2.5 py-1 font-black text-sm ${
                      item.type === "Earned" ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"
                    }`}
                  >
                    {item.type === "Earned" ? `+${item.points}` : `-${item.points}`}
                  </span>
                </div>
              ))
            )}
          </div>
        </>
      )}
    </div>
  );
}