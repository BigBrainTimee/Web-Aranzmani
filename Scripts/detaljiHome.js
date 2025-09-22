document.addEventListener("DOMContentLoaded", () => {
    const container = document.querySelector("#smestajiContainer");
    const originalCards = Array.from(document.querySelectorAll(".smestaj-card"));

    // Filter polja
    const filters = {
        naziv: document.querySelector("#filterNaziv"),
        tip: document.querySelector("#filterTip"),
        bazen: document.querySelector("#filterBazen"),
        spa: document.querySelector("#filterSpa"),
        wifi: document.querySelector("#filterWifi"),
        invaliditet: document.querySelector("#filterInvaliditet"),
        minGostiju: document.querySelector("#minGostiju"),
        maxGostiju: document.querySelector("#maxGostiju"),
        ljubimci: document.querySelector("#filterLjubimci"),
        maxCena: document.querySelector("#maxCena"),
        sortSmestaji: document.querySelector("#sortSmestaji"),
        sortJedinice: document.querySelector("#sortJedinice")
    };

    const btnApply = document.querySelector("#applyFilters");
    const btnReset = document.querySelector("#resetFilters");

    // ------------------ FUNKCIJE ------------------

    // Glavna logika filtera
    function applyFilters() {
        let cards = [...originalCards];

        // Filtar po atributima
        cards = cards.filter(card => {
            const d = card.dataset;
            const matches =
                (!filters.naziv.value || d.naziv.toLowerCase().includes(filters.naziv.value.toLowerCase())) &&
                (!filters.tip.value || d.tip === filters.tip.value.toLowerCase()) &&
                (!filters.bazen.value || d.bazen === filters.bazen.value) &&
                (!filters.spa.value || d.spa === filters.spa.value) &&
                (!filters.wifi.value || d.wifi === filters.wifi.value) &&
                (!filters.invaliditet.value || d.invaliditet === filters.invaliditet.value);

            return matches;
        });

        // Filtar po jedinicama
        cards = cards.filter(card => {
            const jedinice = Array.from(card.querySelectorAll(".jedinica-row"));
            return jedinice.some(row => {
                const gostiju = parseInt(row.dataset.gostiju || 0);
                const cena = parseFloat(row.dataset.cena || 0);
                const ljubimci = row.dataset.ljubimci;

                return (!filters.minGostiju.value || gostiju >= parseInt(filters.minGostiju.value)) &&
                    (!filters.maxGostiju.value || gostiju <= parseInt(filters.maxGostiju.value)) &&
                    (!filters.ljubimci.value || ljubimci === filters.ljubimci.value) &&
                    (!filters.maxCena.value || cena <= parseFloat(filters.maxCena.value));
            });
        });

        // Sortiranje smeštaja
        if (filters.sortSmestaji.value) {
            const sortMap = {
                nazivAsc: (a, b) => a.dataset.naziv.localeCompare(b.dataset.naziv),
                nazivDesc: (a, b) => b.dataset.naziv.localeCompare(a.dataset.naziv),
                jediniceAsc: (a, b) => parseInt(a.dataset.jedinice) - parseInt(b.dataset.jedinice),
                jediniceDesc: (a, b) => parseInt(b.dataset.jedinice) - parseInt(a.dataset.jedinice),
                slobodneAsc: (a, b) => parseInt(a.dataset.slobodne) - parseInt(b.dataset.slobodne),
                slobodneDesc: (a, b) => parseInt(b.dataset.slobodne) - parseInt(a.dataset.slobodne)
            };
            cards.sort(sortMap[filters.sortSmestaji.value]);
        }

        // Sortiranje jedinica unutar svake kartice
        if (filters.sortJedinice.value) {
            const sortMapJedinice = {
                gostAsc: (a, b) => parseInt(a.dataset.gostiju) - parseInt(b.dataset.gostiju),
                gostDesc: (a, b) => parseInt(b.dataset.gostiju) - parseInt(a.dataset.gostiju),
                cenaAsc: (a, b) => parseFloat(a.dataset.cena) - parseFloat(b.dataset.cena),
                cenaDesc: (a, b) => parseFloat(b.dataset.cena) - parseFloat(a.dataset.cena)
            };

            for (const card of cards) {
                const tbody = card.querySelector(".jedinice-table tbody");
                if (!tbody) continue;

                const rows = Array.from(tbody.querySelectorAll(".jedinica-row"));
                rows.sort(sortMapJedinice[filters.sortJedinice.value]);
                tbody.innerHTML = "";
                for (const row of rows) tbody.appendChild(row);
            }
        }

        // Renderuj filtrirane
        container.innerHTML = "";
        for (const c of cards) container.appendChild(c);
    }

    // Reset svih filtera
    function resetFilters() {
        for (const key in filters) {
            if (filters[key]?.tagName === "SELECT" || filters[key]?.tagName === "INPUT") {
                filters[key].value = "";
            }
        }
        container.innerHTML = "";
        for (const c of originalCards) container.appendChild(c);
    }

    // ------------------ EVENTI ------------------
    btnApply?.addEventListener("click", applyFilters);
    btnReset?.addEventListener("click", resetFilters);
});
