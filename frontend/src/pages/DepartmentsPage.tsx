import { useState, useEffect } from "react"
import { departmentApi } from "../features/department/api/departmentApi"
import type { Department } from "../entities/department/model/types"

function DepartmentsPage() {
  const [departments, setDepartments] = useState<Department[]>([])
  const [loading, setLoading] = useState(true)
  const [name, setName] = useState("")
  const [editId, setEditId] = useState<number | null>(null)

  const loadList = () => {
    departmentApi.getAll().then((data) => {
      setDepartments(data)
      setLoading(false)
    })
  }

  useEffect(() => { loadList() }, [])

  const handleSave = async () => {
    if (!name.trim()) return

    if (editId !== null) {
      await departmentApi.update(editId, name)
    } else {
      await departmentApi.create(name)
    }

    setName("")
    setEditId(null)
    loadList()
  }

  const handleEdit = (dept: Department) => {
    setName(dept.name)
    setEditId(dept.id)
  }

  const handleDelete = async (id: number) => {
    await departmentApi.delete(id)
    loadList()
  }

  if (loading) return <p className="p-4 text-gray-500">Đang tải...</p>

  return (
    <div className="p-4">
      <h2 className="text-xl font-semibold mb-4">Danh sách phòng ban</h2>

      <div className="flex gap-2 mb-4">
        <input
          placeholder="Tên phòng ban"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="px-3 py-2 border border-gray-300 rounded focus:outline-none focus:border-blue-500"
        />
        <button
          onClick={handleSave}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 cursor-pointer"
        >
          {editId !== null ? "Cập nhật" : "Thêm"}
        </button>
        {editId !== null && (
          <button
            onClick={() => { setName(""); setEditId(null) }}
            className="px-4 py-2 bg-gray-300 rounded hover:bg-gray-400 cursor-pointer"
          >
            Hủy
          </button>
        )}
      </div>

      <ul className="space-y-2">
        {departments.map((dept) => (
          <li key={dept.id} className="flex items-center gap-3 p-3 border border-gray-200 rounded">
            <span className="flex-1">{dept.name}</span>
            <button
              onClick={() => handleEdit(dept)}
              className="px-3 py-1 bg-yellow-400 rounded hover:bg-yellow-500 cursor-pointer"
            >
              Sửa
            </button>
            <button
              onClick={() => handleDelete(dept.id)}
              className="px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600 cursor-pointer"
            >
              Xóa
            </button>
          </li>
        ))}
      </ul>
    </div>
  )
}

export default DepartmentsPage
