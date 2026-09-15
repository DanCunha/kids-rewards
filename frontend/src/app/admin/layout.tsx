import AdminSidebar from "@/components/AdminSidebar";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col bg-brand-bg text-brand-text lg:flex-row">
      <AdminSidebar />
      <main className="min-w-0 flex-1 overflow-y-auto">
        <div className="px-4 py-6 lg:px-8 lg:py-8">{children}</div>
      </main>
    </div>
  );
}