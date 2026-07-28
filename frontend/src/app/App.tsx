import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import Navbar from "../widgets/Navbar"
import LoginPage from "../pages/LoginPage"
import DashboardPage from "../pages/DashboardPage"
import DepartmentsPage from "../pages/DepartmentsPage"
import EmployeesPage from "../pages/EmployeesPage"

// Component gốc của ứng dụng, định nghĩa routing
function App() {
  // Kiểm tra token để biết user đã đăng nhập chưa
  const token = localStorage.getItem("accessToken")

  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50">
        <Navbar />                          {/* Thanh điều hướng trên cùng */}
        <div className="p-6">
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            {/* Các route yêu cầu đăng nhập: nếu không có token thì chuyển về /login */}
            <Route path="/dashboard" element={token ? <DashboardPage /> : <Navigate to="/login" />} />
            <Route path="/departments" element={token ? <DepartmentsPage /> : <Navigate to="/login" />} />
            <Route path="/employees" element={token ? <EmployeesPage /> : <Navigate to="/login" />} />
          </Routes>
        </div>
      </div>
    </BrowserRouter>
  )
}

export default App
