const API_URL = process.env.NEXT_PUBLIC_WEBAPP_API_URL ?? "http://localhost:5000";

export function getToken(): string | null {
  return typeof window !== "undefined" ? localStorage.getItem("admin_token") : null;
}

export function setToken(token: string) {
  localStorage.setItem("admin_token", token);
}

export function clearToken() {
  localStorage.removeItem("admin_token");
  localStorage.removeItem("admin_role");
}

function authHeaders() {
  return { Authorization: `Bearer ${getToken()}`, "Content-Type": "application/json" };
}

export async function login(email: string, password: string) {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) throw new Error("Invalid credentials");
  return res.json() as Promise<{ token: string; role: string; userId: string }>;
}

// ── Packages ────────────────────────────────────────────────
export async function getPackages() {
  const res = await fetch(`${API_URL}/api/packages`, { headers: authHeaders() });
  return res.json();
}

// ── POIs (Owner) ────────────────────────────────────────────
export async function getMyPois() {
  const res = await fetch(`${API_URL}/api/pois`, { headers: authHeaders() });
  return res.json();
}

export async function createPoi(data: any) {
  const res = await fetch(`${API_URL}/api/pois`, {
    method: "POST", headers: authHeaders(), body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Create failed");
  return res.json();
}

export async function submitPoi(id: string) {
  const res = await fetch(`${API_URL}/api/pois/${id}/submit`, {
    method: "PATCH", headers: authHeaders(),
  });
  if (!res.ok) throw new Error("Submit failed");
  return res.json();
}

export async function updatePoi(id: string, data: any) {
  const res = await fetch(`${API_URL}/api/pois/${id}`, {
    method: "PUT", headers: authHeaders(), body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Update failed");
  return res.json();
}

export async function deletePoi(id: string) {
  await fetch(`${API_URL}/api/pois/${id}`, { method: "DELETE", headers: authHeaders() });
}

// ── POIs (Admin) ─────────────────────────────────────────────
export async function getPendingPois() {
  const res = await fetch(`${API_URL}/api/pois/pending`, { headers: authHeaders() });
  return res.json();
}

export async function approvePoi(id: string) {
  const res = await fetch(`${API_URL}/api/pois/${id}/approve`, {
    method: "PATCH", headers: authHeaders(),
  });
  if (!res.ok) throw new Error("Approve failed");
  return res.json();
}

export async function rejectPoi(id: string, note?: string) {
  const res = await fetch(`${API_URL}/api/pois/${id}/reject`, {
    method: "PATCH", headers: authHeaders(), body: JSON.stringify({ note }),
  });
  if (!res.ok) throw new Error("Reject failed");
  return res.json();
}

// ── QR ───────────────────────────────────────────────────────
export async function getQrCodes() {
  const res = await fetch(`${API_URL}/api/qr`, { headers: authHeaders() });
  return res.json();
}

export async function createQr() {
  const res = await fetch(`${API_URL}/api/qr`, { method: "POST", headers: authHeaders() });
  if (!res.ok) throw new Error("Create QR failed");
  return res.json();
}

export async function toggleQr(id: string) {
  const res = await fetch(`${API_URL}/api/qr/${id}/toggle`, {
    method: "PATCH", headers: authHeaders(),
  });
  return res.json();
}

export async function deleteQr(id: string) {
  await fetch(`${API_URL}/api/qr/${id}`, { method: "DELETE", headers: authHeaders() });
}

export async function regenerateQr(id: string) {
  const res = await fetch(`${API_URL}/api/qr/${id}/regenerate`, {
    method: "POST", headers: authHeaders(),
  });
  if (!res.ok) throw new Error("Regenerate QR failed");
  return res.json();
}

// ── Stats ────────────────────────────────────────────────────
export async function getStats(period: string) {
  const res = await fetch(`${API_URL}/api/stats/sessions?period=${period}`, { headers: authHeaders() });
  return res.json();
}

export async function getHeatmap(period: string) {
  const res = await fetch(`${API_URL}/api/stats/heatmap?period=${period}`, { headers: authHeaders() });
  return res.json();
}

export async function getRealtimeCount() {
  const res = await fetch(`${API_URL}/api/stats/realtime`, { headers: authHeaders() });
  return res.json();
}

export { API_URL };
