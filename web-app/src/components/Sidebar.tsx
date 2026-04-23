"use client";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { clearToken } from "@/lib/api";

const ownerLinks = [
  { href: "/pois", label: "POI của tôi" },
];

const adminLinks = [
  { href: "/admin/manage-pois", label: "Quản lý POI" },
  { href: "/admin/pois", label: "Duyệt POI" },
  { href: "/admin/qr", label: "Quản lý QR" },
  { href: "/admin/stats", label: "Thống kê" },
  { href: "/admin/realtime", label: "Trực tuyến" },
];

export default function Sidebar({ role }: { role: string }) {
  const pathname = usePathname();
  const router = useRouter();
  const links = role === "Admin" ? adminLinks : ownerLinks;

  function logout() {
    clearToken();
    router.replace("/login");
  }

  return (
    <aside className="w-56 bg-white border-r flex flex-col">
      <div className="p-4 border-b">
        <span className="font-bold text-orange-500">🎙️ AudioTravelling</span>
        <p className="text-xs text-gray-500 mt-0.5">{role}</p>
      </div>
      <nav className="flex-1 p-3 flex flex-col gap-1">
        {links.map((l) => (
          <Link
            key={l.href} href={l.href}
            className={`px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
              pathname.startsWith(l.href)
                ? "bg-orange-50 text-orange-600"
                : "text-gray-600 hover:bg-gray-100"
            }`}
          >
            {l.label}
          </Link>
        ))}
      </nav>
      <div className="p-3 border-t">
        <button onClick={logout} className="w-full text-sm text-gray-500 hover:text-red-500 py-2 text-left px-3">
          Đăng xuất
        </button>
      </div>
    </aside>
  );
}
