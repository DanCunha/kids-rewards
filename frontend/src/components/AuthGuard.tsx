"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import Spinner from "@/components/Spinner";

export default function AuthGuard({
  children,
  allowChild = true,
}: {
  children: React.ReactNode;
  allowChild?: boolean;
}) {
  const { user, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (loading) return;
    if (!user) {
      router.replace("/login");
    } else if (!allowChild && user.type === "Child") {
      router.replace("/");
    }
  }, [loading, user, allowChild, router]);

  if (loading || !user || (!allowChild && user.type === "Child")) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-brand-bg">
        <div className="flex items-center gap-2 text-sm font-medium text-brand-muted">
          <Spinner /> Carregando...
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
