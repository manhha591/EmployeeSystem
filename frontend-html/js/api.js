// =============================================
// api.js — Fetch wrapper: token, refresh token
// =============================================

// API URL — đổi ở đây nếu cần (VD: https://employee-api-zkyo.onrender.com/api)
const API_URL = "/api";

// Lấy token từ localStorage
function getAccessToken() {
  return localStorage.getItem("accessToken");
}

// Gọi API với token tự động đính kèm + tự refresh khi 401
async function apiFetch(path, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {}),
  };

  const token = getAccessToken();
  if (token) headers["Authorization"] = `Bearer ${token}`;

  let res = await fetch(`${API_URL}${path}`, { ...options, headers });

  // Token hết hạn (401) -> thử refresh 1 lần
  if (res.status === 401 && !options._retried) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      options._retried = true;
      const newHeaders = { ...headers, Authorization: `Bearer ${getAccessToken()}` };
      res = await fetch(`${API_URL}${path}`, { ...options, headers: newHeaders });
    }
  }

  return res;
}

// Gọi API và tự parse JSON, ném lỗi nếu response không OK
async function apiRequest(path, options = {}) {
  const res = await apiFetch(path, options);

  if (!res.ok) {
    const text = await res.text();
    let message = text;
    try {
      message = JSON.parse(text).message || text;
    } catch {}
    throw new Error(message || `HTTP ${res.status}`);
  }

  const contentType = res.headers.get("content-type") || "";
  return contentType.includes("application/json") ? res.json() : res.text();
}

// --- Helper methods ---
const api = {
  get: (path) => apiRequest(path),
  post: (path, body) =>
    apiRequest(path, { method: "POST", body: JSON.stringify(body) }),
  put: (path, body) =>
    apiRequest(path, { method: "PUT", body: JSON.stringify(body) }),
  delete: (path) => apiRequest(path, { method: "DELETE" }),
  upload: (path, formData) => {
    const headers = {};
    const token = getAccessToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;
    return apiFetch(path, { method: "POST", body: formData, headers }).then((res) => {
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return res.json();
    });
  },
};

// --- Refresh token ---
async function tryRefreshToken() {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) {
    logout();
    return false;
  }

  try {
    const res = await fetch(`${API_URL}/Auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) {
      logout();
      return false;
    }

    const data = await res.json();
    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    return true;
  } catch {
    logout();
    return false;
  }
}

// --- Auth helpers ---
function logout() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  window.location.href = "login.html";
}

// Kiểm tra đăng nhập: nếu chưa có token -> về trang login
function requireAuth() {
  if (!getAccessToken()) {
    window.location.href = "login.html";
    return false;
  }
  return true;
}

// Định dạng số tiền VND
function formatVND(amount) {
  return Number(amount || 0).toLocaleString("vi-VN") + " VND";
}

// Nút "Đăng xuất" trên navbar (nếu có)
document.addEventListener("DOMContentLoaded", () => {
  const logoutBtn = document.getElementById("logoutBtn");
  if (logoutBtn) {
    logoutBtn.addEventListener("click", (e) => {
      e.preventDefault();
      logout();
    });
  }
});
