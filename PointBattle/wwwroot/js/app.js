// Input focus handling for mobile and desktop
function setupInputFocus() {
    console.log("Setting up input focus handlers");

    // Add event listeners for handling viewport adjustments on mobile
    window.addEventListener('resize', function() {
        // This will help adjust the view when keyboard appears/disappears on mobile
        if (document.activeElement.tagName === 'INPUT') {
            setTimeout(() => document.activeElement.scrollIntoView({ behavior: 'smooth', block: 'center' }), 100);
        }
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

// Clear default value from input field
function clearInputValue(inputElement) {
    if (inputElement && inputElement.value === "0") {
        inputElement.value = "";
    }
}