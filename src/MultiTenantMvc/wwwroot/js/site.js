// Base Global Client-Side Script
// Used by: Base Site (default) and any tenant without a custom script override (e.g. Tenant Alpha)
document.addEventListener("DOMContentLoaded", function () {
    console.log("%c[Base Platform Script] Running /js/site.js", "color: #2563eb; font-weight: bold; font-size: 14px;");

    const container = document.getElementById("tenant-js-output");
    if (container) {
        container.innerHTML = `
            <div class="d-flex align-items-center justify-content-between">
                <div>
                    <span class="badge bg-primary mb-1">Global Base Script Active</span>
                    <div class="fw-bold text-dark">/js/site.js</div>
                    <div class="small text-muted">Standard platform scripts initialized. All default event handlers and tracking are active.</div>
                </div>
                <div class="text-end">
                    <span class="badge bg-light text-secondary border">DOM Ready</span>
                </div>
            </div>
        `;
    }
});
