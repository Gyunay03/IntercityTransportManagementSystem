window.initFilter = function () {
    const filterForm = document.getElementById("filterForm");

    if (!filterForm) return;

    const selects = filterForm.querySelectorAll("select");
    selects.forEach(select => {
        select.replaceWith(select.cloneNode(true));
    });

    filterForm.querySelectorAll("select").forEach(select => {
        select.addEventListener("change", () => {
            filterForm.dispatchEvent(new Event("submit", { bubbles: true }));
        });
    });

    const searchInput = document.getElementById("searchString");
    if (searchInput) {
        searchInput.replaceWith(searchInput.cloneNode(true));
        document.getElementById("searchString").addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                filterForm.dispatchEvent(new Event("submit", { bubbles: true }));
            }
        });
    }
};