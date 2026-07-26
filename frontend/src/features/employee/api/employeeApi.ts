import api from "../../../shared/api/axios"
import type { Employee, PagedResult } from "../../../entities/employee/model/types"

export const employeeApi = {
  getPaged: (params: { search?: string; page: number; pageSize: number }) => {
    const query = new URLSearchParams()
    if (params.search) query.set("search", params.search)
    query.set("page", String(params.page))
    query.set("pageSize", String(params.pageSize))
    return api.get<PagedResult<Employee>>(`/Employees?${query}`).then((r) => r.data)
  },
}
