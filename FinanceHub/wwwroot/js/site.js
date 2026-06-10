document.addEventListener("DOMContentLoaded", () => {
    const shell = document.getElementById("appShell");
    const desktopToggle = document.getElementById("sidebarToggle");
    const mobileToggle = document.getElementById("mobileMenuToggle");
    const backdrop = document.getElementById("sidebarBackdrop");
    const accordionButtons = document.querySelectorAll(".sidebar-accordion-button");
    const mobileBreakpoint = 900;

    if (!shell || !desktopToggle || !mobileToggle || !backdrop) {
        return;
    }

    const updateDesktopToggle = () => {
        const collapsed = shell.classList.contains("sidebar-collapsed");
        desktopToggle.setAttribute("aria-expanded", String(!collapsed));
        desktopToggle.setAttribute("aria-label", collapsed ? "Expandir menu" : "Recolher menu");
    };

    if (window.innerWidth > mobileBreakpoint &&
        localStorage.getItem("financehub-sidebar-collapsed") === "true") {
        shell.classList.add("sidebar-collapsed");
    }
    updateDesktopToggle();

    accordionButtons.forEach((button) => {
        button.addEventListener("click", () => {
            if (shell.classList.contains("sidebar-collapsed") &&
                window.innerWidth > mobileBreakpoint) {
                shell.classList.remove("sidebar-collapsed");
                localStorage.setItem("financehub-sidebar-collapsed", "false");
                updateDesktopToggle();
            }
        });
    });

    desktopToggle.addEventListener("click", () => {
        if (window.innerWidth <= mobileBreakpoint) {
            shell.classList.remove("mobile-menu-open");
            mobileToggle.setAttribute("aria-expanded", "false");
            return;
        }

        shell.classList.toggle("sidebar-collapsed");
        localStorage.setItem(
            "financehub-sidebar-collapsed",
            String(shell.classList.contains("sidebar-collapsed"))
        );
        updateDesktopToggle();
    });

    mobileToggle.addEventListener("click", () => {
        shell.classList.toggle("mobile-menu-open");
        mobileToggle.setAttribute(
            "aria-expanded",
            String(shell.classList.contains("mobile-menu-open"))
        );
    });

    backdrop.addEventListener("click", () => {
        shell.classList.remove("mobile-menu-open");
        mobileToggle.setAttribute("aria-expanded", "false");
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > mobileBreakpoint) {
            shell.classList.remove("mobile-menu-open");
            mobileToggle.setAttribute("aria-expanded", "false");
        }
    });
});
