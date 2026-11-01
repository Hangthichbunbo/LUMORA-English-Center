// TruyenVietSub Global Client Script

document.addEventListener('DOMContentLoaded', function () {
    initTheme();
    initReaderSettings();
    initPasswordToggle();
    initAdPopup();
});

// ==========================================
// 1. THEME SWITCHER (DARK / LIGHT MODE)
// ==========================================
function initTheme() {
    const savedTheme = localStorage.getItem('truyenvietsub_theme') || localStorage.getItem('storyhub_theme') || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    setTheme(savedTheme);

    const themeToggleBtn = document.getElementById('theme-toggle-btn');
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            const currentTheme = document.documentElement.getAttribute('data-bs-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
            localStorage.setItem('truyenvietsub_theme', newTheme);
        });
    }
}

function setTheme(theme) {
    document.documentElement.setAttribute('data-bs-theme', theme);
    const icon = document.getElementById('theme-icon');
    if (icon) {
        if (theme === 'dark') {
            icon.className = 'bi bi-sun-fill text-warning';
        } else {
            icon.className = 'bi bi-moon-stars-fill text-body-secondary';
        }
    }
}

// ==========================================
// 2. READER SETTINGS (FONT SIZE, WIDTH)
// ==========================================
function initReaderSettings() {
    const readerContainer = document.querySelector('.reader-container');
    if (!readerContainer) return;

    let fontSize = parseInt(localStorage.getItem('reader_font_size')) || 18;
    let width = parseInt(localStorage.getItem('reader_width')) || 800;

    applyReaderStyles(fontSize, width);

    const btnFontInc = document.getElementById('btn-font-inc');
    const btnFontDec = document.getElementById('btn-font-dec');
    const btnWidthToggle = document.getElementById('btn-width-toggle');

    if (btnFontInc) {
        btnFontInc.addEventListener('click', function () {
            if (fontSize < 32) {
                fontSize += 2;
                localStorage.setItem('reader_font_size', fontSize);
                applyReaderStyles(fontSize, width);
            }
        });
    }

    if (btnFontDec) {
        btnFontDec.addEventListener('click', function () {
            if (fontSize > 14) {
                fontSize -= 2;
                localStorage.setItem('reader_font_size', fontSize);
                applyReaderStyles(fontSize, width);
            }
        });
    }

    if (btnWidthToggle) {
        btnWidthToggle.addEventListener('click', function () {
            width = width === 800 ? 1100 : (width === 1100 ? 650 : 800);
            localStorage.setItem('reader_width', width);
            applyReaderStyles(fontSize, width);
        });
    }
}

function applyReaderStyles(fontSize, width) {
    document.documentElement.style.setProperty('--reader-font-size', fontSize + 'px');
    document.documentElement.style.setProperty('--reader-max-width', width + 'px');

    const fontSizeDisplay = document.getElementById('font-size-display');
    if (fontSizeDisplay) fontSizeDisplay.textContent = fontSize + 'px';
}

// ==========================================
// 3. CSRF & AJAX UTILITIES
// ==========================================
function getCsrfToken() {
    return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content')
        || document.querySelector('input[name="__RequestVerificationToken"]')?.value
        || '';
}

function getAjaxHeaders(extraHeaders = {}) {
    const token = getCsrfToken();
    return {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
        'X-XSRF-TOKEN': token,
        'RequestVerificationToken': token,
        ...extraHeaders
    };
}

// ==========================================
// 4. RATING AJAX
// ==========================================
async function submitRating(storyId, score) {
    try {
        const response = await fetch('/Rating/Submit', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({ storyId: storyId, score: score })
        });

        let data = null;
        try {
            data = await response.json();
        } catch (_) {
            // Non-JSON response
        }

        if (response.ok && data && data.success) {
            alert(data.message || 'Cảm ơn bạn đã đánh giá truyện!');
            const avgDisplay = document.getElementById('story-average-rating');
            const countDisplay = document.getElementById('story-rating-count');
            if (avgDisplay && data.data) avgDisplay.textContent = Number(data.data.averageRating).toFixed(1);
            if (countDisplay && data.data) countDisplay.textContent = data.data.ratingCount;
        } else if (response.status === 401) {
            alert(data?.message || 'Vui lòng đăng nhập để đánh giá truyện.');
            window.location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else if (response.status === 429) {
            alert(data?.message || 'Bạn thao tác đánh giá quá nhanh. Vui lòng thử lại sau giây lát.');
        } else {
            alert(data?.message || 'Không thể gửi đánh giá.');
        }
    } catch (e) {
        console.error('Error submitting rating:', e);
        alert('Lỗi kết nối khi gửi đánh giá.');
    }
}

