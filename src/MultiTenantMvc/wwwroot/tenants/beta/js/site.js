// Tenant Beta Client-Side Script Override
// Overrides: /js/site.js when active tenant is 'beta'
// Discovered dynamically by TenantScriptTagHelper
document.addEventListener("DOMContentLoaded", function () {
    console.log("%c[Tenant Beta Custom Script] Running /tenants/beta/js/site.js", "color: #059669; font-weight: bold; font-size: 14px;");

    const container = document.getElementById("tenant-js-output");
    if (container) {
        container.innerHTML = `
            <div class="p-3 bg-success bg-opacity-10 border border-success rounded-3">
                <div class="d-flex align-items-center justify-content-between mb-2">
                    <span class="badge bg-success text-uppercase">⭐ Custom Tenant JS Override Active</span>
                    <span class="badge bg-dark">Tenant Beta Retail Engine</span>
                </div>
                <div class="fw-bold text-success fs-6">Loaded: /tenants/beta/js/site.js</div>
                <p class="small text-dark mt-1 mb-2">
                    This interactive component was injected exclusively by Tenant Beta's custom JavaScript override.
                    Base site and Tenant Alpha do not execute this code.
                </p>
                <div class="bg-white p-2 rounded border mt-2">
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="small fw-semibold text-secondary">Beta Live Flash Promo:</span>
                        <span id="beta-flash-countdown" class="badge bg-danger">ENDS IN 14m 52s</span>
                    </div>
                    <div class="d-flex gap-2 mt-2">
                        <button class="btn btn-sm btn-success w-100" onclick="alert('Tenant Beta custom action executed!')">
                            Claim Beta Retail Voucher
                        </button>
                    </div>
                </div>
            </div>
        `;

        // Interactive countdown simulation
        let seconds = 892;
        setInterval(() => {
            seconds--;
            if (seconds <= 0) seconds = 900;
            const mins = Math.floor(seconds / 60);
            const secs = seconds % 60;
            const badge = document.getElementById("beta-flash-countdown");
            if (badge) {
                badge.innerText = `ENDS IN ${mins}m ${secs.toString().padStart(2, '0')}s`;
            }
        }, 1000);
    }
});
