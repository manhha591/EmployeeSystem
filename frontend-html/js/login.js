// =============================================
// login.js — Xử lý đăng nhập
// =============================================

// Đã đăng nhập thì vào thẳng dashboard
if (getAccessToken()) {
  window.location.href = "dashboard.html";
}

document.getElementById("loginForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const username = document.getElementById("username").value.trim();
  const password = document.getElementById("password").value;
  const errorBox = document.getElementById("loginError");
  const btn = document.getElementById("loginBtn");

  errorBox.style.display = "none";

  if (!username || !password) {
    showError("Vui lòng nhập đầy đủ tài khoản và mật khẩu");
    return;
  }

  btn.disabled = true;
  btn.textContent = "Đang xử lý...";

  try {
    const data = await api.post("/Auth/login", { username, password });

    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);

    window.location.href = "dashboard.html";
  } catch {
    showError("Sai tài khoản hoặc mật khẩu");
  } finally {
    btn.disabled = false;
    btn.textContent = "Đăng nhập";
  }
});

function showError(message) {
  const errorBox = document.getElementById("loginError");
  errorBox.textContent = message;
  errorBox.style.display = "block";
}
