import api from "../../../shared/api/axios"
import type { Department } from "../../../entities/department/model/types"

export const departmentApi = {
  getAll: () => api.get<Department[]>("/Departments").then((r) => r.data),

  create: (name: string) =>
    api.post<Department>("/Departments", { name }).then((r) => r.data),

  update: (id: number, name: string) =>
    api.put(`/Departments/${id}`, { id, name }),

  delete: (id: number) => api.delete(`/Departments/${id}`),
}