// ==========================================
// 5. TOGGLE FOLLOW STORY
// ==========================================
async function toggleFollow(storyId) {
    try {
        const response = await fetch('/Story/ToggleFollow', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({ storyId: storyId })
        });

        let data = null;
        try {
            data = await response.json();
        } catch (_) {}

        if (response.ok && data && data.success) {
            const btn = document.getElementById('btn-follow');
            if (btn) {
                if (data.data === true) {
                    btn.className = 'btn btn-outline-danger';
                    btn.innerHTML = '<i class="bi bi-heart-fill me-1"></i>Đã theo dõi';
                } else {
                    btn.className = 'btn btn-danger';
                    btn.innerHTML = '<i class="bi bi-heart me-1"></i>Theo dõi truyện';
                }
            }
        } else if (response.status === 401) {
            alert(data?.message || 'Vui lòng đăng nhập để theo dõi truyện.');
            window.location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else {
            alert(data?.message || 'Thao tác không thành công.');
        }
    } catch (e) {
        console.error('Error toggling follow:', e);
        alert('Lỗi kết nối khi theo dõi truyện.');
    }
}

// ==========================================
// 6. COMMENT MANAGEMENT
// ==========================================
async function submitComment(event) {
    event.preventDefault();
    const form = event.target;
    const formData = new FormData(form);

    const storyId = parseInt(formData.get('StoryId'));
    const chapterId = formData.get('ChapterId') ? parseInt(formData.get('ChapterId')) : null;
    const content = formData.get('Content');

    const submitBtn = form.querySelector('button[type="submit"]');
    if (submitBtn) submitBtn.disabled = true;

    try {
        const response = await fetch('/Comment/Create', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({
                storyId: storyId,
                chapterId: chapterId,
                content: content
            })
        });

        let data = null;
        try { data = await response.json(); } catch (_) {}

        if (response.ok && data && data.success) {
            form.reset();
            const charCount = document.getElementById('char-count');
            if (charCount) charCount.textContent = '0';
            location.reload();
        } else if (response.status === 401) {
            alert(data?.message || 'Vui lòng đăng nhập để bình luận.');
            window.location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else {
            alert(data?.message || 'Lỗi khi đăng bình luận.');
        }
    } catch (e) {
        console.error('Error submitting comment:', e);
        alert('Lỗi kết nối tới máy chủ.');
    } finally {
        if (submitBtn) submitBtn.disabled = false;
    }
}

function showReplyForm(commentId, authorName) {
    const container = document.getElementById('reply-form-container-' + commentId);
    if (!container) return;

    if (!container.classList.contains('d-none')) {
        container.classList.add('d-none');
        container.innerHTML = '';
        return;
    }

    container.classList.remove('d-none');
    container.innerHTML = `
        <div class="card p-2 bg-body border-0 shadow-sm mt-2">
            <form onsubmit="submitReply(event, ${commentId})">
                <div class="mb-2">
                    <textarea class="form-control form-control-sm" name="ReplyContent" rows="2" 
                              placeholder="Trả lời @${authorName}..." required maxlength="1000"></textarea>
                </div>
                <div class="text-end">
                    <button type="button" class="btn btn-sm btn-light me-1" onclick="showReplyForm(${commentId})">Hủy</button>
                    <button type="submit" class="btn btn-sm btn-primary">Gửi trả lời</button>
                </div>
            </form>
        </div>
    `;
}

