"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { LogIn } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      const u = await login({ email, password });
      router.replace(u.type === "Child" ? "/" : "/admin");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Não foi possível entrar.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-brand-bg p-4">
      <div className="w-full max-w-sm rounded-3xl border border-slate-100 bg-white p-8 shadow-lg">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-sidebar text-2xl shadow">
            ⭐
          </div>
          <h1 className="text-2xl font-bold text-slate-800">KidsRewards</h1>
          <p className="mt-1 text-sm text-brand-muted">Entre para gerenciar as recompensas</p>
        </div>

        {error && (
          <div className="mb-4">
            <ErrorMessage message={error} />
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">Email</label>
            <input
              type="email"
              required
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="voce@email.com"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
          <div>
            <label className="mb-1 block text-xs font-semibold text-slate-600">Senha</label>
            <input
              type="password"
              required
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Sua senha"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none focus:border-brand-sidebar focus:ring-2 focus:ring-blue-100"
            />
          </div>
          <button
            type="submit"
            disabled={submitting}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-brand-sidebar px-5 py-3 text-sm font-bold text-white transition-colors hover:bg-brand-sidebar-hover disabled:opacity-60"
          >
            {submitting ? <Spinner className="h-4 w-4 border-white/40 border-t-white" /> : <LogIn className="h-4 w-4" />}
            {submitting ? "Entrando..." : "Entrar"}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-brand-muted">
          Ainda não tem conta?{" "}
          <Link href="/register" className="font-semibold text-brand-sidebar hover:underline">
            Cadastre-se
          </Link>
        </p>
      </div>
    </main>
  );
}
