// =============================================
// dashboard.js — Hiển thị thống kê
// =============================================

if (requireAuth()) {
  loadDashboard();
}

async function loadDashboard() {
  try {
    const data = await api.get("/Dashboard");

    document.getElementById("statEmployees").textContent = data.totalEmployees;
    document.getElementById("statDepartments").textContent = data.totalDepartments;
    document.getElementById("statSalary").textContent = formatVND(data.totalSalary);

    renderDeptTable(data.employeesByDepartment);
  } catch (err) {
    document.getElementById("deptTableBody").innerHTML =
      `<tr><td colspan="3" class="loading">Không tải được dữ liệu: ${err.message}</td></tr>`;
  }
}

// Vẽ bảng số nhân viên theo phòng ban kèm thanh biểu đồ
function renderDeptTable(departments) {
  const tbody = document.getElementById("deptTableBody");
  if (!departments.length) {
    tbody.innerHTML = `<tr><td colspan="3" class="loading">Chưa có dữ liệu</td></tr>`;
    return;
  }

  const maxCount = Math.max(...departments.map((d) => d.count), 1);

  tbody.innerHTML = departments
    .map(
      (d) => `
      <tr>
        <td>${d.departmentName}</td>
        <td>
          <div style="display: flex; align-items: center; gap: 8px;">
            <div style="height: 20px; border-radius: 4px; background: #1677ff; width: ${Math.max(
              (d.count / maxCount) * 100,
              2
            )}%;"></div>
            ${d.count}
          </div>
        </td>
        <td>${formatVND(d.totalSalary)}</td>
      </tr>`
    )
    .join("");
}
