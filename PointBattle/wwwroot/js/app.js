/**
 * Sets the document's text direction to RTL or LTR and updates the UI layout accordingly.
 *
 * Applies the specified direction to the `<html>` element, toggles relevant CSS classes for RTL or LTR mode, and triggers layout and UI element adjustments to ensure consistent appearance. Returns `true` if the operation succeeds, or `false` if an error occurs.
 *
 * @param {string} dir - The desired text direction, either 'rtl' or 'ltr'.
 * @returns {boolean} `true` if the direction was set successfully; `false` if an error occurred.
 */
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

/**
 * Adjusts UI element alignment and layout based on the specified text direction.
 *
 * Updates text alignment for input fields, sets flex direction for button containers, and toggles RTL-specific classes on certain elements to match the given direction.
 *
 * @param {string} dir - The text direction, either 'rtl' or 'ltr'.
 */
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

/**
 * Adjusts the fixed positioning and padding of key UI elements to maintain correct layout for the specified text direction.
 *
 * Ensures the `.top-row` and `.navbar-toggler` elements are fixed at the top of the viewport, with horizontal alignment based on {@link dir}. Adds top padding to the `<main>` element on small screens to prevent overlap.
 *
 * @param {string} dir - The text direction, either 'rtl' or 'ltr'.
 */
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

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log("JavaScript: DOM content loaded - initializing app.js");

    try {
        // Get current language and apply appropriate direction
        const savedLanguage = localStorage.getItem("app_language") || "en";
        const isRtl = savedLanguage.toLowerCase().startsWith("ckb") || savedLanguage.toLowerCase().startsWith("ar");
        setDirection(isRtl ? "rtl" : "ltr");
        console.log(`JavaScript: Applied direction for language ${savedLanguage}: ${isRtl ? "rtl" : "ltr"}`);
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