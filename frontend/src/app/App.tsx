import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom"
import Navbar from "../widgets/Navbar"
import LoginPage from "../pages/LoginPage"
import DashboardPage from "../pages/DashboardPage"
import DepartmentsPage from "../pages/DepartmentsPage"
import EmployeesPage from "../pages/EmployeesPage"

function App() {
  const token = localStorage.getItem("accessToken")

  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <div className="p-6">
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
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
