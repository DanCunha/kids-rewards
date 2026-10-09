"use client";

import { useState } from "react";
import Link from "next/link";
import { MailCheck, Send } from "lucide-react";
import { forgotPassword } from "@/lib/api";
import ErrorMessage from "@/components/ErrorMessage";
import Spinner from "@/components/Spinner";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await forgotPassword({ email });
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Não foi possível enviar o email.");
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
          <h1 className="text-2xl font-bold text-slate-800">Recuperar senha</h1>
          <p className="mt-1 text-sm text-brand-muted">
            {sent ? "Verifique seu email" : "Informe seu email para receber o link de redefinição"}
          </p>
        </div>

        {sent ? (
          <div className="space-y-4">
            <div className="flex items-start gap-3 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
              <MailCheck className="mt-0.5 h-5 w-5 shrink-0" />
              Se este email estiver cadastrado, enviaremos um link para redefinir sua senha.
            </div>
            <Link
              href="/login"
              className="block w-full rounded-xl bg-brand-sidebar px-5 py-3 text-center text-sm font-bold text-white transition-colors hover:bg-brand-sidebar-hover"
            >
              Voltar para o login
            </Link>
          </div>
        ) : (
          <>
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
              <button
                type="submit"
                disabled={submitting}
                className="flex w-full items-center justify-center gap-2 rounded-xl bg-brand-sidebar px-5 py-3 text-sm font-bold text-white transition-colors hover:bg-brand-sidebar-hover disabled:opacity-60"
              >
                {submitting ? <Spinner className="h-4 w-4 border-white/40 border-t-white" /> : <Send className="h-4 w-4" />}
                {submitting ? "Enviando..." : "Enviar link"}
              </button>
            </form>
          </>
        )}

        <p className="mt-6 text-center text-sm text-brand-muted">
          Lembrou a senha?{" "}
          <Link href="/login" className="font-semibold text-brand-sidebar hover:underline">
            Entrar
          </Link>
        </p>
      </div>
    </main>
  );
}
