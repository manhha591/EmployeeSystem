// =============================================
// employees.js — CRUD Nhân viên + phân trang + lọc
// =============================================

let editingId = null;        // null = thêm mới, có số = sửa
let currentPage = 1;
const PAGE_SIZE = 10;
let departments = [];        // cache danh sách phòng ban

if (requireAuth()) {
  init();
}

async function init() {
  // Load phòng ban cho dropdown lọc và select trong modal
  departments = await api.get("/Departments").catch(() => []);
  fillDeptFilter();
  fillDeptSelect();
  loadEmployees();
}

// ---- Load danh sách nhân viên (phân trang + tìm kiếm + lọc) ----
async function loadEmployees() {
  const tbody = document.getElementById("empBody");
  const search = document.getElementById("searchInput").value.trim();
  const deptId = document.getElementById("deptFilter").value;

  const params = new URLSearchParams({ page: currentPage, pageSize: PAGE_SIZE });
  if (search) params.set("search", search);
  if (deptId) params.set("departmentId", deptId);

  try {
    const data = await api.get(`/Employees?${params}`);
    renderEmployees(data.items);
    renderPagination(data.totalCount, data.page, data.totalPages);
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="8" class="loading">Không tải được dữ liệu: ${err.message}</td></tr>`;
  }
}

function renderEmployees(employees) {
  const tbody = document.getElementById("empBody");

  if (!employees.length) {
    tbody.innerHTML = `<tr><td colspan="8" class="loading">Không có nhân viên nào</td></tr>`;
    return;
  }

  tbody.innerHTML = employees
    .map((e) => {
      const avatarUrl = e.avatar
        ? e.avatar.startsWith("http")
          ? e.avatar
          : new URL(API_URL, window.location.href).origin + e.avatar
        : null;
      return `
      <tr>
        <td>${e.id}</td>
        <td>${
          avatarUrl
            ? `<img class="avatar" src="${avatarUrl}" onerror="this.style.display='none'">`
            : `<div class="avatar" style="background:#e5e7eb;display:flex;align-items:center;justify-content:center;color:#9ca3af;">—</div>`
        }</td>
        <td>${e.fullName}</td>
        <td>${e.email}</td>
        <td>${e.phone || "—"}</td>
        <td>${e.departmentName}</td>
        <td class="text-right">${formatVND(e.salary)}</td>
        <td>
          <button class="btn btn-sm btn-ghost" onclick='openEditModal(${JSON.stringify(e)})'>Sửa</button>
          <button class="btn btn-sm btn-danger" onclick="deleteEmployee(${e.id}, '${e.fullName.replace(/'/g, "\\'")}')">Xóa</button>
        </td>
      </tr>`;
    })
    .join("");
}

// ---- Phân trang ----
function renderPagination(totalCount, page, totalPages) {
  const box = document.getElementById("pagination");
  if (totalPages <= 1) {
    box.innerHTML = "";
    return;
  }

  let html = `<button ${page === 1 ? "disabled" : ""} onclick="goPage(${page - 1})">‹</button>`;

  for (let i = 1; i <= totalPages; i++) {
    html += `<button class="${i === page ? "active" : ""}" onclick="goPage(${i})">${i}</button>`;
  }

  html += `<button ${page === totalPages ? "disabled" : ""} onclick="goPage(${page + 1})">›</button>`;
  box.innerHTML = html;
}

function goPage(page) {
  currentPage = page;
  loadEmployees();
}

// Tìm kiếm / lọc khi gõ hoặc đổi select (debounce 300ms)
let searchTimer;
document.getElementById("searchInput").addEventListener("input", () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    currentPage = 1;
    loadEmployees();
  }, 300);
});

document.getElementById("deptFilter").addEventListener("change", () => {
  currentPage = 1;
  loadEmployees();
});

// ---- Dropdown phòng ban ----
function fillDeptFilter() {
  const select = document.getElementById("deptFilter");
  select.innerHTML =
    `<option value="">Tất cả phòng ban</option>` +
    departments.map((d) => `<option value="${d.id}">${d.name}</option>`).join("");
}

function fillDeptSelect() {
  document.getElementById("deptSelect").innerHTML = departments
    .map((d) => `<option value="${d.id}">${d.name}</option>`)
    .join("");
}

// ---- Modal ----
function openCreateModal() {
  editingId = null;
  document.getElementById("modalTitle").textContent = "Thêm nhân viên";
  ["fullName", "email", "phone", "salary", "avatarFile"].forEach((id) => (document.getElementById(id).value = ""));
  document.getElementById("deptSelect").value = departments[0]?.id || "";
  document.getElementById("modalOverlay").classList.add("open");
}

function openEditModal(emp) {
  editingId = emp.id;
  document.getElementById("modalTitle").textContent = "Sửa nhân viên";
  document.getElementById("fullName").value = emp.fullName;
  document.getElementById("email").value = emp.email;
  document.getElementById("phone").value = emp.phone || "";
  document.getElementById("salary").value = emp.salary;
  document.getElementById("avatarFile").value = "";

  // Chọn đúng phòng ban của nhân viên
  const dept = departments.find((d) => d.name === emp.departmentName);
  document.getElementById("deptSelect").value = dept ? dept.id : departments[0]?.id || "";

  document.getElementById("modalOverlay").classList.add("open");
}

function closeModal() {
  document.getElementById("modalOverlay").classList.remove("open");
}

// ---- Lưu (tạo mới / cập nhật + upload avatar) ----
document.getElementById("saveBtn").addEventListener("click", async () => {
  const fullName = document.getElementById("fullName").value.trim();
  const email = document.getElementById("email").value.trim();
  const phone = document.getElementById("phone").value.trim();
  const salary = document.getElementById("salary").value;
  const departmentId = document.getElementById("deptSelect").value;
  const avatarFile = document.getElementById("avatarFile").files[0];

  if (!fullName || !email || !salary || !departmentId) {
    return alert("Vui lòng điền đầy đủ: họ tên, email, lương, phòng ban");
  }

  const dto = { fullName, email, phone, salary: Number(salary), departmentId: Number(departmentId) };

  try {
    let empId;

    if (editingId === null) {
      const created = await api.post("/Employees", dto);
      empId = created.id;
      showAlert("Thêm nhân viên thành công!", "success");
    } else {
      await api.put(`/Employees/${editingId}`, { id: editingId, ...dto });
      empId = editingId;
      showAlert("Cập nhật thành công!", "success");
    }

    // Upload avatar nếu có chọn file
    if (avatarFile && empId) {
      const formData = new FormData();
      formData.append("file", avatarFile);
      await api.upload(`/Employees/${empId}/avatar`, formData);
    }

    closeModal();
    loadEmployees();
  } catch (err) {
    showAlert(err.message, "error");
  }
});

// ---- Xóa ----
async function deleteEmployee(id, fullName) {
  if (!confirm(`Bạn có chắc muốn xóa nhân viên "${fullName}"?`)) return;

  try {
    await api.delete(`/Employees/${id}`);
    showAlert("Xóa thành công!", "success");
    loadEmployees();
  } catch (err) {
    showAlert(err.message, "error");
  }
}

function showAlert(message, type) {
  const box = document.getElementById("alertBox");
  box.textContent = message;
  box.className = `alert show alert-${type}`;
  setTimeout(() => box.classList.remove("show"), 3000);
}