async function submitReply(event, parentCommentId) {
    event.preventDefault();
    const form = event.target;
    const content = form.querySelector('textarea[name="ReplyContent"]').value;
    const commentSection = document.getElementById('comment-section');
    const storyId = parseInt(commentSection.getAttribute('data-story-id'));
    const chapterIdAttr = commentSection.getAttribute('data-chapter-id');
    const chapterId = chapterIdAttr ? parseInt(chapterIdAttr) : null;

    try {
        const response = await fetch('/Comment/Create', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({
                storyId: storyId,
                chapterId: chapterId,
                parentCommentId: parentCommentId,
                content: content
            })
        });

        let data = null;
        try { data = await response.json(); } catch (_) {}

        if (response.ok && data && data.success) {
            location.reload();
        } else if (response.status === 401) {
            alert(data?.message || 'Vui lòng đăng nhập để trả lời bình luận.');
            window.location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else {
            alert(data?.message || 'Không thể đăng câu trả lời.');
        }
    } catch (e) {
        console.error('Error submitting reply:', e);
        alert('Lỗi kết nối máy chủ.');
    }
}

async function deleteComment(commentId) {
    if (!confirm('Bạn có chắc chắn muốn xóa bình luận này?')) return;

    try {
        const response = await fetch('/Comment/Delete', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({ commentId: commentId })
        });

        let data = null;
        try { data = await response.json(); } catch (_) {}

        if (response.ok && data && data.success) {
            const elem = document.getElementById('comment-' + commentId);
            if (elem) elem.remove();
        } else if (response.status === 401) {
            alert(data?.message || 'Vui lòng đăng nhập.');
            window.location.href = '/dang-nhap?returnUrl=' + encodeURIComponent(window.location.pathname);
        } else {
            alert(data?.message || 'Không thể xóa bình luận.');
        }
    } catch (e) {
        console.error('Error deleting comment:', e);
        alert('Lỗi kết nối khi xóa bình luận.');
    }
}

function openReportModal(commentId) {
    const input = document.getElementById('report-comment-id');
    if (input) input.value = commentId;
    const modalEl = document.getElementById('reportCommentModal');
    if (modalEl) {
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }
}

