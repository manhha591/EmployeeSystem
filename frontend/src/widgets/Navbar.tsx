import { Link } from "react-router-dom"

function Navbar() {
  // Kiểm tra accessToken trong localStorage — nếu không có thì không hiển thị navbar (chưa đăng nhập)
  const token = localStorage.getItem("accessToken")
  if (!token) return null

  return (
    <nav className="flex items-center gap-4 px-6 py-3 border-b border-gray-200 bg-white shadow-sm">
      {/* Link đến trang tổng quan */}
      <Link to="/dashboard" className="text-blue-600 hover:text-blue-800 font-medium">Tổng quan</Link>
      {/* Link đến trang danh sách phòng ban */}
      <Link to="/departments" className="text-blue-600 hover:text-blue-800 font-medium">Phòng ban</Link>
      {/* Link đến trang danh sách nhân viên */}
      <Link to="/employees" className="text-blue-600 hover:text-blue-800 font-medium">Nhân viên</Link>
      {/* Nút đăng xuất: xoá toàn bộ localStorage và chuyển hướng về trang login */}
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
