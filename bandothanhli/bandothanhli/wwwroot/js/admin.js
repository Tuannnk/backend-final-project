(() => {
  const moneyFormatter = new Intl.NumberFormat("vi-VN");

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  function formatMoney(value) {
    const num = Number(value ?? 0);
    if (!Number.isFinite(num)) return "0";
    return moneyFormatter.format(num);
  }

  function shortId(value) {
    const s = String(value ?? "");
    if (s.length <= 8) return s;
    return `${s.slice(0, 4)}…${s.slice(-4)}`;
  }

  function readToken() {
    return localStorage.getItem("token") || "";
  }

  function readUser() {
    try {
      const raw = localStorage.getItem("nguoiDung");
      if (!raw) return null;
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  async function api(url, init) {
    const token = readToken();
    const headers = new Headers(init?.headers || {});
    headers.set("Accept", "application/json");
    if (token) headers.set("Authorization", `Bearer ${token}`);

    const res = await fetch(url, { ...init, headers, credentials: "same-origin" });
    const isJson = (res.headers.get("content-type") || "").includes("application/json");
    const data = isJson ? await res.json().catch(() => null) : null;
    if (!res.ok) {
      throw new Error(data?.message || `Request failed (${res.status})`);
    }
    return data;
  }

  function setText(id, value) {
    const el = document.getElementById(id);
    if (el) el.textContent = value ?? "";
  }

  function showAlert(id, message) {
    const el = document.getElementById(id);
    if (!el) return;
    if (message) {
      el.textContent = message;
      el.classList.remove("d-none");
    } else {
      el.textContent = "";
      el.classList.add("d-none");
    }
  }

  function renderPagination(ul, page, pageSize, total, onPage) {
    if (!ul) return;
    ul.innerHTML = "";
    const totalPages = Math.max(1, Math.ceil((total || 0) / (pageSize || 10)));

    function add(label, p, disabled, active) {
      const li = document.createElement("li");
      li.className = `page-item ${disabled ? "disabled" : ""} ${active ? "active" : ""}`.trim();
      const a = document.createElement("a");
      a.className = "page-link";
      a.href = "#";
      a.textContent = label;
      a.addEventListener("click", (e) => {
        e.preventDefault();
        if (disabled || active) return;
        onPage(p);
      });
      li.appendChild(a);
      ul.appendChild(li);
    }

    add("«", Math.max(1, page - 1), page <= 1, false);

    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    for (let p = start; p <= end; p++) {
      add(String(p), p, false, p === page);
    }

    add("»", Math.min(totalPages, page + 1), page >= totalPages, false);
  }

  function mapOrderStatus(status) {
    const s = String(status || "");
    if (s === "cho_xac_nhan") return "Chờ xác nhận";
    if (s === "dang_giao") return "Đang giao";
    if (s === "da_giao") return "Đã giao";
    if (s === "hoan_thanh") return "Hoàn thành";
    if (s === "da_huy") return "Đã hủy";
    return s || "--";
  }

  async function initCommon() {
    const page = window.__ADMIN_PAGE__ || "";
    document.querySelectorAll(".adm-nav-link[data-nav]").forEach((a) => {
      if (a.getAttribute("data-nav") === page) a.classList.add("is-active");
    });

    const logoutBtn = document.getElementById("admLogoutBtn");
    if (logoutBtn) {
      logoutBtn.addEventListener("click", async () => {
        try {
          await fetch("/api/auth/dang-xuat", { method: "POST", credentials: "same-origin" });
        } catch {}
        localStorage.removeItem("token");
        localStorage.removeItem("nguoiDung");
        window.location.href = "/auth/dang-nhap";
      });
    }

    // Prefer server profile (works with cookie token), fallback to localStorage
    const userLabel = document.getElementById("admUserName");
    const roleBadge = document.getElementById("admRoleBadge");

    try {
      const profile = await api("/api/nguoi-dung/ho-so");
      if (userLabel) userLabel.textContent = profile?.hoTen ? `👤 ${profile.hoTen}` : "";
      if (roleBadge) {
        roleBadge.textContent = profile?.vaiTro || "";
        roleBadge.style.display = profile?.vaiTro ? "" : "none";
      }
    } catch {
      const u = readUser();
      if (userLabel) userLabel.textContent = u?.hoTen ? `👤 ${u.hoTen}` : "";
      if (roleBadge) {
        const role = u?.vaiTro || "";
        roleBadge.textContent = role;
        roleBadge.style.display = role ? "" : "none";
      }
    }
  }

  async function initDashboard() {
    const now = new Date();
    const month = now.getMonth() + 1;
    const year = now.getFullYear();
    setText("admMonthLabel", `${month}/${year}`);
    try {
      const data = await api(`/api/admin/thong-ke/tong-quan?nam=${year}&thang=${month}`);
      setText("admTotalUsers", String(data?.tongNguoiDung ?? "--"));
      setText("admTotalOrders", String(data?.tongDonHang ?? "--"));
      setText("admUsersThisMonth", String(data?.nguoiDungThangNay ?? "--"));
      setText("admOrdersThisMonth", String(data?.donHangThangNay ?? "--"));
      setText("admRevenueThisMonth", `${formatMoney(data?.doanhThuThangNay ?? 0)} đ`);

      const body = document.getElementById("admTopCategoriesBody");
      if (body) {
        const rows = Array.isArray(data?.topDanhMuc) ? data.topDanhMuc : [];
        if (!rows.length) {
          body.innerHTML = `<tr><td colspan="3" class="text-muted">Chưa có dữ liệu</td></tr>`;
        } else {
          body.innerHTML = rows
            .map(
              (x) => `
              <tr>
                <td>${escapeHtml(x.tenDanhMuc || "")}</td>
                <td class="text-end">${escapeHtml(String(x.soLuongBan ?? 0))}</td>
                <td class="text-end">${escapeHtml(formatMoney(x.doanhThu ?? 0))} đ</td>
              </tr>
            `,
            )
            .join("");
        }
      }
    } catch (err) {
      setText("admRevenueThisMonth", "--");
      const body = document.getElementById("admTopCategoriesBody");
      if (body) body.innerHTML = `<tr><td colspan="3" class="text-danger">${escapeHtml(err?.message || "Lỗi tải dữ liệu")}</td></tr>`;
    }
  }

  async function initUsers() {
    const els = {
      body: document.getElementById("admUsersBody"),
      error: "admUsersError",
      meta: document.getElementById("admUsersMeta"),
      pagination: document.getElementById("admUsersPagination"),
      refresh: document.getElementById("admUsersRefreshBtn"),
      pageSize: document.getElementById("admUsersPageSize"),
    };

    const state = { page: 1, pageSize: Number(els.pageSize?.value || 10), total: 0, data: [] };

    async function load() {
      showAlert(els.error, "");
      try {
        const res = await api(`/api/nguoi-dung?trang=${state.page}&soLuong=${state.pageSize}`);
        state.total = res?.total ?? 0;
        state.data = res?.data ?? [];
        render();
      } catch (err) {
        showAlert(els.error, err?.message || "Không tải được người dùng");
      }
    }

    function render() {
      if (!els.body) return;
      const rows = Array.isArray(state.data) ? state.data : [];
      els.body.innerHTML = rows
        .map((u) => {
          const role = u?.vaiTro || "";
          const isAdmin = role === "admin";
          return `
            <tr data-id="${escapeHtml(u.id)}" data-role="${escapeHtml(role)}">
              <td>${escapeHtml(u.hoTen || "")}</td>
              <td class="text-muted">${escapeHtml(u.email || "")}</td>
              <td>${escapeHtml(u.soDienThoai || "")}</td>
              <td><span class="badge text-bg-light border">${escapeHtml(role || "--")}</span></td>
              <td class="text-center">${u.daXacThuc ? "✅" : "⛔"}</td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-secondary" type="button" data-action="toggle">${u.daXacThuc ? "Khóa" : "Mở khóa"}</button>
                <button class="btn btn-sm btn-outline-danger" type="button" data-action="delete" ${isAdmin ? "disabled" : ""}>Xóa</button>
              </td>
            </tr>
          `;
        })
        .join("");

      if (els.meta) {
        const totalPages = Math.max(1, Math.ceil((state.total || 0) / state.pageSize));
        els.meta.textContent = `Trang ${state.page}/${totalPages} • Tổng ${state.total}`;
      }

      renderPagination(els.pagination, state.page, state.pageSize, state.total, (p) => {
        state.page = p;
        load();
      });
    }

    els.body?.addEventListener("click", async (e) => {
      const btn = e.target?.closest?.("button[data-action]");
      const tr = e.target?.closest?.("tr[data-id]");
      if (!btn || !tr) return;
      const id = tr.getAttribute("data-id");
      const action = btn.getAttribute("data-action");
      const role = tr.getAttribute("data-role");

      showAlert(els.error, "");
      try {
        if (action === "toggle") {
          await api(`/api/nguoi-dung/${id}/khoa`, { method: "PUT" });
          await load();
        } else if (action === "delete") {
          if (role === "admin") return;
          if (!confirm("Xóa người dùng này?")) return;
          await api(`/api/nguoi-dung/${id}`, { method: "DELETE" });
          await load();
        }
      } catch (err) {
        showAlert(els.error, err?.message || "Thao tác thất bại");
      }
    });

    els.refresh?.addEventListener("click", () => load());
    els.pageSize?.addEventListener("change", () => {
      state.pageSize = Number(els.pageSize.value || 10);
      state.page = 1;
      load();
    });

    await load();
  }

  async function initOrders() {
    const els = {
      body: document.getElementById("admOrdersBody"),
      error: "admOrdersError",
      meta: document.getElementById("admOrdersMeta"),
      pagination: document.getElementById("admOrdersPagination"),
      refresh: document.getElementById("admOrdersRefreshBtn"),
      pageSize: document.getElementById("admOrdersPageSize"),
      status: document.getElementById("admOrdersStatus"),
    };

    const state = {
      page: 1,
      pageSize: Number(els.pageSize?.value || 10),
      total: 0,
      data: [],
      status: els.status?.value || "",
    };

    async function load() {
      showAlert(els.error, "");
      try {
        const qs = new URLSearchParams();
        if (state.status) qs.set("trangThai", state.status);
        qs.set("trang", String(state.page));
        qs.set("soLuong", String(state.pageSize));
        const res = await api(`/api/don-hang?${qs.toString()}`);
        state.total = res?.total ?? 0;
        state.data = res?.data ?? [];
        render();
      } catch (err) {
        showAlert(els.error, err?.message || "Không tải được đơn hàng");
      }
    }

    function renderActions(o) {
      const st = String(o?.trangThai || "");
      if (st === "cho_xac_nhan") return `<button class="btn btn-sm btn-outline-primary" type="button" data-action="xac-nhan">Xác nhận</button>`;
      if (st === "dang_giao") return `<button class="btn btn-sm btn-outline-primary" type="button" data-action="dang-giao">Cập nhật</button>`;
      if (st === "da_giao") return `<button class="btn btn-sm btn-outline-primary" type="button" data-action="da-giao">Hoàn thành</button>`;
      return `<span class="text-muted">--</span>`;
    }

    function render() {
      if (!els.body) return;
      const rows = Array.isArray(state.data) ? state.data : [];
      els.body.innerHTML = rows
        .map(
          (o) => `
          <tr data-id="${escapeHtml(o.id)}">
            <td><span class="font-monospace">${escapeHtml(shortId(o.id))}</span></td>
            <td>${escapeHtml(o.nguoiMua || "")}</td>
            <td>${escapeHtml(mapOrderStatus(o.trangThai))}</td>
            <td class="text-end">${escapeHtml(formatMoney(o.tongTien))} đ</td>
            <td class="text-muted">${escapeHtml(o.ngayTao ? new Date(o.ngayTao).toLocaleString("vi-VN") : "")}</td>
            <td class="text-end">${renderActions(o)}</td>
          </tr>
        `,
        )
        .join("");

      if (els.meta) {
        const totalPages = Math.max(1, Math.ceil((state.total || 0) / state.pageSize));
        els.meta.textContent = `Trang ${state.page}/${totalPages} • Tổng ${state.total}`;
      }

      renderPagination(els.pagination, state.page, state.pageSize, state.total, (p) => {
        state.page = p;
        load();
      });
    }

    els.body?.addEventListener("click", async (e) => {
      const btn = e.target?.closest?.("button[data-action]");
      const tr = e.target?.closest?.("tr[data-id]");
      if (!btn || !tr) return;
      const id = tr.getAttribute("data-id");
      const action = btn.getAttribute("data-action");
      showAlert(els.error, "");

      try {
        if (action === "xac-nhan") await api(`/api/don-hang/${id}/xac-nhan`, { method: "PUT" });
        if (action === "dang-giao") await api(`/api/don-hang/${id}/dang-giao`, { method: "PUT" });
        if (action === "da-giao") await api(`/api/don-hang/${id}/da-giao`, { method: "PUT" });
        await load();
      } catch (err) {
        showAlert(els.error, err?.message || "Thao tác thất bại");
      }
    });

    els.refresh?.addEventListener("click", () => load());
    els.pageSize?.addEventListener("change", () => {
      state.pageSize = Number(els.pageSize.value || 10);
      state.page = 1;
      load();
    });
    els.status?.addEventListener("change", () => {
      state.status = els.status.value || "";
      state.page = 1;
      load();
    });

    await load();
  }

  async function initCategories() {
    const els = {
      body: document.getElementById("admCatBody"),
      error: "admCatError",
      refresh: document.getElementById("admCatRefreshBtn"),
      add: document.getElementById("admCatAddBtn"),
      modal: document.getElementById("admCatModal"),
      modalTitle: document.getElementById("admCatModalTitle"),
      modalError: "admCatModalError",
      ten: document.getElementById("admCatTen"),
      slug: document.getElementById("admCatSlug"),
      icon: document.getElementById("admCatIcon"),
      thuTu: document.getElementById("admCatThuTu"),
      parent: document.getElementById("admCatParent"),
      save: document.getElementById("admCatSaveBtn"),
    };

    const state = { data: [], editingId: null, modalInstance: null };

    function openModal(title, cat) {
      showAlert(els.modalError, "");
      state.editingId = cat?.id ?? null;
      if (els.modalTitle) els.modalTitle.textContent = title;
      if (els.ten) els.ten.value = cat?.tenDanhMuc ?? "";
      if (els.slug) els.slug.value = cat?.slug ?? "";
      if (els.icon) els.icon.value = cat?.iconUrl ?? "";
      if (els.thuTu) els.thuTu.value = String(cat?.thuTu ?? 0);
      if (els.parent) els.parent.value = cat?.danhMucChaId ?? "";

      if (!state.modalInstance && els.modal && window.bootstrap?.Modal) {
        state.modalInstance = new window.bootstrap.Modal(els.modal);
      }
      state.modalInstance?.show();
    }

    function validateModal() {
      const tenDanhMuc = (els.ten?.value || "").trim();
      const slug = (els.slug?.value || "").trim();
      const iconUrl = (els.icon?.value || "").trim();
      const thuTu = Number(String(els.thuTu?.value || "0").replace(/[^\d-]/g, "")) || 0;
      const danhMucChaId = (els.parent?.value || "").trim() || null;

      if (!tenDanhMuc) throw new Error("Vui lòng nhập tên danh mục");
      if (!slug) throw new Error("Vui lòng nhập slug");

      return { tenDanhMuc, slug, iconUrl, thuTu, danhMucChaId };
    }

    function flattenCategories() {
      const rows = [];
      (state.data || []).forEach((p) => {
        rows.push({ ...p, __level: 0 });
        (p.danhMucCon || []).forEach((c) => rows.push({ ...c, danhMucChaId: p.id, __level: 1 }));
      });
      return rows;
    }

    function render() {
      if (!els.body) return;
      const flat = flattenCategories();
      els.body.innerHTML = flat
        .map((c) => {
          const pad = c.__level ? "&nbsp;".repeat(6) + "↳ " : "";
          const icon = c.iconUrl ? `<a href="${escapeHtml(c.iconUrl)}" target="_blank" rel="noreferrer">link</a>` : `<span class="text-muted">--</span>`;
          return `
            <tr data-id="${escapeHtml(c.id)}" data-level="${escapeHtml(String(c.__level))}">
              <td>${pad}${escapeHtml(c.tenDanhMuc || "")}</td>
              <td class="text-muted">${escapeHtml(c.slug || "")}</td>
              <td class="text-center">${escapeHtml(String(c.thuTu ?? 0))}</td>
              <td>${icon}</td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-secondary" type="button" data-action="edit">Sửa</button>
                <button class="btn btn-sm btn-outline-danger" type="button" data-action="delete">Xóa</button>
              </td>
            </tr>
          `;
        })
        .join("");

      // Parent options (only root)
      if (els.parent) {
        const current = els.parent.value || "";
        const roots = Array.isArray(state.data) ? state.data : [];
        els.parent.innerHTML = `<option value="">(Không)</option>` + roots.map((r) => `<option value="${escapeHtml(r.id)}">${escapeHtml(r.tenDanhMuc || "")}</option>`).join("");
        els.parent.value = current;
      }
    }

    async function load() {
      showAlert(els.error, "");
      try {
        state.data = await api("/api/danh-muc");
        render();
      } catch (err) {
        showAlert(els.error, err?.message || "Không tải được danh mục");
      }
    }

    function findById(id) {
      const flat = flattenCategories();
      return flat.find((x) => String(x.id) === String(id)) || null;
    }

    els.add?.addEventListener("click", () => openModal("Thêm danh mục", null));
    els.refresh?.addEventListener("click", () => load());

    els.body?.addEventListener("click", async (e) => {
      const btn = e.target?.closest?.("button[data-action]");
      const tr = e.target?.closest?.("tr[data-id]");
      if (!btn || !tr) return;
      const id = tr.getAttribute("data-id");
      const action = btn.getAttribute("data-action");

      showAlert(els.error, "");
      try {
        if (action === "edit") {
          const cat = await api(`/api/danh-muc/${id}`);
          openModal("Sửa danh mục", cat);
        } else if (action === "delete") {
          if (!confirm("Xóa danh mục này?")) return;
          await api(`/api/danh-muc/${id}`, { method: "DELETE" });
          await load();
        }
      } catch (err) {
        showAlert(els.error, err?.message || "Thao tác thất bại");
      }
    });

    els.save?.addEventListener("click", async () => {
      showAlert(els.modalError, "");
      els.save.disabled = true;
      els.save.textContent = "Đang lưu...";
      try {
        const dto = validateModal();
        if (state.editingId) {
          await api(`/api/danh-muc/${state.editingId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
          });
        } else {
          await api("/api/danh-muc", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
          });
        }
        state.modalInstance?.hide();
        await load();
      } catch (err) {
        showAlert(els.modalError, err?.message || "Lưu thất bại");
      } finally {
        els.save.disabled = false;
        els.save.textContent = "Lưu";
      }
    });

    await load();
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await initCommon();
    const page = window.__ADMIN_PAGE__ || "";
    if (page === "dashboard") await initDashboard();
    if (page === "users") await initUsers();
    if (page === "orders") await initOrders();
    if (page === "categories") await initCategories();
  });
})();

