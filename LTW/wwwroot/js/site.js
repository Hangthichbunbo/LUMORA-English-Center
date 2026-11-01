// ============================================================================
// Main Author: hoangthuhang
// Project: LTW Project - IELTS Online Practice Landing Page
// File: wwwroot/js/site.js
// Description: Tập lệnh JavaScript hỗ trợ tương tác người dùng:
//              - Khởi tạo Carousel mượt mà
//              - Tự động đóng navbar khi click liên kết trên thiết bị di động
//              - Hiệu ứng cuộn mượt (Smooth scroll)
// ============================================================================

document.addEventListener('DOMContentLoaded', function () {
    console.log("LTW Landing Page initialized - Designed by hoangthuhang");

    // 1. Tự động đóng navbar menu khi người dùng click vào một link trên mobile
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link, .navbar-nav .dropdown-item');
    const navbarCollapse = document.getElementById('navbarMainNavigation') || document.getElementById('navbarMain');

    if (navbarCollapse && typeof bootstrap !== 'undefined') {
        const bsCollapse = new bootstrap.Collapse(navbarCollapse, { toggle: false });
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992 && navbarCollapse.classList.contains('show')) {
                    bsCollapse.hide();
                }
            });
        });
    }

    // 2. Khởi tạo và kích hoạt Bootstrap Carousel cho Testimonials
    const testimonialEl = document.getElementById('testimonialCarousel');
    if (testimonialEl && typeof bootstrap !== 'undefined') {
        new bootstrap.Carousel(testimonialEl, {
            interval: 5000,
            ride: 'carousel',
            wrap: true
        });
    }

    // 3. Highlight liên kết navbar theo URL hiện tại nếu chưa được đánh dấu active bởi server
    const currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    let hasServerActive = document.querySelector('.navbar-iot-clean .nav-link.active');
    if (!hasServerActive) {
        navLinks.forEach(function (link) {
            const href = (link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '') || '/';
            if (href.startsWith('#')) return;

            if (currentPath === href || 
                (href !== '/' && currentPath.startsWith(href + '/')) ||
                (href === '/' && (currentPath === '' || currentPath === '/home' || currentPath === '/home/index'))) {
                link.classList.add('active');
                const dropdown = link.closest('.dropdown');
                if (dropdown) {
                    const toggle = dropdown.querySelector('.dropdown-toggle');
                    if (toggle) toggle.classList.add('active');
                }
            }
        });
    }

    // 4. Highlight liên kết navbar theo vị trí cuộn trang (nếu có các liên kết dạng #sectionId)
    window.addEventListener('scroll', function () {
        const sections = document.querySelectorAll('section[id]');
        const scrollPos = window.scrollY + 100;

        sections.forEach(function (section) {
            const sectionTop = section.offsetTop;
            const sectionHeight = section.offsetHeight;
            const sectionId = section.getAttribute('id');

            if (scrollPos >= sectionTop && scrollPos < sectionTop + sectionHeight) {
                navLinks.forEach(function (link) {
                    if (link.getAttribute('href') === '#' + sectionId) {
                        link.classList.add('active');
                    } else if (link.getAttribute('href')?.startsWith('#')) {
                        link.classList.remove('active');
                    }
                });
            }
        });
    });
});