async function submitReport(event) {
    event.preventDefault();
    const form = event.target;
    const formData = new FormData(form);

    const commentId = parseInt(formData.get('CommentId'));
    const reason = formData.get('Reason');
    const description = formData.get('Description');

    try {
        const response = await fetch('/Comment/Report', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({
                commentId: commentId,
                reason: reason,
                description: description
            })
        });

        let data = null;
        try { data = await response.json(); } catch (_) {}

        alert(data?.message || (response.ok ? 'Đã gửi báo cáo vi phạm thành công.' : 'Lỗi khi gửi báo cáo.'));
        const modalEl = document.getElementById('reportCommentModal');
        if (modalEl) {
            const modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();
        }
        form.reset();
    } catch (e) {
        console.error('Error submitting report:', e);
        alert('Lỗi kết nối khi gửi báo cáo.');
    }
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    const pad = (n) => n.toString().padStart(2, '0');
    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function renderCommentHtml(c, isAuthenticated) {
    const avatar = c.userAvatarUrl || c.UserAvatarUrl || '/images/default-avatar.webp';
    const author = escapeHtml(c.userDisplayName || c.UserDisplayName || 'Người dùng');
    const content = escapeHtml(c.content || c.Content || '');
    const dateFormatted = formatDate(c.createdAt || c.CreatedAt);
    const chapterTitle = c.chapterTitle || c.ChapterTitle;
    const chapterBadge = chapterTitle ? `<div class="badge bg-secondary-subtle text-secondary small mb-1"><i class="bi bi-bookmark me-1"></i>${escapeHtml(chapterTitle)}</div>` : '';
    const commentId = c.id || c.Id;
    const canEdit = c.canEdit || c.CanEdit;
    const canDelete = c.canDelete || c.CanDelete;
    const replies = c.replies || c.Replies || [];

    let actionsHtml = '';
    if (isAuthenticated) {
        actionsHtml += `<button type="button" class="btn btn-link p-0 small text-decoration-none text-muted" onclick="showReplyForm(${commentId}, '${author.replace(/'/g, "\\'")}')"><i class="bi bi-reply-fill me-1"></i>Trả lời</button>`;
    }
    if (canEdit) {
        actionsHtml += `<button type="button" class="btn btn-link p-0 small text-decoration-none text-muted" onclick="editComment(${commentId})"><i class="bi bi-pencil me-1"></i>Sửa</button>`;
    }
    if (canDelete) {
        actionsHtml += `<button type="button" class="btn btn-link p-0 small text-decoration-none text-danger" onclick="deleteComment(${commentId})"><i class="bi bi-trash me-1"></i>Xóa</button>`;
    }
    actionsHtml += `<button type="button" class="btn btn-link p-0 small text-decoration-none text-muted" onclick="openReportModal(${commentId})"><i class="bi bi-flag me-1"></i>Báo cáo</button>`;

    let repliesHtml = '';
    if (replies.length > 0) {
        repliesHtml += '<div class="replies-list ms-4 mt-3 border-start ps-3 bg-body-tertiary rounded p-2">';
        replies.forEach(r => {
            const rAvatar = r.userAvatarUrl || r.UserAvatarUrl || '/images/default-avatar.webp';
            const rAuthor = escapeHtml(r.userDisplayName || r.UserDisplayName || 'Người dùng');
            const rContent = escapeHtml(r.content || r.Content || '');
            const rDate = formatDate(r.createdAt || r.CreatedAt);
            const rId = r.id || r.Id;
            let rActions = '';
            if (r.canEdit || r.CanEdit) rActions += `<button type="button" class="btn btn-link p-0 text-decoration-none text-muted" onclick="editComment(${rId})">Sửa</button>`;
            if (r.canDelete || r.CanDelete) rActions += `<button type="button" class="btn btn-link p-0 text-decoration-none text-danger" onclick="deleteComment(${rId})">Xóa</button>`;
            rActions += `<button type="button" class="btn btn-link p-0 text-decoration-none text-muted" onclick="openReportModal(${rId})">Báo cáo</button>`;

            repliesHtml += `
                <div class="reply-item py-2 border-bottom border-light" id="comment-${rId}">
                    <div class="d-flex">
                        <img src="${rAvatar}" class="rounded-circle me-2 flex-shrink-0" width="30" height="30" alt="${rAuthor}" onerror="this.onerror=null; this.src='/images/default-avatar.webp';" />
                        <div class="flex-grow-1">
                            <div class="d-flex justify-content-between align-items-baseline">
                                <span class="fw-bold small">${rAuthor}</span>
                                <small class="text-muted" style="font-size: 0.75rem;">${rDate}</small>
                            </div>
                            <p class="mb-1 small text-body" style="white-space: pre-line;">${rContent}</p>
                            <div class="d-flex gap-3 small text-muted" style="font-size: 0.75rem;">
                                ${rActions}
                            </div>
                        </div>
                    </div>
                </div>`;
        });
        repliesHtml += '</div>';
    }

    return `
        <div class="comment-item border-bottom py-3" id="comment-${commentId}">
            <div class="d-flex">
                <img src="${avatar}" class="rounded-circle me-3 flex-shrink-0" width="40" height="40" alt="${author}" onerror="this.onerror=null; this.src='/images/default-avatar.webp';" />
                <div class="flex-grow-1">
                    <div class="d-flex justify-content-between align-items-baseline">
                        <h6 class="mb-0 fw-bold">${author}</h6>
                        <small class="text-muted">${dateFormatted}</small>
                    </div>
                    ${chapterBadge}
                    <p class="mb-2 mt-1 comment-text text-body" style="white-space: pre-line;">${content}</p>
                    <div class="d-flex gap-3 small text-muted">
                        ${actionsHtml}
                    </div>
                    ${repliesHtml}
                    <div id="reply-form-container-${commentId}" class="mt-2 d-none"></div>
                </div>
            </div>
        </div>`;
}

