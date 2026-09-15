import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "KidsRewards",
  description: "Gerenciador de atividades e recompensas infantis",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR" className="antialiased">
      <body>{children}</body>
    </html>
  );
}