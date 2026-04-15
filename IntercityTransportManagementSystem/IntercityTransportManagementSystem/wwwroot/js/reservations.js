import './filter.js'
import './pagination.js'

document.addEventListener("DOMContentLoaded", function () {
    console.log("Reservations page loaded.");

    initDynamicElements();

    document.getElementById("filterForm")?.addEventListener("submit", function (e) {
        e.preventDefault();
        fetchReservations();
    });
});

function fetchReservations(url) {
    url = url || window.location.href;

    fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(res => res.text())
        .then(html => {
            document.getElementById("reservationsContainer").innerHTML = html;
            initDynamicElements();
        })
        .catch(err => console.error(err));
}

function initDynamicElements() {
    if (window.initFilter) window.initFilter();
    if (window.initPagination) window.initPagination();
}