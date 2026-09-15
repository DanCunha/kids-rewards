"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import {
  Award,
  CheckCircle2,
  Gift,
  Sparkles,
  Star,
  Users,
} from "lucide-react";
import { getActivitiesByChild, getChildren, getRewardsAll } from "@/lib/api";
import type { Activity, Child, Reward } from "@/lib/types";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

interface Kpi {
  label: string;
  value: number | string;
  icon: typeof Users;
  iconBg: string;
  iconColor: string;
}

export default function AdminOverview() {
  const [children, setChildren] = useState<Child[]>([]);
  const [rewards, setRewards] = useState<Reward[]>([]);
  const [pending, setPending] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const [kids, rws] = await Promise.all([getChildren(), getRewardsAll()]);
        const actByChild: Activity[][] = await Promise.all(kids.map((k) => getActivitiesByChild(k.id)));
        const pendingCount = actByChild.flat().filter((a) => !a.isCompleted).length;
        if (!active) return;
        setChildren(kids);
        setRewards(rws);
        setPending(pendingCount);
      } catch (err) {
        if (active) setError(err instanceof Error ? err.message : "Erro ao carregar dados.");
      } finally {
        if (active) setLoading(false);
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center gap-2 py-24 text-sm font-medium text-brand-muted">
        <Spinner /> Carregando painel...
      </div>
    );
  }

  if (error) return <ErrorMessage message={error} />;

  const totalBalance = children.reduce((acc, c) => acc + c.pointsBalance, 0);
  const activeRewards = rewards.filter((r) => r.isActive).length;

  const kpis: Kpi[] = [
    { label: "Crianças", value: children.length, icon: Users, iconBg: "bg-blue-50", iconColor: "text-blue-600" },
    { label: "Tarefas pendentes", value: pending, icon: CheckCircle2, iconBg: "bg-emerald-50", iconColor: "text-emerald-600" },
    { label: "Recompensas ativas", value: activeRewards, icon: Gift, iconBg: "bg-purple-50", iconColor: "text-purple-600" },
    { label: "Pontos em circulação", value: totalBalance, icon: Star, iconBg: "bg-amber-50", iconColor: "text-amber-500" },
  ];

  return (
    <div className="space-y-6">
      {/* Banner */}
      <section className="rounded-3xl bg-brand-banner p-6 text-white shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold">Olá, responsável!</h1>
              <span className="rounded-full bg-white/20 px-3 py-1 text-xs font-medium">
                Painel de gerenciamento
              </span>
            </div>
            <p className="mt-1 text-sm text-blue-100">
              Acompanhe as tarefas, os pontos e as recompensas das crianças.
            </p>
          </div>
          <Link
            href="/admin/criancas"
            className="flex items-center gap-2 rounded-lg border border-white/10 bg-white/10 px-4 py-2 text-sm font-medium transition-colors hover:bg-white/20"
          >
            <Sparkles className="h-4 w-4" /> Adicionar criança
          </Link>
        </div>
      </section>

      {/* KPIs */}
      <section className="grid grid-cols-2 gap-4 xl:grid-cols-4">
        {kpis.map((kpi) => {
          const Icon = kpi.icon;
          return (
            <div key={kpi.label} className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
              <div className="mb-4 flex items-center justify-between">
                <div className={`flex h-10 w-10 items-center justify-center rounded-full ${kpi.iconBg} ${kpi.iconColor}`}>
                  <Icon className="h-5 w-5" />
                </div>
              </div>
              <h3 className="text-2xl font-bold text-slate-800">{kpi.value}</h3>
              <p className="text-xs font-medium text-brand-muted">{kpi.label}</p>
            </div>
          );
        })}
      </section>

      <section className="grid gap-6 xl:grid-cols-3">
        {/* Crianças recentes */}
        <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm xl:col-span-2">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h3 className="text-lg font-bold text-slate-800">Crianças</h3>
              <p className="text-xs text-brand-muted">Resumo do saldo de cada criança</p>
            </div>
            <Link href="/admin/criancas" className="text-xs font-semibold text-blue-600 hover:underline">
              Gerenciar →
            </Link>
          </div>
          {children.length === 0 ? (
            <p className="py-8 text-center text-sm text-brand-muted">
              Nenhuma criança cadastrada ainda.
            </p>
          ) : (
            <div className="space-y-2">
              {children.map((child) => (
                <div
                  key={child.id}
                  className="flex items-center justify-between rounded-xl p-3 transition-colors hover:bg-slate-50"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-amber-400 to-orange-400 text-lg">
                      👦
                    </div>
                    <div>
                      <p className="text-sm font-bold text-slate-800">
                        {child.name} <span className="font-normal text-brand-muted">• {child.age} anos</span>
                      </p>
                      <p className="text-[10px] text-slate-400">
                        Meta: {child.monthlyGoalPoints} pts
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-1.5 rounded-xl bg-amber-50 px-3 py-1.5">
                    <Star className="h-4 w-4 fill-amber-400 text-amber-500" />
                    <span className="text-sm font-black text-slate-800">{child.pointsBalance}</span>
                    <span className="text-[10px] font-bold text-slate-500">pts</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Ações rápidas */}
        <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-lg font-bold text-slate-800">Ações rápidas</h3>
          <div className="grid gap-4">
            <Link
              href="/admin/atividades"
              className="flex min-h-[90px] flex-col justify-center rounded-2xl bg-emerald-500 p-5 text-white transition-colors hover:bg-emerald-600"
            >
              <div className="font-bold">Criar atividade</div>
              <div className="mt-1 text-xs text-emerald-100">Nova tarefa para ganhar pontos</div>
            </Link>
            <Link
              href="/admin/recompensas"
              className="flex min-h-[90px] flex-col justify-center rounded-2xl bg-purple-500 p-5 text-white transition-colors hover:bg-purple-600"
            >
              <div className="font-bold">Cadastrar recompensa</div>
              <div className="mt-1 text-xs text-purple-100">Novo prêmio no catálogo</div>
            </Link>
            <Link
              href="/"
              className="flex min-h-[90px] flex-col justify-center rounded-2xl bg-amber-400 p-5 text-white transition-colors hover:bg-amber-500"
            >
              <div className="flex items-center gap-2 font-bold">
                <Award className="h-5 w-5" /> Abrir painel da criança
              </div>
              <div className="mt-1 text-xs text-amber-100">Visualização mobile gamificada</div>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}