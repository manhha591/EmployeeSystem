import axios from "axios"

// Ưu tiên dùng API_URL từ runtime (config.js), fallback về Vite env (build-time)
const baseURL = (window as any).__API_URL__ || import.meta.env.VITE_API_URL
const api = axios.create({ baseURL })

// Interceptor cho request: tự động đính kèm JWT token vào header
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken")
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Interceptor cho response: tự động refresh token khi hết hạn (401)
api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const originalRequest = err.config

    // Nếu lỗi 401 và chưa thử refresh lần nào
    if (err.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      const refreshToken = localStorage.getItem("refreshToken")
      if (!refreshToken) {
        localStorage.clear()
        window.location.href = "/login"
        return Promise.reject(err)
      }

      try {
        // Gọi API refresh token để lấy cặp token mới
        const { data } = await axios.post(
          `${baseURL}/Auth/refresh`,
          { refreshToken }
        )

        localStorage.setItem("accessToken", data.accessToken)
        localStorage.setItem("refreshToken", data.refreshToken)

        // Gắn token mới và thực hiện lại request ban đầu
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
        return api(originalRequest)
      } catch {
        // Refresh thất bại -> xóa toàn bộ token, quay về login
        localStorage.clear()
        window.location.href = "/login"
        return Promise.reject(err)
      }
    }

    return Promise.reject(err)
  }
)

export default api
