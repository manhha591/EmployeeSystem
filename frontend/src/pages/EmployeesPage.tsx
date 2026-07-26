import { useState, useEffect } from "react"
import { employeeApi } from "../features/employee/api/employeeApi"
import type { PagedResult } from "../entities/employee/model/types"
import type { Employee } from "../entities/employee/model/types"

function EmployeesPage() {
  const [data, setData] = useState<PagedResult<Employee> | null>(null)
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)

  const loadList = () => {
    employeeApi.getPaged({ search, page, pageSize: 5 }).then(setData)
  }

  useEffect(() => { loadList() }, [page])

  const handleSearch = () => {
    setPage(1)
  }

  if (!data) return <p className="p-4 text-gray-500">Đang tải...</p>

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Danh sách nhân viên</h2>

      <div className="flex gap-2 mb-4">
        <input
          placeholder="Tìm kiếm..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
        />
        <button
          onClick={handleSearch}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 cursor-pointer"
        >
          Tìm
        </button>
      </div>

      <table className="w-full border-collapse border border-gray-300">
        <thead>
          <tr className="bg-gray-100">
            <th className="border border-gray-300 px-4 py-2 text-left">Tên</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Email</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Phone</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Lương</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Phòng ban</th>
          </tr>
        </thead>
        <tbody>
          {data.items.map((emp) => (
            <tr key={emp.id} className="hover:bg-gray-50">
              <td className="border border-gray-300 px-4 py-2">{emp.fullName}</td>
              <td className="border border-gray-300 px-4 py-2">{emp.email}</td>
              <td className="border border-gray-300 px-4 py-2">{emp.phone || "-"}</td>
              <td className="border border-gray-300 px-4 py-2">{emp.salary}</td>
              <td className="border border-gray-300 px-4 py-2">{emp.departmentName}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="flex items-center gap-3 mt-4">
        <button
          disabled={page <= 1}
          onClick={() => setPage(page - 1)}
          className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
        >
          Trước
        </button>
        <span className="text-gray-600">Trang {data.page} / {data.totalPages}</span>
        <button
          disabled={page >= data.totalPages}
          onClick={() => setPage(page + 1)}
          className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
        >
          Sau
        </button>
      </div>
    </div>
  )
}

export default EmployeesPage
