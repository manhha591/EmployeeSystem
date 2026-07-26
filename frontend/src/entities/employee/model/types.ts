export interface Employee {
  id: number
  fullName: string
  email: string
  phone: string | null
  salary: number
  avatar: string | null
  departmentName: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}
