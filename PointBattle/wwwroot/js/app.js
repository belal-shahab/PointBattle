// Set document direction for RTL/LTR
function setDirection(dir) {
    console.log(`JavaScript: Setting document direction to: ${dir}`);

    try {
        // Set direction on HTML element
        document.documentElement.setAttribute('dir', dir);
        document.documentElement.style.direction = dir;

        // Apply additional layout adjustments for RTL/LTR
        if (dir === 'rtl') {
            document.body.classList.add('rtl-mode');
            document.body.classList.remove('ltr-mode');
            document.documentElement.classList.add('rtl');
        } else {
            document.body.classList.remove('rtl-mode');
            document.body.classList.add('ltr-mode');
            document.documentElement.classList.remove('rtl');
        }

        // Apply layout adjustments
        adjustLayoutForDirection(dir);

        // Ensure UI elements stay in correct positions
        setTimeout(() => {
            ensureUIElementsPosition(dir);
        }, 100);

        console.log(`JavaScript: Document direction set to: ${dir}`);
        return true;
    } catch (error) {
        console.error("JavaScript: Error setting direction:", error);
        return false;
    }
}

// Handle layout adjustments when direction changes
function adjustLayoutForDirection(dir) {
    try {
        // Fix input field alignments
        const inputs = document.querySelectorAll('input[type="text"], input[type="number"], textarea');
        inputs.forEach(input => {
            input.style.textAlign = (dir === 'rtl') ? 'right' : 'left';
        });

        // Fix button layout for specific containers only
        const gameButtonContainers = document.querySelectorAll('.button-container, .modal-actions');
        gameButtonContainers.forEach(container => {
            if (dir === 'rtl') {
                container.style.flexDirection = 'row-reverse';
            } else {
                container.style.flexDirection = 'row';
            }
        });

        // Adjust RTL-specific elements but NOT navbar or top-row
        const rtlElements = document.querySelectorAll('.team-score, .round-actions, .game-card-actions');
        rtlElements.forEach(elem => {
            if (dir === 'rtl') {
                elem.classList.add('rtl-direction');
            } else {
                elem.classList.remove('rtl-direction');
            }
        });

        console.log("Layout adjustments applied");

    } catch (error) {
        console.error("JavaScript: Error adjusting layout:", error);
    }
}

// Ensure UI elements stay in correct positions
function ensureUIElementsPosition(dir) {
    try {
        // Keep top-row and navbar elements in fixed positions
        const topRow = document.querySelector('.top-row');
        const navbar = document.querySelector('.navbar-toggler');
        const languageSwitcher = document.querySelector('.language-switcher');

        if (topRow) {
            topRow.style.position = 'fixed';
            topRow.style.top = '0';
            topRow.style.left = '0';
            topRow.style.right = '0';
            topRow.style.zIndex = '200';
        }

        if (navbar) {
            navbar.style.position = 'fixed';
            navbar.style.top = '0.5rem';
            navbar.style.zIndex = '1002';

            if (dir === 'rtl') {
                navbar.style.left = '1rem';
                navbar.style.right = 'auto';
            } else {
                navbar.style.right = '1rem';
                navbar.style.left = 'auto';
            }
        }

        // Ensure main content has proper padding
        const main = document.querySelector('main');
        if (main && window.innerWidth <= 640) {
            main.style.paddingTop = '3.5rem';
        }

        console.log("UI elements positioned correctly for direction:", dir);

    } catch (error) {
        console.error("JavaScript: Error positioning UI elements:", error);
    }
}

// Mobile Input Focus and Keyboard Handling
let activeInput = null;
let originalViewport = null;
let keyboardHeight = 0;

function setupMobileInputHandling() {
    console.log("Setting up mobile input handling...");

    // Store original viewport
    const viewportMeta = document.querySelector('meta[name="viewport"]');
    if (viewportMeta) {
        originalViewport = viewportMeta.getAttribute('content');
    }

    // Add event listeners to all inputs
    document.addEventListener('focusin', handleInputFocus, true);
    document.addEventListener('focusout', handleInputBlur, true);

    // Handle viewport changes (keyboard appearance)
    window.addEventListener('resize', handleViewportResize);

    // Visual viewport API for better keyboard detection
    if (window.visualViewport) {
        window.visualViewport.addEventListener('resize', handleVisualViewportChange);
    }
}

