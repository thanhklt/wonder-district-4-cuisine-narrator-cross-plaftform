// Rỗng = relative URL (hoạt động qua nginx và Cloudflare tunnel)
// Có giá trị = URL tuyệt đối (dev local không qua nginx)
const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "";

export function getSessionToken(): string | null {
  return typeof window !== "undefined" ? localStorage.getItem("session_token") : null;
}

export function setSessionToken(token: string) {
  localStorage.setItem("session_token", token);
}

export function clearSessionToken() {
  if (typeof window !== "undefined") localStorage.removeItem("session_token");
}

export type VerifyResult =
  | { status: "valid"; expiresAt: string }
  | { status: "invalid" }
  | { status: "offline" };

export async function verifySession(): Promise<VerifyResult> {
  const token = getSessionToken();
  if (!token) return { status: "invalid" };
  try {
    const res = await fetch(`${API_URL}/api/access/verify`, {
      method: "POST",
      headers: { "X-Session-Token": token },
    });
    if (res.status === 401) return { status: "invalid" };
    if (!res.ok) return { status: "offline" };
    const data = await res.json();
    return { status: "valid", expiresAt: data.expiresAt };
  } catch {
    return { status: "offline" };
  }
}

export async function fetchBootstrap() {
  const token = getSessionToken();
  const res = await fetch(`${API_URL}/api/access/bootstrap`, {
    headers: { "X-Session-Token": token ?? "" },
  });
  if (!res.ok) throw new Error("Bootstrap failed");
  return res.json();
}

export async function initiatePayment(code: string) {
  const res = await fetch(`${API_URL}/api/access/pay`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ code }),
  });
  if (!res.ok) throw new Error("Payment init failed");
  return res.json() as Promise<{ paymentUrl: string; txnRef: string }>;
}

export async function translateText(text: string, targetLang: string): Promise<string> {
  const deepTranslateUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";
  const res = await fetch(`${deepTranslateUrl}/api/localize/translate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text, source_language: "vi", target_language: targetLang }),
  });
  if (!res.ok) return text;
  const data = await res.json();
  return data.translated_text ?? text;
}
