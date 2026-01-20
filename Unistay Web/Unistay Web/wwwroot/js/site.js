/* ============================================
   UNISTAY - MODERN INTERACTIONS & UTILITIES
   ============================================ */

// Theme Toggling Logic
const themeToggleBtn = document.getElementById('theme-toggle');
const lightIcon = document.querySelector('.theme-icon-light');
const darkIcon = document.querySelector('.theme-icon-dark');
const htmlElement = document.documentElement;

// Function to apply theme
function applyTheme(theme) {
    if (theme === 'dark') {
        htmlElement.setAttribute('data-theme', 'dark');
        if (lightIcon && darkIcon) {
            lightIcon.style.display = 'none';
            darkIcon.style.display = 'block';
        }
    } else {
        htmlElement.removeAttribute('data-theme');
        if (lightIcon && darkIcon) {
            lightIcon.style.display = 'block';
            darkIcon.style.display = 'none';
        }
    }
    localStorage.setItem('theme', theme);
}

// Initial check
const savedTheme = localStorage.getItem('theme') || 'light';
applyTheme(savedTheme);

// Event listener
if (themeToggleBtn) {
    themeToggleBtn.addEventListener('click', () => {
        const currentTheme = htmlElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        applyTheme(newTheme);
    });
}

// Smooth scroll polyfill for older browsers
if (!('scrollBehavior' in document.documentElement.style)) {
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/smoothscroll-polyfill@0.4.4/dist/smoothscroll.min.js';
    document.head.appendChild(script);
}

// Intersection Observer for animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const animateOnScroll = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animate-fade-in');
            animateOnScroll.unobserve(entry.target);
        }
    });
}, observerOptions);

// Observe all elements with animation classes
document.addEventListener('DOMContentLoaded', () => {
    const animatedElements = document.querySelectorAll('.stagger-animation > *, .hover-lift, .hover-scale');
    animatedElements.forEach(el => animateOnScroll.observe(el));
});

// Ripple effect for buttons
function createRipple(event) {
    const button = event.currentTarget;
    const ripple = document.createElement('span');
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;

    ripple.style.width = ripple.style.height = `${diameter}px`;
    ripple.style.left = `${event.clientX - button.offsetLeft - radius}px`;
    ripple.style.top = `${event.clientY - button.offsetTop - radius}px`;
    ripple.classList.add('ripple');

    const rippleElement = button.getElementsByClassName('ripple')[0];
    if (rippleElement) {
        rippleElement.remove();
    }

    button.appendChild(ripple);
}

// Add ripple effect to all buttons
document.querySelectorAll('.btn').forEach(button => {
    button.addEventListener('click', createRipple);
});

// Form validation with modern feedback
function validateForm(form) {
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    inputs.forEach(input => {
        const errorElement = input.parentElement.querySelector('.form-error');
        
        if (!input.value.trim()) {
            isValid = false;
            input.classList.add('is-invalid');
            if (errorElement) {
                errorElement.textContent = 'Trường này là bắt buộc';
            }
        } else {
            input.classList.remove('is-invalid');
            if (errorElement) {
                errorElement.textContent = '';
            }
        }
    });

    return isValid;
}

// Real-time form validation
document.querySelectorAll('input, select, textarea').forEach(input => {
    input.addEventListener('blur', function() {
        if (this.hasAttribute('required') && !this.value.trim()) {
            this.classList.add('is-invalid');
        } else {
            this.classList.remove('is-invalid');
        }
    });

    input.addEventListener('input', function() {
        if (this.classList.contains('is-invalid') && this.value.trim()) {
            this.classList.remove('is-invalid');
        }
    });
});

