import { useState } from "react"
import api from "../shared/api/axios"

// Trang đăng nhập
function LoginPage() {
  // State quản lý form đăng nhập
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState("")

  // Xử lý khi submit form
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    try {
      // Gọi API đăng nhập (baseURL đã được cấu hình trong axios.ts)
      const res = await api.post("/Auth/login", { username, password })
      // Lưu token vào localStorage để dùng cho các request sau
      localStorage.setItem("accessToken", res.data.accessToken)
      localStorage.setItem("refreshToken", res.data.refreshToken)
      // Chuyển hướng về dashboard
      window.location.href = "/dashboard"
    } catch {
      setError("Sai tài khoản hoặc mật khẩu")
    }
  }

  return (
    <div className="max-w-md mx-auto mt-24 p-6">
      <h2 className="text-2xl font-semibold mb-6 text-center">Đăng nhập</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <input
            id="username"
            name="username"
            placeholder="Tên đăng nhập"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
          />
        </div>
        <div>
          <input
            id="password"
            name="password"
            type="password"
            placeholder="Mật khẩu"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
          />
        </div>
        {error && <p className="text-red-500 text-sm">{error}</p>}
        <button
          type="submit"
          className="w-full py-2 bg-blue-600 text-white rounded hover:bg-blue-700 cursor-pointer"
        >
          Đăng nhập
        </button>
      </form>
    </div>
  )
}

export default LoginPage
