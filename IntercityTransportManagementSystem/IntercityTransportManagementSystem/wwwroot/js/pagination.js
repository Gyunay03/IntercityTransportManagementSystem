window.initPagination = function () {
    const paginationLinks = document.querySelectorAll(".pagination a");

    if (paginationLinks.length === 0) return;

    paginationLinks.forEach(link => {
        link.replaceWith(link.cloneNode(true));
    });

    document.querySelectorAll(".pagination a").forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();

            const url = new URL(this.href);
            const currentUrl = new URL(window.location.href);

            currentUrl.searchParams.forEach((value, key) => {
                url.searchParams.set(key, value);
                fetchReservations(url.toString());
            });
        });
    });
}

document.addEventListener("click", function (e) {

    if (e.target.matches(".pagination a")) {
        e.preventDefault();

        const container = document.getElementById("reservationsContainer");

        fetch(e.target.href, {
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        })
        .then(res => res.text())
        .then(html => {
            container.innerHTML = html;
        });
    }
});