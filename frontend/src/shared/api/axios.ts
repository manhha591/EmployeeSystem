import axios from "axios"

const baseURL = (window as any).__API_URL__ || import.meta.env.VITE_API_URL
const api = axios.create({ baseURL })

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken")
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (res) => res,
  async (err) => {
    const originalRequest = err.config

    if (err.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      const refreshToken = localStorage.getItem("refreshToken")
      if (!refreshToken) {
        localStorage.clear()
        window.location.href = "/login"
        return Promise.reject(err)
      }

      try {
        const { data } = await axios.post(
          `${baseURL}/Auth/refresh`,
          { refreshToken }
        )

        localStorage.setItem("accessToken", data.accessToken)
        localStorage.setItem("refreshToken", data.refreshToken)

        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`
        return api(originalRequest)
      } catch {
        localStorage.clear()
        window.location.href = "/login"
        return Promise.reject(err)
      }
    }

    return Promise.reject(err)
  }
)

export default api
