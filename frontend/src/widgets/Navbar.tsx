import { Link } from "react-router-dom"

function Navbar() {
  const token = localStorage.getItem("accessToken")
  if (!token) return null

  return (
    <nav className="flex items-center gap-4 px-6 py-3 border-b border-gray-200 bg-white shadow-sm">
      <Link to="/dashboard" className="text-blue-600 hover:text-blue-800 font-medium">Tổng quan</Link>
      <Link to="/departments" className="text-blue-600 hover:text-blue-800 font-medium">Phòng ban</Link>
      <Link to="/employees" className="text-blue-600 hover:text-blue-800 font-medium">Nhân viên</Link>
      <button
        onClick={() => { localStorage.clear(); window.location.href = "/login" }}
        className="ml-auto px-3 py-1.5 bg-red-500 text-white rounded hover:bg-red-600 cursor-pointer"
      >
        Đăng xuất
      </button>
    </nav>
  )
}

export default Navbar
