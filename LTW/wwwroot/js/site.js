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

    // 1. Tự động đóng navbar menu khi người dùng click vào một link điều hướng trên mobile (loại trừ dropdown-toggle)
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link:not(.dropdown-toggle), .navbar-nav .dropdown-item');
    const navbarCollapse = document.getElementById('navbarMainNavigation') || document.getElementById('navbarMain');

    if (navbarCollapse && typeof bootstrap !== 'undefined') {
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992 && navbarCollapse.classList.contains('show')) {
                    const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse) || new bootstrap.Collapse(navbarCollapse, { toggle: false });
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

    // 5. Footer Interactive Campus Map Switcher
    const campusMapData = {
        caugiay: {
            embed: "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3724.096956637841!2d105.80053937587095!3d21.02880748777717!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3135ab43ad1a017b%3A0xbcf046cb51a2d659!2zMyBD4bqndSBHaeG6pXksIE5n4buNYyBLaMOhbmgsIMSQ4buRbmcgxJBhLCBIw6AgTuG7mWksIFZp4buHdCBOYW0!5e0!3m2!1svi!2s!4v1700000000000!5m2!1svi!2s",
            external: "https://maps.google.com/?q=3+Cau+Giay+Lang+Thuong+Dong+Da+Hanoi"
        },
        landmark: {
            embed: "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3724.485507963404!2d105.78135897587056!3d21.013249787980806!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3135ab57849e701d%3A0xdba343b6dc08a3d3!2sKeangnam%20Hanoi%20Landmark%20Tower!5e0!3m2!1svi!2s!4v1700000000001!5m2!1svi!2s",
            external: "https://maps.google.com/?q=Keangnam+Landmark+72+Nam+Tu+Liem+Hanoi"
        },
        hcm: {
            embed: "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.517826500473!2d106.70183217573426!3d10.771594989376788!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f41712a2305%3A0x6b7106fe1d02c89!2sBitexco%20Financial%20Tower!5e0!3m2!1svi!2s!4v1700000000002!5m2!1svi!2s",
            external: "https://maps.google.com/?q=Bitexco+Financial+Tower+Hai+Trieu+District+1+Ho+Chi+Minh"
        }
    };

    const campusButtons = document.querySelectorAll('.campus-switch-btn');
    const mapIframe = document.getElementById('footerGoogleMapIframe');
    const externalLink = document.getElementById('footerMapExternalLink');

    if (campusButtons.length > 0 && mapIframe && externalLink) {
        campusButtons.forEach(function (btn) {
            btn.addEventListener('click', function () {
                const campusKey = this.getAttribute('data-campus');
                if (campusMapData[campusKey]) {
                    // Update Active styling
                    campusButtons.forEach(b => b.classList.remove('active-campus'));
                    this.classList.add('active-campus');

                    // Update Iframe and Link
                    mapIframe.src = campusMapData[campusKey].embed;
                    externalLink.href = campusMapData[campusKey].external;
                }
            });
        });
    }
});