async function loadMoreComments() {
    const btn = document.getElementById('btn-load-more-comments');
    if (!btn) return;

    const commentSection = document.getElementById('comment-section');
    if (!commentSection) return;

    const storyId = parseInt(commentSection.getAttribute('data-story-id'));
    const chapterIdAttr = commentSection.getAttribute('data-chapter-id');
    const chapterId = chapterIdAttr ? parseInt(chapterIdAttr) : '';

    const currentPage = parseInt(btn.dataset.currentPage || '1');
    const totalPages = parseInt(btn.dataset.totalPages || '1');
    const nextPage = currentPage + 1;

    if (nextPage > totalPages) {
        document.getElementById('load-more-comments-container')?.remove();
        return;
    }

    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Đang tải...';

    try {
        let url = `/Comment/GetList?storyId=${storyId}&pageIndex=${nextPage}`;
        if (chapterId) url += `&chapterId=${chapterId}`;

        const response = await fetch(url, {
            headers: getAjaxHeaders()
        });

        if (!response.ok) throw new Error('Network response was not ok');

        const result = await response.json();
        const items = result?.data?.items || result?.data?.Items || [];
        const returnedTotalPages = result?.data?.totalPages || result?.data?.TotalPages || totalPages;

        if (result && result.success && items.length > 0) {
            const container = document.getElementById('comments-container');
            const isAuthenticated = document.getElementById('comment-form') !== null;
            
            items.forEach(c => {
                const commentEl = document.createElement('div');
                commentEl.innerHTML = renderCommentHtml(c, isAuthenticated);
                container.appendChild(commentEl.firstElementChild);
            });

            btn.dataset.currentPage = nextPage.toString();
            btn.dataset.totalPages = returnedTotalPages.toString();

            if (nextPage >= returnedTotalPages) {
                document.getElementById('load-more-comments-container')?.remove();
            }
        } else {
            document.getElementById('load-more-comments-container')?.remove();
        }
    } catch (e) {
        console.error('Error loading more comments:', e);
        alert('Không thể tải thêm bình luận. Vui lòng thử lại sau.');
    } finally {
        if (btn && btn.isConnected) {
            btn.disabled = false;
            btn.innerHTML = originalHtml;
        }
    }
}

async function editComment(commentId) {
    const commentEl = document.getElementById('comment-' + commentId);
    const p = commentEl?.querySelector('.comment-text, p.small');
    const oldText = p ? p.innerText.trim() : '';
    const newText = prompt('Chỉnh sửa bình luận của bạn:', oldText);
    if (!newText || newText.trim() === '' || newText.trim() === oldText) return;

    try {
        const response = await fetch('/Comment/Update', {
            method: 'POST',
            headers: getAjaxHeaders(),
            body: JSON.stringify({ commentId: commentId, content: newText.trim() })
        });
        const data = await response.json();
        if (response.ok && data.success) {
            if (p) p.innerText = newText.trim();
        } else {
            alert(data?.message || 'Không thể cập nhật bình luận.');
        }
    } catch (e) {
        alert('Lỗi kết nối khi cập nhật bình luận.');
    }
}

// ==========================================
// 4. PASSWORD VISIBILITY TOGGLER
// ==========================================
function initPasswordToggle() {
    document.addEventListener('click', function (e) {
        const toggleBtn = e.target.closest('.toggle-password-btn');
        if (!toggleBtn) return;

        e.preventDefault();
        const group = toggleBtn.closest('.input-group');
        if (!group) return;

        const input = group.querySelector('input');
        if (!input) return;

        const icon = toggleBtn.querySelector('i');
        if (input.type === 'password') {
            input.type = 'text';
            if (icon) {
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            }
            toggleBtn.setAttribute('title', 'Ẩn mật khẩu');
            toggleBtn.setAttribute('aria-label', 'Ẩn mật khẩu');
        } else {
            input.type = 'password';
            if (icon) {
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
            toggleBtn.setAttribute('title', 'Hiện mật khẩu');
            toggleBtn.setAttribute('aria-label', 'Hiện mật khẩu');
        }
    });
}

// ==========================================
// 5. ADVERTISEMENT POPUP & TRACKING
// ==========================================
function initAdPopup() {
    const popup = document.getElementById('ad-popup-modal');
    if (!popup) return;

    if (sessionStorage.getItem('tvs_popup_closed') === 'true') {
        return;
    }

    setTimeout(function () {
        popup.style.display = 'flex';
    }, 600);
}

function closeAdPopup() {
    const popup = document.getElementById('ad-popup-modal');
    if (popup) {
        popup.style.display = 'none';
        sessionStorage.setItem('tvs_popup_closed', 'true');
    }
}

function trackAdClick(adId) {
    if (!adId) return;
    try {
        if (navigator.sendBeacon) {
            navigator.sendBeacon('/api/ads/click/' + encodeURIComponent(adId));
        }
    } catch (e) { }
}
