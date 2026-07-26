import { useState, useEffect } from "react"
import Card from "../shared/ui/Card"
import { dashboardApi, type DashboardData } from "../features/dashboard/api/dashboardApi"

function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null)

  useEffect(() => {
    dashboardApi.getStats().then(setData)
  }, [])

  if (!data) return <p className="p-4 text-gray-500">Đang tải...</p>

  const maxCount = Math.max(...data.employeesByDepartment.map((d) => d.count), 1)

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-6">Tổng quan</h2>

      <div className="flex gap-4 mb-6">
        <Card label="Nhân viên" value={data.totalEmployees} color="#1677ff" />
        <Card label="Phòng ban" value={data.totalDepartments} color="#52c41a" />
        <Card label="Tổng lương" value={`${data.totalSalary.toLocaleString()} VND`} color="#fa8c16" />
      </div>

      <h3 className="text-lg font-medium mb-3">Nhân viên theo phòng ban</h3>
      <table className="w-full border-collapse border border-gray-300">
        <thead>
          <tr className="bg-gray-100">
            <th className="border border-gray-300 px-4 py-2 text-left">Phòng ban</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Số nhân viên</th>
            <th className="border border-gray-300 px-4 py-2 text-left">Tổng lương</th>
          </tr>
        </thead>
        <tbody>
          {data.employeesByDepartment.map((d) => (
            <tr key={d.departmentName} className="hover:bg-gray-50">
              <td className="border border-gray-300 px-4 py-2">{d.departmentName}</td>
              <td className="border border-gray-300 px-4 py-2">
                <div className="flex items-center gap-2">
                  <div
                    className="h-5 rounded-sm bg-blue-500"
                    style={{ width: `${Math.max((d.count / maxCount) * 100, 2)}%` }}
                  />
                  {d.count}
                </div>
              </td>
              <td className="border border-gray-300 px-4 py-2">{d.totalSalary.toLocaleString()} VND</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default DashboardPage
