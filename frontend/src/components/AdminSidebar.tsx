"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Award,
  CheckCircle2,
  Gift,
  History,
  LayoutDashboard,
  Users,
} from "lucide-react";

const navItems = [
  { href: "/admin", label: "Dashboard", icon: LayoutDashboard },
  { href: "/admin/criancas", label: "Crianças", icon: Users },
  { href: "/admin/atividades", label: "Atividades", icon: CheckCircle2 },
  { href: "/admin/recompensas", label: "Recompensas", icon: Gift },
  { href: "/admin/historico", label: "Histórico", icon: History },
];

export default function AdminSidebar() {
  const pathname = usePathname();

  return (
    <>
      {/* Sidebar desktop */}
      <aside className="hidden w-64 shrink-0 flex-col bg-brand-sidebar text-white lg:flex">
        <div className="p-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-white text-lg shadow">
              ⭐
            </div>
            <div>
              <h2 className="text-lg font-bold tracking-wide">KidsRewards</h2>
              <p className="text-xs text-blue-200">Painel dos Pais</p>
            </div>
          </div>
        </div>

        <nav className="flex-1 space-y-1.5 overflow-y-auto px-4 py-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = pathname === item.href || (item.href !== "/admin" && pathname.startsWith(item.href));
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center gap-3 rounded-xl px-4 py-2.5 text-sm font-medium transition-colors ${
                  active ? "bg-brand-sidebar-hover text-white" : "text-blue-100 hover:bg-brand-sidebar-hover/50"
                }`}
              >
                <Icon className="h-4 w-4" /> {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="mt-auto border-t border-blue-400/30 p-4">
          <Link
            href="/"
            className="flex items-center gap-3 rounded-xl px-3 py-2 text-sm text-blue-100 hover:bg-brand-sidebar-hover/50"
          >
            <Award className="h-4 w-4" /> Ver painel da criança
          </Link>
        </div>
      </aside>

      {/* Navegação mobile */}
      <nav className="sticky top-0 z-20 flex gap-1 overflow-x-auto bg-brand-sidebar px-3 py-2 lg:hidden">
        {navItems.map((item) => {
          const Icon = item.icon;
          const active = pathname === item.href || (item.href !== "/admin" && pathname.startsWith(item.href));
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex shrink-0 items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-semibold ${
                active ? "bg-white text-brand-sidebar" : "text-blue-100"
              }`}
            >
              <Icon className="h-3.5 w-3.5" /> {item.label}
            </Link>
          );
        })}
        <Link
          href="/"
          className="flex shrink-0 items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-semibold text-amber-200"
        >
          <Award className="h-3.5 w-3.5" /> Criança
        </Link>
      </nav>
    </>
  );
}