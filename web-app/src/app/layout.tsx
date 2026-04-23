import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "AudioTravelling — Admin",
  description: "Quản trị hệ thống AudioTravelling",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="vi" className="h-full">
      <body className="h-full bg-gray-100 text-gray-900 antialiased">{children}</body>
    </html>
  );
}
