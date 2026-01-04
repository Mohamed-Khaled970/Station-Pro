// ============================================
// REPORT PAGE JAVASCRIPT
// ============================================

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    initializePeriodTabs();
    initializeCharts();
    animateCounters();
    animateProgressBars();
});

// ============================================
// PERIOD TABS
// ============================================

function initializePeriodTabs() {
    const tabs = document.querySelectorAll('.period-tab');

    tabs.forEach(tab => {
        tab.addEventListener('click', function (e) {
            // Remove active class from all tabs
            tabs.forEach(t => t.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');
        });
    });
}

// ============================================
// CHARTS INITIALIZATION
// ============================================

function initializeCharts() {
    // Charts removed - no longer needed
    // Keeping function for future use if needed
}

// Daily Revenue Chart
function initializeDailyRevenueChart(canvas) {
    const data = JSON.parse(canvas.dataset.chartData || '[]');

    if (data.length === 0) return;

    const ctx = canvas.getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.dateFormatted),
            datasets: [{
                label: 'Revenue',
                data: data.map(d => d.revenue),
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4,
                pointRadius: 4,
                pointHoverRadius: 6,
                pointBackgroundColor: '#3b82f6',
                pointBorderColor: '#fff',
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: '#1f2937',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    cornerRadius: 8,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'Revenue: EGP ' + context.parsed.y.toFixed(2);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return 'EGP ' + value;
                        }
                    },
                    grid: {
                        color: '#e5e7eb'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

// Hourly Usage Chart
function initializeHourlyUsageChart(canvas) {
    const data = JSON.parse(canvas.dataset.chartData || '[]');

    if (data.length === 0) return;

    const ctx = canvas.getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(d => d.timeRange),
            datasets: [{
                label: 'Sessions',
                data: data.map(d => d.sessionCount),
                backgroundColor: 'rgba(59, 130, 246, 0.8)',
                borderColor: '#3b82f6',
                borderWidth: 2,
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: '#1f2937',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    cornerRadius: 8,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            const revenue = data[context.dataIndex].revenue;
                            return [
                                'Sessions: ' + context.parsed.y,
                                'Revenue: EGP ' + revenue.toFixed(2)
                            ];
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    },
                    grid: {
                        color: '#e5e7eb'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

// Device Performance Chart
function initializeDevicePerformanceChart(canvas) {
    const data = JSON.parse(canvas.dataset.chartData || '[]');

    if (data.length === 0) return;

    const ctx = canvas.getContext('2d');
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.deviceName),
            datasets: [{
                data: data.map(d => d.totalRevenue),
                backgroundColor: [
                    '#3b82f6',
                    '#10b981',
                    '#8b5cf6',
                    '#f59e0b',
                    '#ef4444',
                    '#06b6d4'
                ],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 15,
                        font: {
                            size: 12,
                            weight: '600'
                        }
                    }
                },
                tooltip: {
                    backgroundColor: '#1f2937',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        label: function (context) {
                            const device = data[context.dataIndex];
                            return [
                                context.label,
                                'Revenue: EGP ' + device.totalRevenue.toFixed(2),
                                'Sessions: ' + device.sessionCount,
                                'Utilization: ' + device.utilizationPercentage.toFixed(1) + '%'
                            ];
                        }
                    }
                }
            }
        }
    });
}

// ============================================
// COUNTER ANIMATIONS
// ============================================

function animateCounters() {
    const counters = document.querySelectorAll('.summary-stat-value[data-value]');

    counters.forEach(counter => {
        const target = parseFloat(counter.dataset.value);
        const isCurrency = counter.dataset.currency === 'true';

        // Skip animation if value is NaN or text-based
        if (isNaN(target)) return;

        const duration = 1500; // 1.5 seconds
        const startTime = performance.now();

        // Use requestAnimationFrame for smoother animation
        function animate(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function for smooth deceleration
            const easeOutQuad = progress * (2 - progress);
            const current = target * easeOutQuad;

            if (isCurrency) {
                counter.textContent = 'EGP ' + current.toFixed(2);
            } else {
                counter.textContent = Math.floor(current).toLocaleString();
            }

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                // Ensure final value is exact
                if (isCurrency) {
                    counter.textContent = 'EGP ' + target.toFixed(2);
                } else {
                    counter.textContent = Math.floor(target).toLocaleString();
                }
            }
        }

        requestAnimationFrame(animate);
    });
}

// Format duration (minutes to HH:MM:SS)
function formatDuration(minutes) {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    return `${hours}h ${mins}m`;
}

// ============================================
// PROGRESS BAR ANIMATIONS
// ============================================

function animateProgressBars() {
    const progressBars = document.querySelectorAll('.performance-progress-bar, .hourly-bar');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const bar = entry.target;
                const targetWidth = bar.dataset.width;

                setTimeout(() => {
                    bar.style.width = targetWidth;
                }, 100);

                observer.unobserve(bar);
            }
        });
    }, {
        threshold: 0.1
    });

    progressBars.forEach(bar => {
        bar.style.width = '0%';
        observer.observe(bar);
    });
}

// ============================================
// EXPORT FUNCTIONS
// ============================================

function exportToCSV(period) {
    window.location.href = `/Report/ExportCsv?period=${period}`;

    showToast('success', 'Exporting report...', 'Your CSV file will download shortly.');
}

function printReport(period) {
    window.open(`/Report/Print?period=${period}`, '_blank');
}

// ============================================
// TOAST NOTIFICATIONS
// ============================================

function showToast(type, title, message) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;

    const icon = type === 'success' ? 'fa-check-circle' :
        type === 'error' ? 'fa-exclamation-circle' :
            'fa-info-circle';

    toast.innerHTML = `
        <i class="fas ${icon} text-xl"></i>
        <div>
            <div class="font-semibold">${title}</div>
            <div class="text-sm text-gray-600">${message}</div>
        </div>
    `;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ============================================
// TABLE SORTING
// ============================================

function initializeTableSorting() {
    const headers = document.querySelectorAll('.sessions-table th[data-sortable]');

    headers.forEach(header => {
        header.style.cursor = 'pointer';
        header.innerHTML += ' <i class="fas fa-sort text-gray-400 ml-1"></i>';

        header.addEventListener('click', function () {
            const table = this.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const columnIndex = Array.from(this.parentNode.children).indexOf(this);
            const currentOrder = this.dataset.order || 'asc';
            const newOrder = currentOrder === 'asc' ? 'desc' : 'asc';

            // Sort rows
            rows.sort((a, b) => {
                const aValue = a.children[columnIndex].textContent.trim();
                const bValue = b.children[columnIndex].textContent.trim();

                if (newOrder === 'asc') {
                    return aValue.localeCompare(bValue, undefined, { numeric: true });
                } else {
                    return bValue.localeCompare(aValue, undefined, { numeric: true });
                }
            });

            // Update table
            rows.forEach(row => tbody.appendChild(row));

            // Update header icons
            headers.forEach(h => {
                h.querySelector('i').className = 'fas fa-sort text-gray-400 ml-1';
                delete h.dataset.order;
            });

            this.dataset.order = newOrder;
            this.querySelector('i').className = `fas fa-sort-${newOrder === 'asc' ? 'up' : 'down'} text-blue-600 ml-1`;
        });
    });
}

// Initialize table sorting when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeTableSorting);
} else {
    initializeTableSorting();
}

// ============================================
// UTILITY FUNCTIONS
// ============================================

// Format currency
function formatCurrency(amount) {
    return 'EGP ' + parseFloat(amount).toFixed(2);
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

// Format time
function formatTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit'
    });
}