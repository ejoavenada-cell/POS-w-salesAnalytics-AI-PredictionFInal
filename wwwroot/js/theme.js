/**
 * Tasty Station Theme Manager
 * Handles persistent branding colors and dark mode settings
 */

const ThemeManager = {
    // Default branding colors
    defaultColors: {
        primary: '#2D9F96',
        secondary: '#3B82F6',
        accent: '#f43f5e'
    },

    init() {
        this.applySavedTheme();
    },

    getSavedTheme() {
        const saved = localStorage.getItem('tasty_station_theme');
        return saved ? JSON.parse(saved) : this.defaultColors;
    },

    saveTheme(colors) {
        localStorage.setItem('tasty_station_theme', JSON.stringify(colors));
        this.applyTheme(colors);
    },

    applyTheme(colors) {
        console.log("Applying theme:", colors);
        const root = document.documentElement;
        
        // Update both standard and POS-specific variables
        root.style.setProperty('--primary-color', colors.primary);
        root.style.setProperty('--pos-primary', colors.primary);
        
        // Calculate Light version (90% lightness)
        const lightColor = this.adjustBrightness(colors.primary, 180);
        root.style.setProperty('--primary-light', lightColor);
        root.style.setProperty('--pos-primary-light', lightColor);
        
        // Secondary & Accent
        if (colors.secondary) root.style.setProperty('--secondary-color', colors.secondary);
        if (colors.accent) root.style.setProperty('--accent-color', colors.accent);
    },

    applySavedTheme() {
        const colors = this.getSavedTheme();
        this.applyTheme(colors);
    },

    // Helper to lighten/darken hex colors correctly
    adjustBrightness(hex, amt) {
        let col = hex.replace('#', '');
        if (col.length === 3) col = col[0] + col[0] + col[1] + col[1] + col[2] + col[2];
        
        let num = parseInt(col, 16);
        let r = (num >> 16) + amt;
        let g = ((num >> 8) & 0x00FF) + amt;
        let b = (num & 0x0000FF) + amt;

        const clamp = (val) => Math.min(255, Math.max(0, val));
        
        r = clamp(r);
        g = clamp(g);
        b = clamp(b);

        return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
    }
};

// Auto-init on load
ThemeManager.init();
