import api from "../../../shared/api/axios"

export interface DashboardData {
  totalEmployees: number
  totalDepartments: number
  totalSalary: number
  employeesByDepartment: {
    departmentName: string
    count: number
    totalSalary: number
  }[]
}

export const dashboardApi = {
  getStats: () =>
    api.get<DashboardData>("/Dashboard").then((r) => r.data),
}
