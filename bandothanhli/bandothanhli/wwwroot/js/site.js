// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  const moneyFormatter = new Intl.NumberFormat("vi-VN");

  function getQuery() {
    return new URLSearchParams(window.location.search);
  }

  function setQuery(next) {
    const url = new URL(window.location.href);
    url.search = next.toString();
    window.location.href = url.toString();
  }

  function normalizeNumber(value) {
    if (value == null) return null;
    const raw = String(value).replace(/[^\d]/g, "");
    if (!raw) return null;
    const parsed = Number(raw);
    return Number.isFinite(parsed) ? parsed : null;
  }

  function formatMoney(value) {
    if (value == null) return "";
    const num = Number(value);
    if (!Number.isFinite(num)) return "";
    return moneyFormatter.format(num);
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
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

  function readToken() {
    return localStorage.getItem("token") || "";
  }

  function readCart() {
    try {
      const raw = localStorage.getItem("cart");
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  function saveCart(items) {
    localStorage.setItem("cart", JSON.stringify(items || []));
  }

  function addToCart(productId, qty = 1) {
    const items = readCart();
    const found = items.find((x) => String(x.productId) === String(productId));
    if (found) {
      found.qty = Math.min(99, Number(found.qty || 1) + Number(qty || 1));
    } else {
      items.push({ productId, qty: Math.max(1, Number(qty || 1)), selected: true });
    }
    saveCart(items);
  }

  async function fetchJson(url) {
    const res = await fetch(url, { headers: { Accept: "application/json" } });
    if (!res.ok) {
      let message = "Không tải được dữ liệu";
      try {
        const err = await res.json();
        if (err?.message) message = err.message;
      } catch {}
      throw new Error(message);
    }
    return res.json();
  }

  async function fetchJsonAuth(url, init) {
    const token = readToken();
    const headers = new Headers(init?.headers || {});
    headers.set("Accept", "application/json");
    if (token) headers.set("Authorization", `Bearer ${token}`);
    const res = await fetch(url, { ...init, headers });
    const isJson = (res.headers.get("content-type") || "").includes("application/json");

    if (!res.ok) {
      let message = "Có lỗi xảy ra";
      if (isJson) {
        try {
          const err = await res.json();
          if (err?.message) message = err.message;
        } catch {}
      }
      throw new Error(message);
    }

    return isJson ? res.json() : null;
  }

  function buildCategoryHref(categoryId) {
    const q = getQuery();
    if (categoryId) q.set("danhMucId", categoryId);
    else q.delete("danhMucId");
    q.set("trang", "1");
    return `${window.location.pathname}?${q.toString()}`;
  }

  function renderCategoryLink(cat, activeId, extraClassName = "") {
    const isActive = activeId && String(activeId) === String(cat.id);
    const label = escapeHtml(cat.tenDanhMuc ?? "");
    const icon = cat.iconUrl
      ? `<img alt="" src="${escapeHtml(cat.iconUrl)}" style="width:16px;height:16px;border-radius:4px;" />`
      : "📦";
    return `
      <a class="eshop-cat ${extraClassName} ${isActive ? "is-active" : ""}" href="${escapeHtml(cat.href)}" data-id="${escapeHtml(cat.id)}">
        <span>${icon}</span>
        <span>${label}</span>
      </a>
    `;
  }

  async function initHeader() {
    const loginBtn = document.getElementById("headerLoginBtn");
    const userDropdown = document.getElementById("headerUserDropdown");
    const userName = document.getElementById("headerUserName");
    const logoutBtn = document.getElementById("headerLogoutBtn");

    const user = readUser();
    if (user && userDropdown && userName && loginBtn) {
      loginBtn.style.display = "none";
      userDropdown.style.display = "";
      userName.textContent = user.hoTen ?? "Tài khoản";
    }

    if (logoutBtn) {
      logoutBtn.addEventListener("click", (e) => {
        e.preventDefault();
        localStorage.removeItem("token");
        localStorage.removeItem("nguoiDung");
        window.location.href = "/";
      });
    }

    const searchForm = document.getElementById("globalSearchForm");
    const searchInput = document.getElementById("globalSearchInput");
    const q = getQuery();
    const tuKhoa = q.get("tuKhoa") ?? "";
    if (searchInput) searchInput.value = tuKhoa;

    if (searchForm && searchInput) {
      searchForm.addEventListener("submit", (e) => {
        e.preventDefault();
        const next = getQuery();
        const keyword = searchInput.value.trim();
        if (keyword) next.set("tuKhoa", keyword);
        else next.delete("tuKhoa");
        next.set("trang", "1");
        setQuery(next);
      });
    }

    const headerCategoryNav = document.getElementById("headerCategoryNav");
    if (headerCategoryNav) {
      try {
        const cats = await fetchJson("/api/danh-muc");
        const active = q.get("danhMucId");
        const links = [
          renderCategoryLink({ id: "", tenDanhMuc: "Tất cả", iconUrl: "", href: buildCategoryHref("") }, active, "eshop-cat-icononly"),
          ...cats.map((c) =>
            renderCategoryLink(
              { id: c.id, tenDanhMuc: c.tenDanhMuc, iconUrl: c.iconUrl, href: buildCategoryHref(c.id) },
              active,
              "eshop-cat-icononly",
            ),
          ),
        ];
        headerCategoryNav.innerHTML = links.join("");
      } catch (err) {
        headerCategoryNav.innerHTML = `<span class="text-muted small">Không tải được danh mục</span>`;
        console.error(err);
      }
    }
  }

  function productCard(item) {
    const title = escapeHtml(item.tieuDe ?? "");
    const location = escapeHtml(item.diaDiem ?? "");
    const condition = escapeHtml(item.tinhTrang ?? "");
    const category = escapeHtml(item.danhMuc ?? "");
    const price = formatMoney(item.gia);
    const oldPrice = item.giaGoc && Number(item.giaGoc) > Number(item.gia) ? formatMoney(item.giaGoc) : "";
    const rating = Number(item.diemDanhGia ?? 0);
    const ratingCount = Number(item.soDanhGia ?? 0);
    const href = `/san-pham/${escapeHtml(item.id)}`;
    const thumb = item.anhDaiDien
      ? `<img alt="${title}" loading="lazy" src="${escapeHtml(item.anhDaiDien)}" />`
      : `<div class="eshop-thumb-fallback">No Image</div>`;

    const stars = renderStars(rating);

    return `
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="eshop-card eshop-card-link" data-href="${href}">
          <div class="eshop-thumb">${thumb}</div>
          <div class="eshop-card-body">
            <div class="eshop-card-title" title="${title}">${title}</div>
            <div class="d-flex align-items-baseline gap-2">
              <div class="eshop-price-main">${price} đ</div>
              ${oldPrice ? `<div class="eshop-price-old">${oldPrice} đ</div>` : ""}
            </div>
            <div class="d-flex align-items-center justify-content-between mt-2">
              <div class="eshop-stars" aria-label="Rating">${stars}</div>
              <div class="text-muted small">${ratingCount ? `(${ratingCount})` : ""}</div>
            </div>
            <div class="eshop-meta">
              ${category ? `<span>🏷️ ${category}</span>` : ""}
              ${condition ? `<span>⭐ ${condition}</span>` : ""}
              ${location ? `<span>📍 ${location}</span>` : ""}
            </div>
            <div class="eshop-actions">
              <button class="eshop-action-btn" type="button" data-action="like">♡ Yêu thích</button>
              <button class="eshop-action-btn" type="button" data-action="chat">💬 Nhắn tin</button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  function topRatedCard(item) {
    const title = escapeHtml(item.tieuDe ?? "");
    const location = escapeHtml(item.diaDiem ?? "");
    const price = formatMoney(item.gia);
    const rating = Number(item.diemDanhGia ?? 0);
    const ratingCount = Number(item.soDanhGia ?? 0);
    const href = `/san-pham/${escapeHtml(item.id)}`;
    const thumb = item.anhDaiDien
      ? `<img alt="${title}" loading="lazy" src="${escapeHtml(item.anhDaiDien)}" />`
      : `<div class="eshop-thumb-fallback">No Image</div>`;

    return `
      <div class="eshop-card eshop-card-link" data-href="${href}">
        <div class="eshop-thumb">${thumb}</div>
        <div class="eshop-card-body">
          <div class="eshop-card-title" title="${title}">${title}</div>
          <div class="d-flex align-items-baseline gap-2">
            <div class="eshop-price-main">${price} đ</div>
          </div>
          <div class="d-flex align-items-center gap-2 mt-2">
            <div class="eshop-stars" aria-label="Rating">${renderStars(rating)}</div>
            <div class="text-muted small">${ratingCount ? `${rating.toFixed(1)} (${ratingCount})` : ""}</div>
          </div>
          <div class="eshop-meta">
            ${location ? `<span>📍 ${location}</span>` : ""}
          </div>
        </div>
      </div>
    `;
  }

  function skeletonCard() {
    return `
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="eshop-skeleton">
          <div class="eshop-thumb"></div>
          <div class="eshop-card-body">
            <div></div>
            <div></div>
          </div>
        </div>
      </div>
    `;
  }

  function renderStars(rating) {
    const normalized = Math.max(0, Math.min(5, rating || 0));
    const on = Math.round(normalized);
    const parts = [];
    for (let i = 1; i <= 5; i++) {
      parts.push(`<span class="${i <= on ? "is-on" : ""}">★</span>`);
    }
    return parts.join("");
  }

  function renderPagination(el, trang, total, soLuong) {
    if (!el) return;
    const totalPages = Math.max(1, Math.ceil((total ?? 0) / (soLuong ?? 1)));
    const current = Math.min(Math.max(1, trang ?? 1), totalPages);

    const q = getQuery();
    function pageHref(p) {
      const next = new URLSearchParams(q);
      next.set("trang", String(p));
      return `${window.location.pathname}?${next.toString()}`;
    }

    const parts = [];
    const addItem = (label, href, disabled, active) => {
      parts.push(`
        <li class="page-item ${disabled ? "disabled" : ""} ${active ? "active" : ""}">
          <a class="page-link" href="${href ?? "#"}">${label}</a>
        </li>
      `);
    };

    addItem("«", pageHref(Math.max(1, current - 1)), current === 1, false);

    const start = Math.max(1, current - 2);
    const end = Math.min(totalPages, current + 2);
    for (let p = start; p <= end; p++) addItem(String(p), pageHref(p), false, p === current);

    addItem("»", pageHref(Math.min(totalPages, current + 1)), current === totalPages, false);

    el.innerHTML = parts.join("");
  }

  function toDateText(value) {
    if (!value) return "";
    try {
      return new Date(value).toLocaleString("vi-VN");
    } catch {
      return "";
    }
  }

  function statusText(status) {
    switch (status) {
      case "cho_xac_nhan":
        return "Chờ xác nhận";
      case "dang_giao":
        return "Đang giao";
      case "da_giao":
        return "Đã giao";
      case "hoan_thanh":
        return "Hoàn thành";
      case "da_huy":
        return "Đã hủy";
      default:
        return status || "—";
    }
  }

  function paymentText(method) {
    switch (method) {
      case "thanh_toan_khi_nhan_hang":
        return "Thanh toán khi nhận hàng";
      case "chuyen_khoan":
        return "Chuyển khoản";
      default:
        return method || "—";
    }
  }

  function paymentStatusText(status) {
    switch (status) {
      case "cho_thanh_toan":
        return "Chờ thanh toán";
      case "da_thanh_toan":
        return "Đã thanh toán";
      default:
        return status || "";
    }
  }

  function orderCardBuyer(o, highlightId) {
    const id = escapeHtml(o.id);
    const isNew = highlightId && String(highlightId) === String(o.id);
    const title = escapeHtml(o.sanPhamDauTien || "Đơn hàng");
    const time = toDateText(o.ngayTao);
    const total = `${formatMoney(o.tongTien)}đ`;
    const badge = `<span class="ord-badge">${escapeHtml(statusText(o.trangThai))}</span>`;
    return `
      <a class="ord-item ${isNew ? "is-new" : ""}" href="/don-hang/${id}">
        <div class="ord-item-main">
          <div class="d-flex align-items-center gap-2">
            <div class="fw-bold">${title}</div>
            ${badge}
          </div>
          <div class="text-muted small">${escapeHtml(time)} • ${escapeHtml(String(o.soSanPham || 0))} sản phẩm</div>
        </div>
        <div class="ord-item-right">
          <div class="fw-bold">${escapeHtml(total)}</div>
          <div class="text-muted small">Xem chi tiết</div>
        </div>
      </a>
    `;
  }

  function orderCardSeller(o, highlightId) {
    const id = escapeHtml(o.id);
    const isNew = highlightId && String(highlightId) === String(o.id);
    const title = escapeHtml(o.sanPhamDauTien || "Đơn hàng");
    const time = toDateText(o.ngayTao);
    const total = `${formatMoney(o.tongTienCuaBan)}đ`;
    const badge = `<span class="ord-badge">${escapeHtml(statusText(o.trangThai))}</span>`;
    return `
      <a class="ord-item ${isNew ? "is-new" : ""}" href="/don-hang/${id}?mode=ban">
        <div class="ord-item-main">
          <div class="d-flex align-items-center gap-2">
            <div class="fw-bold">${title}</div>
            ${badge}
          </div>
          <div class="text-muted small">${escapeHtml(time)} • ${escapeHtml(o.nguoiMua || "—")} • ${escapeHtml(String(o.soSanPhamCuaBan || 0))} sp của bạn</div>
        </div>
        <div class="ord-item-right">
          <div class="fw-bold">${escapeHtml(total)}</div>
          <div class="text-muted small">Xem chi tiết</div>
        </div>
      </a>
    `;
  }

  async function initOrdersBuyer() {
    const root = document.getElementById("ordersBuyerRoot");
    if (!root) return;

    const user = readUser();
    const token = readToken();
    if (!user || !token) {
      window.location.href = "/auth/dang-nhap";
      return;
    }

    const list = document.getElementById("ordersBuyerList");
    const errorEl = document.getElementById("ordersBuyerError");
    const pagination = document.getElementById("ordersBuyerPagination");

    const q = getQuery();
    const trang = Number(q.get("trang") ?? 1) || 1;
    const soLuong = 10;
    const highlightId = q.get("new") || "";

    try {
      if (errorEl) {
        errorEl.classList.add("d-none");
        errorEl.textContent = "";
      }

      list.innerHTML = `<div class="text-muted small">Đang tải...</div>`;
      const res = await fetchJsonAuth(`/api/don-hang/cua-toi?trang=${trang}&soLuong=${soLuong}`);
      const items = res?.data ?? [];
      if (!items.length) {
        list.innerHTML = `<div class="text-muted">Chưa có đơn hàng.</div>`;
      } else {
        list.innerHTML = items.map((o) => orderCardBuyer(o, highlightId)).join("");
      }
      renderPagination(pagination, res?.trang ?? trang, res?.total ?? 0, res?.soLuong ?? soLuong);
    } catch (err) {
      if (errorEl) {
        errorEl.textContent = err?.message ?? "Không tải được đơn mua";
        errorEl.classList.remove("d-none");
      }
      list.innerHTML = "";
      console.error(err);
    }
  }

  async function initOrdersSeller() {
    const root = document.getElementById("ordersSellerRoot");
    if (!root) return;

    const user = readUser();
    const token = readToken();
    if (!user || !token) {
      window.location.href = "/auth/dang-nhap";
      return;
    }

    const list = document.getElementById("ordersSellerList");
    const errorEl = document.getElementById("ordersSellerError");
    const pagination = document.getElementById("ordersSellerPagination");

    const q = getQuery();
    const trang = Number(q.get("trang") ?? 1) || 1;
    const soLuong = 10;

    try {
      if (errorEl) {
        errorEl.classList.add("d-none");
        errorEl.textContent = "";
      }

      list.innerHTML = `<div class="text-muted small">Đang tải...</div>`;
      const res = await fetchJsonAuth(`/api/don-hang/ban-cua-toi?trang=${trang}&soLuong=${soLuong}`);
      const items = res?.data ?? [];
      if (!items.length) {
        list.innerHTML = `<div class="text-muted">Chưa có đơn hàng.</div>`;
      } else {
        list.innerHTML = items.map((o) => orderCardSeller(o)).join("");
      }
      renderPagination(pagination, res?.trang ?? trang, res?.total ?? 0, res?.soLuong ?? soLuong);
    } catch (err) {
      if (errorEl) {
        errorEl.textContent = err?.message ?? "Không tải được đơn bán";
        errorEl.classList.remove("d-none");
      }
      list.innerHTML = "";
      console.error(err);
    }
  }

  function orderItemRow(p) {
    const title = escapeHtml(p.tieuDe || "");
    const img = p.anh ? escapeHtml(p.anh) : "";
    const unit = `${formatMoney(p.giaTaiThoiDiem)}đ`;
    const qty = escapeHtml(String(p.soLuong || 0));
    const line = `${formatMoney(p.thanhTien)}đ`;
    return `
      <div class="ord-row">
        <a class="ord-row-img" href="#" tabindex="-1">
          ${img ? `<img alt="" src="${img}" />` : `<div class="cart-img-fallback">No Image</div>`}
        </a>
        <div class="ord-row-main">
          <div class="fw-bold">${title}</div>
          <div class="text-muted small">${escapeHtml(qty)} × ${escapeHtml(unit)}</div>
        </div>
        <div class="ord-row-right fw-bold">${escapeHtml(line)}</div>
      </div>
    `;
  }

  async function initOrderDetail() {
    const root = document.getElementById("orderDetailRoot");
    if (!root) return;

    const user = readUser();
    const token = readToken();
    if (!user || !token) {
      window.location.href = "/auth/dang-nhap";
      return;
    }

    const orderId = root.getAttribute("data-id");
    if (!orderId) return;

    const itemsEl = document.getElementById("orderItems");
    const statusEl = document.getElementById("orderStatus");
    const totalEl = document.getElementById("orderTotal");
    const payEl = document.getElementById("orderPay");
    const noteEl = document.getElementById("orderNote");
    const buyerEl = document.getElementById("orderBuyer");
    const addrEl = document.getElementById("orderAddress");
    const errorEl = document.getElementById("orderDetailError");
    const cancelBtn = document.getElementById("orderCancelBtn");
    const acceptBtn = document.getElementById("orderAcceptBtn");

    const q = getQuery();
    const mode = q.get("mode") || "";
    const isSellerMode = mode === "ban";

    try {
      if (errorEl) {
        errorEl.classList.add("d-none");
        errorEl.textContent = "";
      }

      itemsEl.innerHTML = `<div class="text-muted small">Đang tải...</div>`;

      const apiUrl = isSellerMode ? `/api/don-hang/ban-cua-toi/${orderId}` : `/api/don-hang/${orderId}`;
      const data = await fetchJsonAuth(apiUrl);

      const trangThai = data.trangThai || "";
      if (statusEl) statusEl.textContent = statusText(trangThai);

      const total = isSellerMode ? data.tongTienCuaBan : data.tongTien;
      if (totalEl) totalEl.textContent = `${formatMoney(total)}đ`;

      if (payEl) {
        const method = data?.thanhToan?.phuongThuc || "";
        const payStatus = data?.thanhToan?.trangThai || "";
        payEl.textContent = payStatus ? `${paymentText(method)} • ${paymentStatusText(payStatus)}` : paymentText(method);
      }
      if (noteEl) noteEl.textContent = data.ghiChu || "—";

      if (buyerEl) buyerEl.textContent = data?.nguoiMua ? `${data.nguoiMua.hoTen} • ${data.nguoiMua.soDienThoai}` : "—";

      if (addrEl && data?.diaChi) {
        addrEl.textContent = `${data.diaChi.hoTen} • ${data.diaChi.soDienThoai} — ${data.diaChi.duongPho}, ${data.diaChi.quanHuyen}, ${data.diaChi.tinhThanh}`;
      }

      const items = data?.sanPhams || [];
      itemsEl.innerHTML = items.length ? items.map(orderItemRow).join("") : `<div class="text-muted">Không có sản phẩm.</div>`;

      if (cancelBtn) {
        if (!isSellerMode && trangThai === "cho_xac_nhan") {
          cancelBtn.classList.remove("d-none");
          cancelBtn.addEventListener("click", async () => {
            if (!confirm("Bạn chắc chắn muốn hủy đơn hàng này?")) return;
            cancelBtn.disabled = true;
            try {
              await fetchJsonAuth(`/api/don-hang/${orderId}/huy`, { method: "PUT" });
              window.location.reload();
            } catch (err) {
              if (errorEl) {
                errorEl.textContent = err?.message ?? "Hủy đơn thất bại";
                errorEl.classList.remove("d-none");
              }
            } finally {
              cancelBtn.disabled = false;
            }
          });
        } else {
          cancelBtn.classList.add("d-none");
        }
      }

      if (acceptBtn) {
        if (isSellerMode && trangThai === "cho_xac_nhan") {
          acceptBtn.classList.remove("d-none");
          acceptBtn.addEventListener("click", async () => {
            if (!confirm("Chấp nhận đơn hàng này và chuyển sang trạng thái đang giao?")) return;
            acceptBtn.disabled = true;
            acceptBtn.textContent = "Đang xử lý...";
            try {
              await fetchJsonAuth(`/api/don-hang/ban-cua-toi/${orderId}/chap-nhan`, { method: "PUT" });
              window.location.reload();
            } catch (err) {
              if (errorEl) {
                errorEl.textContent = err?.message ?? "Chấp nhận đơn thất bại";
                errorEl.classList.remove("d-none");
              }
            } finally {
              acceptBtn.disabled = false;
              acceptBtn.textContent = "Chấp nhận đơn";
            }
          });
        } else {
          acceptBtn.classList.add("d-none");
        }
      }
    } catch (err) {
      if (errorEl) {
        errorEl.textContent = err?.message ?? "Không tải được đơn hàng";
        errorEl.classList.remove("d-none");
      }
      itemsEl.innerHTML = "";
      console.error(err);
    }
  }

  async function initHome() {
    const grid = document.getElementById("homeNewProductGrid");
    const errorEl = document.getElementById("homeError");
    const pagination = document.getElementById("homePagination");
    const topRatedEl = document.getElementById("homeTopRated");
    const heroScrollBtn = document.getElementById("heroScrollBtn");

    if (!grid) return;

    const q = getQuery();
    const danhMucId = q.get("danhMucId");
    const tuKhoa = q.get("tuKhoa");
    const giaMin = q.get("giaMin");
    const giaMax = q.get("giaMax");
    const trang = Number(q.get("trang") ?? 1) || 1;
    const soLuong = 12;

    const priceMinInput = document.getElementById("priceMinInput");
    const priceMaxInput = document.getElementById("priceMaxInput");
    const applyPriceBtn = document.getElementById("applyPriceBtn");
    const clearFiltersBtn = document.getElementById("clearFiltersBtn");

    if (priceMinInput) priceMinInput.value = giaMin ?? "";
    if (priceMaxInput) priceMaxInput.value = giaMax ?? "";

    if (applyPriceBtn) {
      applyPriceBtn.addEventListener("click", () => {
        const next = getQuery();
        const min = normalizeNumber(priceMinInput?.value);
        const max = normalizeNumber(priceMaxInput?.value);
        if (min != null) next.set("giaMin", String(min));
        else next.delete("giaMin");
        if (max != null) next.set("giaMax", String(max));
        else next.delete("giaMax");
        next.set("trang", "1");
        setQuery(next);
      });
    }

    if (clearFiltersBtn) {
      clearFiltersBtn.addEventListener("click", () => {
        const next = new URLSearchParams();
        setQuery(next);
      });
    }

    if (heroScrollBtn) {
      heroScrollBtn.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
      });
    }

    grid.innerHTML = Array.from({ length: 8 }, skeletonCard).join("");
    if (errorEl) {
      errorEl.classList.add("d-none");
      errorEl.textContent = "";
    }

    try {
      const api = new URL("/api/san-pham", window.location.origin);
      if (danhMucId) api.searchParams.set("danhMucId", danhMucId);
      if (tuKhoa) api.searchParams.set("tuKhoa", tuKhoa);
      if (giaMin) api.searchParams.set("giaMin", giaMin);
      if (giaMax) api.searchParams.set("giaMax", giaMax);
      api.searchParams.set("trang", String(trang));
      api.searchParams.set("soLuong", String(soLuong));

      const result = await fetchJson(api.toString());
      const items = result?.data ?? [];

      if (!items.length) {
        grid.innerHTML = `<div class="col-12"><div class="alert alert-secondary mb-0">Chưa có sản phẩm phù hợp.</div></div>`;
      } else {
        grid.innerHTML = items.map(productCard).join("");
      }

      grid.querySelectorAll(".eshop-card-link").forEach((card) => {
        card.addEventListener("click", (e) => {
          if (e.target && (e.target.closest("button") || e.target.closest("a"))) return;
          const href = card.getAttribute("data-href");
          if (href) window.location.href = href;
        });
      });

      grid.querySelectorAll("button[data-action]").forEach((btn) => {
        btn.addEventListener("click", () => {
          const user = readUser();
          if (!user) {
            window.location.href = "/auth/dang-nhap";
            return;
          }
          alert("Tính năng đang phát triển.");
        });
      });

      renderPagination(pagination, result?.trang ?? trang, result?.total ?? 0, result?.soLuong ?? soLuong);

      if (topRatedEl) {
        topRatedEl.innerHTML = Array.from({ length: 4 })
          .map(() => `<div class="eshop-skeleton"><div class="eshop-thumb"></div><div class="eshop-card-body"><div></div><div></div></div></div>`)
          .join("");
        try {
          const topRated = await fetchJson("/api/danh-gia/top-san-pham?soLuong=4");
          topRatedEl.innerHTML = (topRated ?? []).map(topRatedCard).join("");
          topRatedEl.querySelectorAll(".eshop-card-link").forEach((card) => {
            card.addEventListener("click", (e) => {
              if (e.target && (e.target.closest("button") || e.target.closest("a"))) return;
              const href = card.getAttribute("data-href");
              if (href) window.location.href = href;
            });
          });
        } catch (err) {
          topRatedEl.innerHTML = `<div class="text-muted small">Không tải được danh sách top đánh giá.</div>`;
          console.error(err);
        }
      }
    } catch (err) {
      grid.innerHTML = "";
      if (errorEl) {
        errorEl.textContent = err?.message ?? "Lỗi tải sản phẩm";
        errorEl.classList.remove("d-none");
      }
      console.error(err);
    }
  }

  function initMessageWidget() {
    const el = document.getElementById("eshopMessageWidget");
    if (!el) return;
    const user = readUser();
    if (!user) return;

    el.innerHTML = `
      <div class="eshop-message-widget-header">
        <div>Tin nhắn mới</div>
        <button class="btn btn-sm btn-outline-secondary" type="button" id="eshopMessageCloseBtn">×</button>
      </div>
      <div class="eshop-message-widget-body">
        <div class="eshop-message-preview">
          <div class="eshop-avatar">👤</div>
          <div>
            <div class="fw-bold">EcoShop</div>
            <div class="text-muted small">Chào bạn, có sản phẩm mới phù hợp đây…</div>
          </div>
        </div>
        <div class="eshop-message-input">
          <input class="form-control form-control-sm" placeholder="Tìm đồ cũ..." disabled />
        </div>
      </div>
    `;

    el.style.display = "block";
    document.getElementById("eshopMessageCloseBtn")?.addEventListener("click", () => {
      el.style.display = "none";
    });
  }

  function similarCard(item) {
    const title = escapeHtml(item.tieuDe ?? "");
    const price = formatMoney(item.gia);
    const href = `/san-pham/${escapeHtml(item.id)}`;
    const thumb = item.anhDaiDien
      ? `<img alt="${title}" loading="lazy" src="${escapeHtml(item.anhDaiDien)}" />`
      : `<div class="eshop-thumb-fallback">No Image</div>`;

    return `
      <div class="eshop-card eshop-card-link" data-href="${href}">
        <div class="eshop-thumb">${thumb}</div>
        <div class="eshop-card-body">
          <div class="eshop-card-title" title="${title}">${title}</div>
          <div class="eshop-price-main">${price} đ</div>
        </div>
      </div>
    `;
  }

  function reviewCard(item) {
    const name = escapeHtml(item.nguoiDanhGia ?? "Người dùng");
    const comment = escapeHtml(item.binhLuan ?? "");
    const score = Number(item.diemDanhGia ?? 0);
    const date = item.ngayTao ? new Date(item.ngayTao).toLocaleDateString("vi-VN") : "";
    return `
      <div class="pd-review">
        <div class="pd-review-head">
          <div>
            <div class="pd-review-name">${name}</div>
            <div class="eshop-stars" aria-label="Rating">${renderStars(score)}</div>
          </div>
          <div class="text-muted small">${escapeHtml(date)}</div>
        </div>
        <div class="pd-review-body">${comment || "—"}</div>
      </div>
    `;
  }

  async function initProductDetail() {
    const root = document.getElementById("productDetailRoot");
    if (!root) return;

    const productId = root.getAttribute("data-id");
    if (!productId) return;

    const els = {
      title: document.getElementById("pdTitle"),
      price: document.getElementById("pdPrice"),
      oldPrice: document.getElementById("pdOldPrice"),
      desc: document.getElementById("pdDesc"),
      meta: document.getElementById("pdMeta"),
      stars: document.getElementById("pdStars"),
      ratingText: document.getElementById("pdRatingText"),
      mainImg: document.getElementById("pdMainImg"),
      thumbs: document.getElementById("pdThumbs"),
      prev: document.getElementById("pdPrevBtn"),
      next: document.getElementById("pdNextBtn"),
      sellerName: document.getElementById("pdSellerName"),
      sellerEmail: document.getElementById("pdSellerEmail"),
      sellerPhone: document.getElementById("pdSellerPhone"),
      address: document.getElementById("pdAddress"),
      reviews: document.getElementById("pdReviews"),
      reviewSummary: document.getElementById("pdReviewSummary"),
      similar: document.getElementById("pdSimilar"),
    };

    let images = [];
    let currentIndex = 0;

    function setImage(index) {
      if (!images.length) return;
      currentIndex = (index + images.length) % images.length;
      const src = images[currentIndex]?.duongDanAnh;
      if (els.mainImg) els.mainImg.src = src || "";
      if (els.thumbs) {
        els.thumbs.querySelectorAll(".pd-thumb").forEach((t) => t.classList.remove("is-active"));
        const active = els.thumbs.querySelector(`[data-idx="${currentIndex}"]`);
        active?.classList.add("is-active");
      }
    }

    try {
      const data = await fetchJson(`/api/san-pham/${productId}`);

      const title = data.tieuDe ?? "";
      if (els.title) els.title.textContent = title;

      const price = formatMoney(data.gia);
      const oldPrice = data.giaGoc && Number(data.giaGoc) > Number(data.gia) ? formatMoney(data.giaGoc) : "";
      if (els.price) els.price.textContent = price ? `${price} đ` : "";
      if (els.oldPrice) els.oldPrice.textContent = oldPrice ? `${oldPrice} đ` : "";

      if (els.desc) els.desc.textContent = data.moTa ?? "";
      if (els.address) els.address.textContent = data.diaDiem ?? "—";

      const rating = Number(data.diemDanhGia ?? 0);
      const count = Number(data.soDanhGia ?? 0);
      if (els.stars) els.stars.innerHTML = renderStars(rating);
      if (els.ratingText) els.ratingText.textContent = count ? `${rating.toFixed(1)} (${count})` : "Chưa có đánh giá";

      if (els.sellerName) els.sellerName.textContent = data.nguoiBan?.hoTen ?? "Người bán";
      if (els.sellerEmail) els.sellerEmail.textContent = data.nguoiBan?.email ?? "";
      if (els.sellerPhone) els.sellerPhone.textContent = data.nguoiBan?.soDienThoai ?? "";

      if (els.meta) {
        els.meta.innerHTML = [
          `<div class="pd-meta-item"><div class="pd-meta-k">Tình trạng</div><div>${escapeHtml(data.tinhTrang ?? "—")}</div></div>`,
          `<div class="pd-meta-item"><div class="pd-meta-k">Danh mục</div><div>${escapeHtml(data.danhMuc ?? "—")}</div></div>`,
          `<div class="pd-meta-item"><div class="pd-meta-k">Số lượng</div><div>${escapeHtml(data.soLuong ?? "—")}</div></div>`,
        ].join("");
      }

      images = Array.isArray(data.anh) ? data.anh : [];
      if (!images.length) {
        images = [{ duongDanAnh: "" }];
        if (els.mainImg) els.mainImg.removeAttribute("src");
      } else {
        const idx = images.findIndex((a) => a.laAnhDaiDien);
        currentIndex = idx >= 0 ? idx : 0;
      }

      if (els.thumbs) {
        els.thumbs.innerHTML = images
          .filter((a) => a.duongDanAnh)
          .map((a, idx) => {
            const src = escapeHtml(a.duongDanAnh);
            return `<button class="pd-thumb" type="button" data-idx="${idx}" aria-label="Thumb ${idx + 1}"><img alt="" src="${src}" /></button>`;
          })
          .join("");

        els.thumbs.querySelectorAll("button.pd-thumb").forEach((btn) => {
          btn.addEventListener("click", () => {
            const idx = Number(btn.getAttribute("data-idx") ?? 0);
            setImage(idx);
          });
        });
      }

      if (els.prev) els.prev.addEventListener("click", () => setImage(currentIndex - 1));
      if (els.next) els.next.addEventListener("click", () => setImage(currentIndex + 1));
      if (images.length && images[currentIndex]?.duongDanAnh) setImage(currentIndex);

      // Reviews
      if (els.reviews && els.reviewSummary) {
        els.reviews.innerHTML = `<div class="text-muted small">Đang tải đánh giá...</div>`;
        try {
          const reviews = await fetchJson(`/api/danh-gia/san-pham/${productId}?trang=1&soLuong=5`);
          els.reviewSummary.textContent = reviews.total ? `${reviews.diemTrungBinh}/5 (${reviews.total})` : "Chưa có đánh giá";
          const list = reviews.data ?? [];
          els.reviews.innerHTML = list.length ? list.map(reviewCard).join("") : `<div class="text-muted small">Chưa có đánh giá.</div>`;
        } catch (err) {
          els.reviews.innerHTML = `<div class="text-muted small">Không tải được đánh giá.</div>`;
          console.error(err);
        }
      }

      // Similar products
      if (els.similar && data.danhMucId) {
        els.similar.innerHTML = `<div class="text-muted small">Đang tải...</div>`;
        try {
          const api = new URL("/api/san-pham", window.location.origin);
          api.searchParams.set("danhMucId", data.danhMucId);
          api.searchParams.set("trang", "1");
          api.searchParams.set("soLuong", "8");
          const result = await fetchJson(api.toString());
          const items = (result?.data ?? []).filter((x) => String(x.id) !== String(productId)).slice(0, 8);
          els.similar.innerHTML = items.length ? items.map(similarCard).join("") : `<div class="text-muted small">Chưa có sản phẩm tương tự.</div>`;
          els.similar.querySelectorAll(".eshop-card-link").forEach((card) => {
            card.addEventListener("click", (e) => {
              if (e.target && (e.target.closest("button") || e.target.closest("a"))) return;
              const href = card.getAttribute("data-href");
              if (href) window.location.href = href;
            });
          });
        } catch (err) {
          els.similar.innerHTML = `<div class="text-muted small">Không tải được sản phẩm tương tự.</div>`;
          console.error(err);
        }
      }

      // Actions (placeholder)
      const addBtn = document.getElementById("pdAddToCartBtn");
      addBtn?.addEventListener("click", () => {
        addToCart(productId, 1);
        window.location.href = "/gio-hang";
      });

      ["pdBuyNowBtn", "pdChatBtn", "pdLikeBtn"].forEach((id) => {
        const btn = document.getElementById(id);
        btn?.addEventListener("click", () => {
          const user = readUser();
          if (!user) {
            window.location.href = "/auth/dang-nhap";
            return;
          }
          alert("Tính năng đang phát triển.");
        });
      });
    } catch (err) {
      if (els.title) els.title.textContent = "Không tải được sản phẩm";
      console.error(err);
    }
  }

  function flattenCategories(categories) {
    const out = [];
    (categories || []).forEach((c) => {
      out.push({ id: c.id, ten: c.tenDanhMuc, level: 0 });
      (c.danhMucCon || []).forEach((child) => out.push({ id: child.id, ten: child.tenDanhMuc, level: 1 }));
    });
    return out;
  }

  function fileThumb(file) {
    return new Promise((resolve) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result || ""));
      reader.readAsDataURL(file);
    });
  }

  function onlyDigits(value) {
    return String(value ?? "").replace(/[^\d]/g, "");
  }

  async function initPost() {
    const root = document.getElementById("postRoot");
    if (!root) return;

    const user = readUser();
    const token = readToken();
    if (!user || !token) {
      window.location.href = "/auth/dang-nhap";
      return;
    }

    const els = {
      title: document.getElementById("postTitleInput"),
      category: document.getElementById("postCategorySelect"),
      condition: document.getElementById("postConditionSelect"),
      desc: document.getElementById("postDescInput"),
      price: document.getElementById("postPriceInput"),
      oldPrice: document.getElementById("postOldPriceInput"),
      qty: document.getElementById("postQtyInput"),
      location: document.getElementById("postLocationInput"),
      imgInput: document.getElementById("postImageInput"),
      pickImagesBtn: document.getElementById("postPickImagesBtn"),
      imgGrid: document.getElementById("postImageGrid"),
      submit: document.getElementById("postSubmitBtn"),
      cancel: document.getElementById("postCancelBtn"),
      error: document.getElementById("postError"),
      sellerName: document.getElementById("postSellerName"),
      sellerEmail: document.getElementById("postSellerEmail"),
      defaultAddress: document.getElementById("postDefaultAddress"),
      changeAddressBtn: document.getElementById("postChangeAddressBtn"),
      addressPickerWrap: document.getElementById("postAddressPickerWrap"),
      addressSelect: document.getElementById("postAddressSelect"),
    };

    if (els.sellerName) els.sellerName.textContent = user.hoTen ?? "—";
    if (els.sellerEmail) els.sellerEmail.textContent = user.email ?? "—";

    const state = {
      files: [],
      coverIndex: 0,
      addresses: [],
      selectedAddressId: "",
    };

    function showError(msg) {
      if (!els.error) return;
      if (!msg) {
        els.error.textContent = "";
        els.error.classList.add("d-none");
        return;
      }
      els.error.textContent = msg;
      els.error.classList.remove("d-none");
    }

    async function renderImages() {
      if (!els.imgGrid) return;
      if (!state.files.length) {
        els.imgGrid.innerHTML = `
          <div class="post-image-empty">
            <div class="text-muted">Chưa có ảnh</div>
            <div class="text-muted small">Hỗ trợ jpg/png/webp</div>
          </div>
        `;
        return;
      }

      const thumbs = await Promise.all(state.files.map(fileThumb));
      els.imgGrid.innerHTML = thumbs
        .map((src, idx) => {
          const isCover = idx === state.coverIndex;
          return `
            <button class="post-image ${isCover ? "is-cover" : ""}" type="button" data-idx="${idx}" title="Chọn làm ảnh đại diện">
              <img alt="" src="${escapeHtml(src)}" />
              ${isCover ? `<div class="post-cover-badge">Ảnh đại diện</div>` : ""}
              <div class="post-remove" data-remove="${idx}" title="Xóa">×</div>
            </button>
          `;
        })
        .join("");

      els.imgGrid.querySelectorAll("button.post-image[data-idx]").forEach((btn) => {
        btn.addEventListener("click", (e) => {
          const removeIdx = e.target?.getAttribute?.("data-remove");
          if (removeIdx != null) {
            const idx = Number(removeIdx);
            state.files.splice(idx, 1);
            if (state.coverIndex >= state.files.length) state.coverIndex = Math.max(0, state.files.length - 1);
            renderImages();
            return;
          }
          const idx = Number(btn.getAttribute("data-idx") ?? 0);
          state.coverIndex = idx;
          renderImages();
        });
      });
    }

    // Load categories
    try {
      const cats = await fetchJson("/api/danh-muc");
      const flat = flattenCategories(cats);
      if (els.category) {
        els.category.innerHTML = flat
          .map((c) => `<option value="${escapeHtml(c.id)}">${escapeHtml(c.level ? "— " : "")}${escapeHtml(c.ten)}</option>`)
          .join("");
      }
    } catch (err) {
      showError(err?.message ?? "Không tải được danh mục");
    }

    // Load conditions
    try {
      const conditions = await fetchJson("/api/tinh-trang");
      if (els.condition) {
        els.condition.innerHTML = (conditions || [])
          .map((t) => `<option value="${escapeHtml(t.id)}">${escapeHtml(t.tenTinhTrang)}</option>`)
          .join("");
      }
    } catch (err) {
      showError(err?.message ?? "Không tải được tình trạng");
    }

    // Load addresses (default)
    try {
      const addrs = await fetchJsonAuth("/api/dia-chi");
      state.addresses = addrs || [];
      const def = state.addresses.find((a) => a.laMacDinh) || state.addresses[0];
      state.selectedAddressId = def?.id || "";
      const display = def ? `${def.hoTen} • ${def.soDienThoai} — ${def.duongPho}, ${def.quanHuyen}, ${def.tinhThanh}` : "Chưa có địa chỉ";
      if (els.defaultAddress) els.defaultAddress.textContent = display;

      if (els.addressSelect) {
        els.addressSelect.innerHTML = state.addresses
          .map((a) => {
            const text = `${a.hoTen} • ${a.soDienThoai} — ${a.duongPho}, ${a.quanHuyen}, ${a.tinhThanh}${a.laMacDinh ? " (Mặc định)" : ""}`;
            return `<option value="${escapeHtml(a.id)}">${escapeHtml(text)}</option>`;
          })
          .join("");
        if (state.selectedAddressId) els.addressSelect.value = state.selectedAddressId;

        els.addressSelect.addEventListener("change", () => {
          state.selectedAddressId = els.addressSelect.value;
          const chosen = state.addresses.find((a) => String(a.id) === String(state.selectedAddressId));
          if (chosen && els.defaultAddress) {
            els.defaultAddress.textContent = `${chosen.hoTen} • ${chosen.soDienThoai} — ${chosen.duongPho}, ${chosen.quanHuyen}, ${chosen.tinhThanh}`;
          }
        });
      }
    } catch (err) {
      if (els.defaultAddress) els.defaultAddress.textContent = "Không tải được địa chỉ (cần đăng nhập)";
    }

    if (els.changeAddressBtn && els.addressPickerWrap) {
      els.changeAddressBtn.addEventListener("click", () => {
        els.addressPickerWrap.classList.toggle("d-none");
      });
    }

    if (els.pickImagesBtn && els.imgInput) {
      els.pickImagesBtn.addEventListener("click", () => els.imgInput.click());
      els.imgInput.addEventListener("change", () => {
        const next = Array.from(els.imgInput.files || []).filter((f) => f.type.startsWith("image/"));
        state.files = [...state.files, ...next].slice(0, 10);
        if (state.coverIndex >= state.files.length) state.coverIndex = 0;
        els.imgInput.value = "";
        renderImages();
      });
    }

    await renderImages();

    if (els.cancel) {
      els.cancel.addEventListener("click", () => {
        window.location.href = "/";
      });
    }

    if (els.submit) {
      els.submit.addEventListener("click", async () => {
        showError("");
        els.submit.disabled = true;
        els.submit.textContent = "Đang đăng...";

        try {
          const tieuDe = (els.title?.value || "").trim();
          const danhMucId = els.category?.value || "";
          const tinhTrangId = els.condition?.value || "";
          const moTa = (els.desc?.value || "").trim();
          const diaDiem = (els.location?.value || "").trim();

          const gia = Number(onlyDigits(els.price?.value));
          const giaGoc = Number(onlyDigits(els.oldPrice?.value || "0")) || 0;
          const soLuong = Number(onlyDigits(els.qty?.value || "1")) || 1;

          if (!tieuDe) throw new Error("Vui lòng nhập tiêu đề");
          if (!danhMucId) throw new Error("Vui lòng chọn danh mục");
          if (!tinhTrangId) throw new Error("Vui lòng chọn tình trạng");
          if (!moTa) throw new Error("Vui lòng nhập mô tả");
          if (!Number.isFinite(gia) || gia <= 0) throw new Error("Giá bán không hợp lệ");
          if (!Number.isFinite(soLuong) || soLuong <= 0) throw new Error("Số lượng không hợp lệ");
          if (!diaDiem) throw new Error("Vui lòng nhập địa điểm");

          const created = await fetchJsonAuth("/api/san-pham", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ danhMucId, tinhTrangId, tieuDe, moTa, gia, giaGoc, soLuong, diaDiem }),
          });

          const productId = created?.id;
          if (!productId) throw new Error("Tạo sản phẩm thất bại");

          if (state.files.length) {
            const form = new FormData();
            state.files.forEach((f) => form.append("files", f));
            form.append("anhDaiDienIndex", String(state.coverIndex || 0));

            await fetchJsonAuth(`/api/san-pham/${productId}/anh`, { method: "POST", body: form });
          }

          window.location.href = `/san-pham/${productId}`;
        } catch (err) {
          showError(err?.message ?? "Đăng tin thất bại");
        } finally {
          els.submit.disabled = false;
          els.submit.textContent = "Đăng tin";
        }
      });
    }
  }

  async function initCartPage() {
    const root = document.getElementById("cartRoot");
    if (!root) return;

    const tbody = document.getElementById("cartTbody");
    const empty = document.getElementById("cartEmpty");
    const selectAll = document.getElementById("cartSelectAll");
    const deleteSelectedBtn = document.getElementById("cartDeleteSelectedBtn");
    const subtotalEl = document.getElementById("cartSubtotal");
    const totalEl = document.getElementById("cartTotal");
    const shippingEl = document.getElementById("cartShipping");
    const addressText = document.getElementById("cartAddressText");
    const changeAddressBtn = document.getElementById("cartChangeAddressBtn");
    const addressWrap = document.getElementById("cartAddressPickerWrap");
    const addressSelect = document.getElementById("cartAddressSelect");
    const checkoutBtn = document.getElementById("cartCheckoutBtn");
    const noteEl = document.getElementById("cartNote");
    const errorEl = document.getElementById("cartError");
    const applyCouponBtn = document.getElementById("cartApplyCouponBtn");

    const money = (v) => `${formatMoney(v)}đ`;

    const state = {
      items: readCart(),
      products: new Map(),
      addresses: [],
      selectedAddressId: "",
    };

    function showError(msg) {
      if (!errorEl) return;
      if (!msg) {
        errorEl.textContent = "";
        errorEl.classList.add("d-none");
        return;
      }
      errorEl.textContent = msg;
      errorEl.classList.remove("d-none");
    }

    function calc() {
      let subtotal = 0;
      state.items.forEach((it) => {
        if (!it.selected) return;
        const p = state.products.get(String(it.productId));
        if (!p) return;
        subtotal += Number(p.gia || 0) * Number(it.qty || 1);
      });
      const shipping = 0;
      const total = subtotal + shipping;
      if (subtotalEl) subtotalEl.textContent = money(subtotal);
      if (shippingEl) shippingEl.textContent = money(shipping);
      if (totalEl) totalEl.textContent = money(total);

      const allChecked = state.items.length > 0 && state.items.every((x) => !!x.selected);
      if (selectAll) selectAll.checked = allChecked;
    }

    function render() {
      if (!tbody) return;

      if (!state.items.length) {
        tbody.innerHTML = "";
        empty?.classList.remove("d-none");
        calc();
        return;
      }

      empty?.classList.add("d-none");

      const rows = state.items
        .map((it) => {
          const p = state.products.get(String(it.productId));
          if (!p) {
            return `
              <tr>
                <td><input class="form-check-input cart-item-check" type="checkbox" data-id="${escapeHtml(it.productId)}" ${it.selected ? "checked" : ""}></td>
                <td colspan="6" class="text-muted">Đang tải sản phẩm...</td>
              </tr>
            `;
          }

          const title = escapeHtml(p.tieuDe || "");
          const img = p.anh?.find((a) => a.laAnhDaiDien)?.duongDanAnh || p.anhDaiDien || "";
          const unit = Number(p.gia || 0);
          const qty = Math.max(1, Number(it.qty || 1));
          const line = unit * qty;

          return `
            <tr>
              <td>
                <input class="form-check-input cart-item-check" type="checkbox" data-id="${escapeHtml(it.productId)}" ${it.selected ? "checked" : ""}>
              </td>
              <td>
                <a class="cart-img" href="/san-pham/${escapeHtml(it.productId)}">
                  ${img ? `<img alt="" src="${escapeHtml(img)}" />` : `<div class="cart-img-fallback">No Image</div>`}
                </a>
              </td>
              <td>
                <a class="text-decoration-none text-dark fw-bold" href="/san-pham/${escapeHtml(it.productId)}">${title}</a>
                <div class="text-muted small">${escapeHtml(p.tinhTrang || "")}</div>
              </td>
              <td class="text-end">${money(unit)}</td>
              <td class="text-center">
                <div class="cart-qty">
                  <button class="btn btn-sm btn-outline-secondary" type="button" data-qty="dec" data-id="${escapeHtml(it.productId)}">−</button>
                  <input class="form-control form-control-sm text-center" value="${escapeHtml(qty)}" data-qty="input" data-id="${escapeHtml(it.productId)}" />
                  <button class="btn btn-sm btn-outline-secondary" type="button" data-qty="inc" data-id="${escapeHtml(it.productId)}">+</button>
                </div>
              </td>
              <td class="text-end fw-bold">${money(line)}</td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-secondary" type="button" data-remove="${escapeHtml(it.productId)}">🗑️</button>
              </td>
            </tr>
          `;
        })
        .join("");

      tbody.innerHTML = rows;
      calc();

      tbody.querySelectorAll("input.cart-item-check").forEach((cb) => {
        cb.addEventListener("change", () => {
          const id = cb.getAttribute("data-id");
          const it = state.items.find((x) => String(x.productId) === String(id));
          if (it) it.selected = cb.checked;
          saveCart(state.items);
          calc();
        });
      });

      tbody.querySelectorAll("button[data-remove]").forEach((btn) => {
        btn.addEventListener("click", () => {
          const id = btn.getAttribute("data-remove");
          state.items = state.items.filter((x) => String(x.productId) !== String(id));
          saveCart(state.items);
          render();
        });
      });

      tbody.querySelectorAll("button[data-qty]").forEach((btn) => {
        btn.addEventListener("click", () => {
          const id = btn.getAttribute("data-id");
          const mode = btn.getAttribute("data-qty");
          const it = state.items.find((x) => String(x.productId) === String(id));
          if (!it) return;
          const next = mode === "inc" ? Number(it.qty || 1) + 1 : Number(it.qty || 1) - 1;
          it.qty = Math.max(1, Math.min(99, next));
          saveCart(state.items);
          render();
        });
      });

      tbody.querySelectorAll("input[data-qty=\"input\"]").forEach((inp) => {
        inp.addEventListener("change", () => {
          const id = inp.getAttribute("data-id");
          const it = state.items.find((x) => String(x.productId) === String(id));
          if (!it) return;
          const next = Number(onlyDigits(inp.value || "1")) || 1;
          it.qty = Math.max(1, Math.min(99, next));
          saveCart(state.items);
          render();
        });
      });
    }

    if (selectAll) {
      selectAll.addEventListener("change", () => {
        state.items.forEach((x) => (x.selected = selectAll.checked));
        saveCart(state.items);
        render();
      });
    }

    if (deleteSelectedBtn) {
      deleteSelectedBtn.addEventListener("click", () => {
        state.items = state.items.filter((x) => !x.selected);
        saveCart(state.items);
        render();
      });
    }

    if (applyCouponBtn) {
      applyCouponBtn.addEventListener("click", () => {
        alert("Chưa hỗ trợ mã giảm giá (UI demo).");
      });
    }

    // Load products
    if (state.items.length) {
      await Promise.all(
        state.items.map(async (it) => {
          try {
            const p = await fetchJson(`/api/san-pham/${it.productId}`);
            state.products.set(String(it.productId), p);
          } catch (err) {
            console.error(err);
          }
        }),
      );
    }

    // Load addresses
    const user = readUser();
    if (user && addressText) {
      try {
        const addrs = await fetchJsonAuth("/api/dia-chi");
        state.addresses = addrs || [];
        const def = state.addresses.find((a) => a.laMacDinh) || state.addresses[0];
        state.selectedAddressId = def?.id || "";
        addressText.textContent = def
          ? `${def.hoTen} • ${def.soDienThoai} — ${def.duongPho}, ${def.quanHuyen}, ${def.tinhThanh}`
          : "Chưa có địa chỉ. Vui lòng thêm địa chỉ.";

        if (addressSelect) {
          addressSelect.innerHTML = state.addresses
            .map((a) => {
              const text = `${a.hoTen} • ${a.soDienThoai} — ${a.duongPho}, ${a.quanHuyen}, ${a.tinhThanh}${a.laMacDinh ? " (Mặc định)" : ""}`;
              return `<option value="${escapeHtml(a.id)}">${escapeHtml(text)}</option>`;
            })
            .join("");
          if (state.selectedAddressId) addressSelect.value = state.selectedAddressId;
          addressSelect.addEventListener("change", () => {
            state.selectedAddressId = addressSelect.value;
            const chosen = state.addresses.find((a) => String(a.id) === String(state.selectedAddressId));
            if (chosen && addressText) {
              addressText.textContent = `${chosen.hoTen} • ${chosen.soDienThoai} — ${chosen.duongPho}, ${chosen.quanHuyen}, ${chosen.tinhThanh}`;
            }
          });
        }
      } catch {
        addressText.textContent = "Không tải được địa chỉ (cần đăng nhập)";
      }
    }

    if (changeAddressBtn && addressWrap) {
      changeAddressBtn.addEventListener("click", () => {
        addressWrap.classList.toggle("d-none");
      });
    }

    if (checkoutBtn) {
      checkoutBtn.addEventListener("click", async () => {
        showError("");
        const user = readUser();
        if (!user) {
          window.location.href = "/auth/dang-nhap";
          return;
        }
        const selected = state.items.filter((x) => x.selected);
        if (!selected.length) {
          showError("Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.");
          return;
        }
        if (!state.selectedAddressId) {
          showError("Vui lòng chọn địa chỉ nhận hàng.");
          return;
        }

        const pay = "thanh_toan_khi_nhan_hang";
        const ghiChu = noteEl?.value || "";

        checkoutBtn.disabled = true;
        checkoutBtn.textContent = "Đang xử lý...";
        try {
          const dto = {
            diaChiId: state.selectedAddressId,
            phuongThucThanhToan: pay,
            ghiChu,
            sanPhams: selected.map((x) => ({ sanPhamId: x.productId, soLuong: Number(x.qty || 1) })),
          };
          const res = await fetchJsonAuth("/api/don-hang", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
          });
          const donHangId = res?.donHangId || "";
          state.items = state.items.filter((x) => !x.selected);
          saveCart(state.items);
          window.location.href = donHangId ? `/don-hang/mua?new=${encodeURIComponent(donHangId)}` : "/don-hang/mua";
        } catch (err) {
          showError(err?.message ?? "Thanh toán thất bại");
        } finally {
          checkoutBtn.disabled = false;
          checkoutBtn.textContent = "Tiến Hành Thanh Toán";
        }
      });
    }

    render();
  }

  async function initAccountPage() {
    const root = document.getElementById("accountRoot");
    if (!root) return;

    const user = readUser();
    const token = readToken();
    if (!user || !token) {
      window.location.href = "/auth/dang-nhap";
      return;
    }

    const els = {
      name: document.getElementById("accName"),
      email: document.getElementById("accEmail"),
      phone: document.getElementById("accPhone"),
      refresh: document.getElementById("accRefreshBtn"),
      error: document.getElementById("accError"),
      list: document.getElementById("addrList"),
      empty: document.getElementById("addrEmpty"),
      addBtn: document.getElementById("addrAddBtn"),
      modal: document.getElementById("addrModal"),
      modalTitle: document.getElementById("addrModalTitle"),
      modalError: document.getElementById("addrModalError"),
      saveBtn: document.getElementById("addrSaveBtn"),
      hoTen: document.getElementById("addrHoTen"),
      soDienThoai: document.getElementById("addrSoDienThoai"),
      duongPho: document.getElementById("addrDuongPho"),
      quanHuyen: document.getElementById("addrQuanHuyen"),
      tinhThanh: document.getElementById("addrTinhThanh"),
      macDinh: document.getElementById("addrMacDinh"),
    };

    const state = {
      addresses: [],
      editingId: null,
      modalInstance: null,
    };

    function showAccError(msg) {
      if (!els.error) return;
      if (!msg) {
        els.error.textContent = "";
        els.error.classList.add("d-none");
        return;
      }
      els.error.textContent = msg;
      els.error.classList.remove("d-none");
    }

    function showModalError(msg) {
      if (!els.modalError) return;
      if (!msg) {
        els.modalError.textContent = "";
        els.modalError.classList.add("d-none");
        return;
      }
      els.modalError.textContent = msg;
      els.modalError.classList.remove("d-none");
    }

    function fillProfile(profile) {
      if (els.name) els.name.textContent = profile?.hoTen ?? user.hoTen ?? "—";
      if (els.email) els.email.textContent = profile?.email ?? user.email ?? "—";
      if (els.phone) els.phone.textContent = profile?.soDienThoai ?? "—";
    }

    function addressLine(a) {
      return `${a.duongPho}, ${a.quanHuyen}, ${a.tinhThanh}`;
    }

    function renderList() {
      if (!els.list) return;

      if (!state.addresses.length) {
        els.list.innerHTML = "";
        els.empty?.classList.remove("d-none");
        return;
      }

      els.empty?.classList.add("d-none");
      els.list.innerHTML = state.addresses
        .map((a) => {
          const badge = a.laMacDinh ? `<span class="acc-badge">Mặc định</span>` : "";
          return `
            <div class="acc-address">
              <div class="acc-address-main">
                <div class="d-flex align-items-center gap-2">
                  <div class="fw-bold">${escapeHtml(a.hoTen)}</div>
                  ${badge}
                </div>
                <div class="text-muted small">${escapeHtml(a.soDienThoai)}</div>
                <div class="text-muted small">${escapeHtml(addressLine(a))}</div>
              </div>
              <div class="acc-address-actions">
                ${a.laMacDinh ? "" : `<button class="btn btn-sm btn-outline-secondary" type="button" data-default="${escapeHtml(a.id)}">Đặt mặc định</button>`}
                <button class="btn btn-sm btn-outline-secondary" type="button" data-edit="${escapeHtml(a.id)}">Sửa</button>
                <button class="btn btn-sm btn-outline-secondary" type="button" data-del="${escapeHtml(a.id)}">Xóa</button>
              </div>
            </div>
          `;
        })
        .join("");

      els.list.querySelectorAll("button[data-default]").forEach((btn) => {
        btn.addEventListener("click", async () => {
          const id = btn.getAttribute("data-default");
          try {
            await fetchJsonAuth(`/api/dia-chi/${id}/mac-dinh`, { method: "PUT" });
            await loadAddresses();
          } catch (err) {
            showAccError(err?.message ?? "Không đặt được mặc định");
          }
        });
      });

      els.list.querySelectorAll("button[data-edit]").forEach((btn) => {
        btn.addEventListener("click", () => {
          const id = btn.getAttribute("data-edit");
          const a = state.addresses.find((x) => String(x.id) === String(id));
          if (!a) return;
          openModal("Sửa địa chỉ", a);
        });
      });

      els.list.querySelectorAll("button[data-del]").forEach((btn) => {
        btn.addEventListener("click", async () => {
          const id = btn.getAttribute("data-del");
          if (!confirm("Bạn chắc chắn muốn xóa địa chỉ này?")) return;
          try {
            await fetchJsonAuth(`/api/dia-chi/${id}`, { method: "DELETE" });
            await loadAddresses();
          } catch (err) {
            showAccError(err?.message ?? "Xóa địa chỉ thất bại");
          }
        });
      });
    }

    async function loadAddresses() {
      showAccError("");
      try {
        const addrs = await fetchJsonAuth("/api/dia-chi");
        state.addresses = (addrs || []).sort((a, b) => (b.laMacDinh ? 1 : 0) - (a.laMacDinh ? 1 : 0));
        renderList();
      } catch (err) {
        showAccError(err?.message ?? "Không tải được địa chỉ");
      }
    }

    function openModal(title, addr) {
      showModalError("");
      state.editingId = addr?.id ?? null;
      if (els.modalTitle) els.modalTitle.textContent = title;

      if (els.hoTen) els.hoTen.value = addr?.hoTen ?? "";
      if (els.soDienThoai) els.soDienThoai.value = addr?.soDienThoai ?? "";
      if (els.duongPho) els.duongPho.value = addr?.duongPho ?? "";
      if (els.quanHuyen) els.quanHuyen.value = addr?.quanHuyen ?? "";
      if (els.tinhThanh) els.tinhThanh.value = addr?.tinhThanh ?? "";
      if (els.macDinh) els.macDinh.checked = !!addr?.laMacDinh;

      if (!state.modalInstance && els.modal && window.bootstrap?.Modal) {
        state.modalInstance = new window.bootstrap.Modal(els.modal);
      }
      state.modalInstance?.show();
    }

    function validateModal() {
      const hoTen = (els.hoTen?.value || "").trim();
      const soDienThoai = (els.soDienThoai?.value || "").trim();
      const duongPho = (els.duongPho?.value || "").trim();
      const quanHuyen = (els.quanHuyen?.value || "").trim();
      const tinhThanh = (els.tinhThanh?.value || "").trim();
      const laMacDinh = !!els.macDinh?.checked;

      if (!hoTen) throw new Error("Vui lòng nhập họ tên");
      if (!soDienThoai) throw new Error("Vui lòng nhập số điện thoại");
      if (!duongPho) throw new Error("Vui lòng nhập đường/phố");
      if (!quanHuyen) throw new Error("Vui lòng nhập quận/huyện");
      if (!tinhThanh) throw new Error("Vui lòng nhập tỉnh/thành");

      return { hoTen, soDienThoai, duongPho, quanHuyen, tinhThanh, laMacDinh };
    }

    if (els.addBtn) els.addBtn.addEventListener("click", () => openModal("Thêm địa chỉ", null));

    if (els.refresh) {
      els.refresh.addEventListener("click", async () => {
        showAccError("");
        try {
          const profile = await fetchJsonAuth("/api/nguoi-dung/ho-so");
          fillProfile(profile);
        } catch (err) {
          showAccError(err?.message ?? "Không tải được hồ sơ");
        }
      });
    }

    if (els.saveBtn) {
      els.saveBtn.addEventListener("click", async () => {
        showModalError("");
        els.saveBtn.disabled = true;
        els.saveBtn.textContent = "Đang lưu...";

        try {
          const dto = validateModal();

          if (state.editingId) {
            await fetchJsonAuth(`/api/dia-chi/${state.editingId}`, {
              method: "PUT",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(dto),
            });
          } else {
            await fetchJsonAuth("/api/dia-chi", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(dto),
            });
          }

          state.modalInstance?.hide();
          await loadAddresses();
        } catch (err) {
          showModalError(err?.message ?? "Lưu thất bại");
        } finally {
          els.saveBtn.disabled = false;
          els.saveBtn.textContent = "Lưu";
        }
      });
    }

    // Initial load
    fillProfile(user);
    try {
      const profile = await fetchJsonAuth("/api/nguoi-dung/ho-so");
      fillProfile(profile);
    } catch {}
    await loadAddresses();
  }

  document.addEventListener("DOMContentLoaded", async () => {
    await initHeader();
    const isHome = window.__ESHOP_PAGE__ === "home" || document.getElementById("homeNewProductGrid");
    if (isHome) await initHome();
    initMessageWidget();
    const isProduct = window.__ESHOP_PAGE__ === "product" || document.getElementById("productDetailRoot");
    if (isProduct) await initProductDetail();
    const isPost = window.__ESHOP_PAGE__ === "post" || document.getElementById("postRoot");
    if (isPost) await initPost();
    const isCart = window.__ESHOP_PAGE__ === "cart" || document.getElementById("cartRoot");
    if (isCart) await initCartPage();
    const isAccount = window.__ESHOP_PAGE__ === "account" || document.getElementById("accountRoot");
    if (isAccount) await initAccountPage();
    const isOrdersBuyer = window.__ESHOP_PAGE__ === "orders_buyer" || document.getElementById("ordersBuyerRoot");
    if (isOrdersBuyer) await initOrdersBuyer();
    const isOrdersSeller = window.__ESHOP_PAGE__ === "orders_seller" || document.getElementById("ordersSellerRoot");
    if (isOrdersSeller) await initOrdersSeller();
    const isOrderDetail = window.__ESHOP_PAGE__ === "order_detail" || document.getElementById("orderDetailRoot");
    if (isOrderDetail) await initOrderDetail();
  });
})();