function handleInputFocus(event) {
    const input = event.target;
    if (input && (input.type === 'text' || input.type === 'number' || input.tagName === 'TEXTAREA')) {
        console.log("Input focused:", input);
        activeInput = input;

        // Add focused class for styling
        input.classList.add('input-focused');

        // Scroll input into view after a short delay
        setTimeout(() => {
            scrollInputIntoView(input);
        }, 300);

        // Prevent zoom on iOS
        if (isIOS()) {
            input.style.fontSize = '16px';
        }
    }
}

function handleInputBlur(event) {
    const input = event.target;
    if (input && (input.type === 'text' || input.type === 'number' || input.tagName === 'TEXTAREA')) {
        console.log("Input blurred:", input);
        input.classList.remove('input-focused');

        if (activeInput === input) {
            activeInput = null;
        }

        // Reset font size
        if (isIOS() && input.style.fontSize === '16px') {
            input.style.fontSize = '';
        }
    }
}

function scrollInputIntoView(input) {
    if (!input) return;

    try {
        console.log("Scrolling input into view:", input);

        // Calculate the position to scroll to
        const inputRect = input.getBoundingClientRect();
        const viewportHeight = window.innerHeight;

        // Calculate keyboard height (estimated)
        const estimatedKeyboardHeight = Math.max(250, viewportHeight * 0.35);

        // Calculate the target scroll position
        const targetY = inputRect.top + window.pageYOffset - (viewportHeight - estimatedKeyboardHeight) / 2;

        // Smooth scroll to the input
        window.scrollTo({
            top: Math.max(0, targetY),
            behavior: 'smooth'
        });

        // Alternative method using scrollIntoView
        setTimeout(() => {
            input.scrollIntoView({
                behavior: 'smooth',
                block: 'center',
                inline: 'nearest'
            });
        }, 100);

    } catch (error) {
        console.error("Error scrolling input into view:", error);
    }
}

function handleViewportResize() {
    if (activeInput) {
        setTimeout(() => {
            scrollInputIntoView(activeInput);
        }, 100);
    }
}

function handleVisualViewportChange() {
    if (!window.visualViewport || !activeInput) return;

    const viewport = window.visualViewport;
    keyboardHeight = window.innerHeight - viewport.height;

    console.log("Visual viewport changed, keyboard height:", keyboardHeight);

    if (keyboardHeight > 100) { // Keyboard is open
        setTimeout(() => {
            scrollInputIntoView(activeInput);
        }, 50);
    }
}

function isIOS() {
    return /iPad|iPhone|iPod/.test(navigator.userAgent);
}

function isAndroid() {
    return /Android/.test(navigator.userAgent);
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log("JavaScript: DOM content loaded - initializing app.js");

    try {
        // Get current language and apply appropriate direction
        const savedLanguage = localStorage.getItem("app_language") || "en";
        const isRtl = savedLanguage.toLowerCase().startsWith("ckb") || savedLanguage.toLowerCase().startsWith("ar");
        setDirection(isRtl ? "rtl" : "ltr");
        console.log(`JavaScript: Applied direction for language ${savedLanguage}: ${isRtl ? "rtl" : "ltr"}`);

        // Setup mobile input handling
        setupMobileInputHandling();

    } catch (error) {
        console.error("JavaScript: Error during initialization:", error);
    }

    console.log("JavaScript: App.js initialization complete");
});

// Handle window resize to maintain layout
window.addEventListener('resize', function() {
    const currentDir = document.documentElement.getAttribute('dir') || 'ltr';
    setTimeout(() => {
        ensureUIElementsPosition(currentDir);
    }, 100);
});

// Blazor interop functions
window.scrollToInput = function(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        setTimeout(() => {
            scrollInputIntoView(input);
        }, 100);
    }
};

window.setupInputFocus = function() {
    setupMobileInputHandling();
};
// Dark mode functionality
window.setDarkMode = function(isDarkMode) {
    console.log("Setting dark mode:", isDarkMode);

    const html = document.documentElement;
    const body = document.body;

    if (isDarkMode) {
        html.setAttribute('data-theme', 'dark');
        body.classList.add('dark-mode');
        body.classList.remove('light-mode');
    } else {
        html.setAttribute('data-theme', 'light');
        body.classList.remove('dark-mode');
        body.classList.add('light-mode');
    }

    // Also apply to any dynamically created elements
    const page = document.querySelector('.page');
    if (page) {
        page.setAttribute('data-theme', isDarkMode ? 'dark' : 'light');
    }

    return true;
};

// Initialize dark mode on page load
document.addEventListener('DOMContentLoaded', function() {
    try {
        const savedDarkMode = localStorage.getItem('darkMode') === 'true';
        setDarkMode(savedDarkMode);
    } catch (error) {
        console.error("Error initializing dark mode:", error);
    }
});