// Toast notification system
function showToast(message, type = 'info', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        animation: slideInRight 0.3s ease-out;
    `;
    toast.textContent = message;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideInRight 0.3s ease-out reverse';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// Image lazy loading
if ('loading' in HTMLImageElement.prototype) {
    const images = document.querySelectorAll('img[loading="lazy"]');
    images.forEach(img => {
        img.src = img.dataset.src;
    });
} else {
    // Fallback for browsers that don't support lazy loading
    const script = document.createElement('script');
    script.src = 'https://cdn.jsdelivr.net/npm/lazysizes@5.3.2/lazysizes.min.js';
    document.body.appendChild(script);
}

// Modal functionality
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// Close modal on backdrop click
document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
    backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) {
            backdrop.classList.remove('active');
            document.body.style.overflow = '';
        }
    });
});

// Tabs functionality
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', function() {
        const tabGroup = this.parentElement;
        const tabContent = tabGroup.nextElementSibling;
        
        // Remove active class from all tabs
        tabGroup.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        
        // Add active class to clicked tab
        this.classList.add('active');
        
        // Show corresponding content
        const targetId = this.dataset.target;
        if (targetId && tabContent) {
            tabContent.querySelectorAll('.tab-pane').forEach(pane => {
                pane.style.display = 'none';
            });
            const targetPane = document.getElementById(targetId);
            if (targetPane) {
                targetPane.style.display = 'block';
            }
        }
    });
});

// Dropdown functionality
document.querySelectorAll('[data-toggle="dropdown"]').forEach(toggle => {
    toggle.addEventListener('click', function(e) {
        e.stopPropagation();
        const dropdown = this.nextElementSibling;
        if (dropdown && dropdown.classList.contains('dropdown-menu')) {
            dropdown.classList.toggle('show');
        }
    });
});

// Close dropdowns when clicking outside
document.addEventListener('click', () => {
    document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
        menu.classList.remove('show');
    });
});

// Debounce function for search inputs
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Search with debounce
const searchInputs = document.querySelectorAll('input[type="search"]');
searchInputs.forEach(input => {
    input.addEventListener('input', debounce((e) => {
        console.log('Searching for:', e.target.value);
        // Add your search logic here
    }, 300));
});

// Scroll to top button
const scrollToTopBtn = document.createElement('button');
scrollToTopBtn.innerHTML = '↑';
scrollToTopBtn.className = 'btn btn-primary btn-icon';
scrollToTopBtn.style.cssText = `
    position: fixed;
    bottom: 20px;
    right: 20px;
    z-index: 1000;
    display: none;
    border-radius: 50%;
    width: 48px;
    height: 48px;
    font-size: 24px;
`;
scrollToTopBtn.setAttribute('aria-label', 'Scroll to top');
document.body.appendChild(scrollToTopBtn);

window.addEventListener('scroll', () => {
    if (window.pageYOffset > 300) {
        scrollToTopBtn.style.display = 'flex';
    } else {
        scrollToTopBtn.style.display = 'none';
    }
});

scrollToTopBtn.addEventListener('click', () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
});

// Price formatter
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

// Date formatter
function formatDate(date) {
    return new Intl.DateTimeFormat('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// Copy to clipboard
function copyToClipboard(text) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(text).then(() => {
            showToast('Đã sao chép vào clipboard', 'success');
        });
    } else {
        // Fallback for older browsers
        const textarea = document.createElement('textarea');
        textarea.value = text;
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
        showToast('Đã sao chép vào clipboard', 'success');
    }
}

// Share functionality
async function shareContent(title, text, url) {
    if (navigator.share) {
        try {
            await navigator.share({ title, text, url });
        } catch (err) {
            console.log('Error sharing:', err);
        }
    } else {
        copyToClipboard(url);
    }
}

// Favorite toggle
function toggleFavorite(element, itemId) {
    element.classList.toggle('active');
    const isFavorite = element.classList.contains('active');
    
    // Add your API call here
    console.log(`Item ${itemId} favorite status:`, isFavorite);
    
    showToast(
        isFavorite ? 'Đã thêm vào yêu thích' : 'Đã xóa khỏi yêu thích',
        'success'
    );
}

// Image preview before upload
function previewImage(input, previewElement) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            previewElement.src = e.target.result;
            previewElement.style.display = 'block';
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Add event listeners for file inputs
document.querySelectorAll('input[type="file"]').forEach(input => {
    input.addEventListener('change', function() {
        const preview = this.parentElement.querySelector('.image-preview');
        if (preview) {
            previewImage(this, preview);
        }
    });
});

// Keyboard navigation for accessibility
document.addEventListener('keydown', (e) => {
    // Close modal on Escape
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-backdrop.active').forEach(modal => {
            modal.classList.remove('active');
            document.body.style.overflow = '';
        });
    }
});

// Focus trap for modals
function trapFocus(element) {
    const focusableElements = element.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstFocusable = focusableElements[0];
    const lastFocusable = focusableElements[focusableElements.length - 1];

    element.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            if (e.shiftKey) {
                if (document.activeElement === firstFocusable) {
                    lastFocusable.focus();
                    e.preventDefault();
                }
            } else {
                if (document.activeElement === lastFocusable) {
                    firstFocusable.focus();
                    e.preventDefault();
                }
            }
        }
    });
}

// Apply focus trap to all modals
document.querySelectorAll('.modal').forEach(modal => {
    trapFocus(modal);
});

// Performance monitoring
if ('PerformanceObserver' in window) {
    const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
            console.log('Performance:', entry.name, entry.duration);
        }
    });
    observer.observe({ entryTypes: ['measure', 'navigation'] });
}

// Export functions for use in other scripts
window.UnistayUI = {
    showToast,
    openModal,
    closeModal,
    formatPrice,
    formatDate,
    copyToClipboard,
    shareContent,
    toggleFavorite,
    validateForm
};
