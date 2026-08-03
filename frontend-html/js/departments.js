// =============================================
// departments.js — CRUD Phòng ban
// =============================================

let editingId = null; // null = đang thêm mới, có số = đang sửa

if (requireAuth()) {
  loadDepartments();
}

// ---- Load danh sách ----
async function loadDepartments() {
  const tbody = document.getElementById("deptBody");
  try {
    const departments = await api.get("/Departments");

    if (!departments.length) {
      tbody.innerHTML = `<tr><td colspan="3" class="loading">Chưa có phòng ban nào</td></tr>`;
      return;
    }

    tbody.innerHTML = departments
      .map(
        (d) => `
        <tr>
          <td>${d.id}</td>
          <td>${d.name}</td>
          <td>
            <button class="btn btn-sm btn-ghost" onclick="openEditModal(${d.id}, '${d.name.replace(/'/g, "\\'")}')">Sửa</button>
            <button class="btn btn-sm btn-danger" onclick="deleteDepartment(${d.id}, '${d.name.replace(/'/g, "\\'")}')">Xóa</button>
          </td>
        </tr>`
      )
      .join("");
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="3" class="loading">Không tải được dữ liệu: ${err.message}</td></tr>`;
  }
}

// ---- Modal ----
function openCreateModal() {
  editingId = null;
  document.getElementById("modalTitle").textContent = "Thêm phòng ban";
  document.getElementById("deptName").value = "";
  document.getElementById("modalOverlay").classList.add("open");
  document.getElementById("deptName").focus();
}

function openEditModal(id, name) {
  editingId = id;
  document.getElementById("modalTitle").textContent = "Sửa phòng ban";
  document.getElementById("deptName").value = name;
  document.getElementById("modalOverlay").classList.add("open");
}

function closeModal() {
  document.getElementById("modalOverlay").classList.remove("open");
}

// Bấm nút Lưu
document.getElementById("saveBtn").addEventListener("click", async () => {
  const name = document.getElementById("deptName").value.trim();
  if (!name) return alert("Vui lòng nhập tên phòng ban");

  try {
    if (editingId === null) {
      await api.post("/Departments", { name });
      showAlert("Thêm phòng ban thành công!", "success");
    } else {
      await api.put(`/Departments/${editingId}`, { id: editingId, name });
      showAlert("Cập nhật thành công!", "success");
    }
    closeModal();
    loadDepartments();
  } catch (err) {
    showAlert(err.message, "error");
  }
});

// Xóa phòng ban (có xác nhận)
async function deleteDepartment(id, name) {
  if (!confirm(`Bạn có chắc muốn xóa phòng ban "${name}"?`)) return;

  try {
    await api.delete(`/Departments/${id}`);
    showAlert("Xóa thành công!", "success");
    loadDepartments();
  } catch (err) {
    showAlert(err.message, "error");
  }
}

// Thông báo
function showAlert(message, type) {
  const box = document.getElementById("alertBox");
  box.textContent = message;
  box.className = `alert show alert-${type}`;
  setTimeout(() => box.classList.remove("show"), 3000);
}
