// app.js - COMPLETE REPLACEMENT
// Set document direction for RTL/LTR
function setDirection(dir) {
    console.log(`Setting document direction to: ${dir}`);
    
    // Set direction on HTML element
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.style.direction = dir;

    // Apply additional layout adjustments for RTL/LTR
    if (dir === 'rtl') {
        document.body.classList.add('rtl-mode');
        document.body.classList.remove('ltr-mode');
    } else {
        document.body.classList.remove('rtl-mode');
        document.body.classList.add('ltr-mode');
    }

    // Apply any needed layout adjustments
    adjustLayoutForDirection(dir);

    console.log(`Document direction set to: ${dir}`);
}

// Handle layout adjustments when direction changes
function adjustLayoutForDirection(dir) {
    // Fix input field alignments
    const inputs = document.querySelectorAll('input');
    
    inputs.forEach(input => {
        if (input.type === 'text' || input.type === 'number') {
            input.style.textAlign = (dir === 'rtl') ? 'right' : 'left';
        }
    });

    // Fix button layout
    const buttonContainers = document.querySelectorAll('.button-container');
    buttonContainers.forEach(container => {
        if (dir === 'rtl') {
            container.style.flexDirection = 'row-reverse';
        } else {
            container.style.flexDirection = 'row';
        }
    });
    
    // Adjust other RTL-specific elements
    const rtlElements = document.querySelectorAll('.nav-item, .team-score, .round-actions, .game-card-actions');
    rtlElements.forEach(elem => {
        if (dir === 'rtl') {
            elem.classList.add('rtl-direction');
        } else {
            elem.classList.remove('rtl-direction');
        }
    });
}

// Set up input focus handling
function setupInputFocus() {
    console.log("Setting up input focus handlers");

    // Add event listeners for handling viewport adjustments on mobile
    window.addEventListener('resize', function() {
        // This will help adjust the view when keyboard appears/disappears on mobile
        if (document.activeElement.tagName === 'INPUT') {
            setTimeout(() => document.activeElement.scrollIntoView({ behavior: 'smooth', block: 'center' }), 100);
        }

        // Reapply RTL/LTR adjustments on resize
        const dir = document.documentElement.dir || 'ltr';
        adjustLayoutForDirection(dir);
    });
}

// Scroll to input when focused - especially helpful on mobile
function scrollToInput(inputId) {
    console.log("Scrolling to input: " + inputId);

    // Scroll to ensure input is visible, especially on mobile
    const inputElement = document.activeElement;
    if (inputElement) {
        setTimeout(() => inputElement.scrollIntoView({ behavior: 'smooth', block: 'center' }), 100);
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log("DOM content loaded - initializing app.js");
    
    // Check if we just did a language change
    if (localStorage.getItem("app_language_changed") === "true") {
        console.log("Detected completed language change reload");
        localStorage.removeItem("app_language_changed");
        
        // Get current language and apply appropriate direction
        const savedLanguage = localStorage.getItem("app_language") || "en";
        const isRtl = savedLanguage === "ckb";
        setDirection(isRtl ? "rtl" : "ltr");
        console.log(`Applied direction for language ${savedLanguage}: ${isRtl ? "rtl" : "ltr"}`);
    }
    
    // Get current direction from HTML element or default to LTR
    const currentDir = document.documentElement.dir || 'ltr';
    adjustLayoutForDirection(currentDir);
    setupInputFocus();
    
    console.log("App.js initialization complete");
});

// Expose function to check if RTL is active - can be called from Blazor
function isRTLActive() {
    return document.documentElement.dir === 'rtl';
}

// Function to change language with proper UI update
function changeLanguage(languageCode) {
    console.log(`Changing language to: ${languageCode}`);
    
    // First update the direction
    setDirection(languageCode === 'ckb' ? 'rtl' : 'ltr');
    
    // Store the language code for potential use before reload
    localStorage.setItem('app_language', languageCode);
    
    // Return true to indicate success
    return true;
}

// Function to force a complete page reload
function forcePageReload() {
    console.log("Forcing complete page reload for language change");
    
    // Set a flag to indicate we're doing a language change
    localStorage.setItem("app_language_changed", "true");
    
    // Use more aggressive reload approach
    window.location.href = window.location.href.split('#')[0];
    
    // If running in a WebView (MAUI), try additional approaches
    try {
        // Adding a small random parameter forces bypass of cache
        let separator = window.location.href.indexOf('?') > -1 ? '&' : '?';
        window.location.href = window.location.href.split('#')[0] + 
                               separator + 
                               '_refresh=' + new Date().getTime();
    } catch (e) {
        console.warn("Failed advanced reload:", e);
        // Fallback to standard reload
        window.location.reload(true);
    }
}