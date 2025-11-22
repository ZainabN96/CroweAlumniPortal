/*function cardTemplate(a) {
   //debugger;
    const hasImg = a.profilePicturePath && a.profilePicturePath.length > 0;
    return `
     <div class="col-sm-6 col-md-4 col-lg-3">
        <div class="member-card">
          <div class="image-wrapper">
          ${hasImg
        ? `<img src="${a.profilePicturePath}" class="missing-img" alt="${a.firstName}">`
            : `<div class="missing-img" style="background-color: #aeaeae;">
                   <div style="font-size:20px;font-weight:bold;">${(a.firstName + " " + a.lastName)}</div>
                 </div>`
            }
            <span class="message-icon">
              <img src="/assets/img/message_icon.jpeg" alt="Chat Icon">
            </span>
          </div>
          <div class="member-info">
            <h6>${(a.firstName + " " + a.lastName)}</h6>
            <small>${(a.jobTitle + " at " + a.employerOrganization)}</small><br>
            <small>${(a.city + ", " + a.country)}</small>
            <div class="btn-action-section">
             <button class="btn btn-chat btn-sm btn-outline-black btn-primary mt-2" data-user="${a.id}">Message</button>
           </div>
          </div>
        </div>
      </div>
    `;
}
*/
function cardTemplate(a) {
    const hasImg = a.profilePicturePath && a.profilePicturePath.length > 0;
    return `
     <div class="col-sm-6 col-md-4 col-lg-3">
        <div class="member-card">
          <div class="image-wrapper">
          ${hasImg
            ? `<img src="${a.profilePicturePath}" class="missing-img" alt="${a.firstName}">`
            : `<div class="missing-img" style="background-color: #aeaeae;">
                   <div style="font-size:20px;font-weight:bold;">${(a.firstName + " " + a.lastName)}</div>
               </div>`
        }
            <span class="message-icon" data-user="${a.id}">
              <img src="/assets/img/message_icon.jpeg" alt="Chat Icon">
            </span>
          </div>
          <div class="member-info">
            <h6>${(a.firstName + " " + a.lastName)}</h6>
            <small>${(a.jobTitle + " at " + a.employerOrganization)}</small><br>
            <small>${(a.city + ", " + a.country)}</small><br>
            <small class="text-muted">
                ${formatJourney(a.joiningDate, a.leavingDate)}
            </small>
            <div class="btn-action-section">
             <button class="btn btn-chat btn-sm btn-outline-black btn-primary mt-2" data-user="${a.id}">Message</button>
           </div>
          </div>
        </div>
      </div>
    `;
}
function formatJourney(joining, leaving) {
    if (!joining) return "";

    const j = new Date(joining);
    const l = new Date(leaving);

    const options = { year: 'numeric', month: 'short' };

    const jStr = j.toLocaleDateString("en-US", options);
    const lStr = l.toLocaleDateString("en-US", options);

    return `${jStr} — ${lStr}`;
}

//(function () {
//    const $grid = $("#alumniGrid");
//    const $empty = $("#noData");
//    const $input = $("#alumniSearch");
//    const $btn = $("#btnSearch");

//    // Utility: render list
//    function render(list) {
//        if (!list || list.length === 0) {
//            $grid.html("");
//            $empty.show();
//            return;
//        }
//        $empty.hide();
//        const html = list.map(cardTemplate).join("");
//        $grid.html(html);
//    }

//    // Loading indicator
//    function setLoading() {
//        $empty.hide();
//        $grid.html(`<div class="w-100 text-center py-4">Loading…</div>`);
//    }

//    // Fetch helpers
//    function loadAll() {
//        setLoading();
//        $.get("/api/alumni/all")
//            .done(render)
//            .fail(xhr => {
//                $grid.html(`<div class="w-100 text-danger py-4">Failed to load: ${xhr.responseText || xhr.status}</div>`);
//            });
//    }

//    function searchNow() {
//        const q = $input.val() || "";
//        loadAll(q);
//    }

//    // Debounce for typing
//    function debounce(fn, ms) {
//        let t = null;
//        return function (...args) {
//            clearTimeout(t);
//            t = setTimeout(() => fn.apply(this, args), ms);
//        };
//    }

//    const debouncedSearch = debounce(searchNow, 400);

//    // Wire events
//    $btn.on("click", searchNow);
//    $input.on("keyup", debouncedSearch);

//    // Initial load
//    loadAll();
//})();
(function () {
    const $grid = $("#alumniGrid");
    const $empty = $("#noData");

    const $inputSearch = $("#alumniSearch");
    const $btnSearch = $("#btnSearch");
    const $filterCountry = $("#filterCountry");
    const $filterCity = $("#filterCity");
    const $filterIndustry = $("#filterIndustry");

    let allAlumni = [];

    // current filters
    const state = {
        country: "all",
        city: "all",
        industry: "all",
        joinYear: "all",
        leaveYear: "all",
        search: ""
    };
    function render(list) {
        if (!list || list.length === 0) {
            $grid.html("");
            $empty.show();
            return;
        }
        $empty.hide();
        const html = list.map(cardTemplate).join("");
        $grid.html(html);
    }

    // Loading indicator
    function setLoading() {
        $empty.hide();
        $grid.html(`<div class="w-100 text-center py-4">Loading…</div>`);
    }

    // ---------- Build dropdown options from loaded data ----------
    function populateFilters() {
        const countries = new Set();
        const cities = new Set();
        const industries = new Set();
        const joinYears = new Set();
        const leaveYears = new Set();

        allAlumni.forEach(a => {
            if (a.joiningDate) {
                joinYears.add(new Date(a.joiningDate).getFullYear());
            }
            if (a.leavingDate) {
                leaveYears.add(new Date(a.leavingDate).getFullYear());
            }
        });

        // Fill dropdowns
        fillSelect($("#filterJoinYear"), joinYears, "All years");
        fillSelect($("#filterLeaveYear"), leaveYears, "All years");


        allAlumni.forEach(a => {
            if (a.country) countries.add(a.country);
            if (a.city) cities.add(a.city);
            if (a.industry) industries.add(a.industry);
        });

        // helper
        function fillSelect($sel, items, allLabel) {
            $sel.empty();
            $sel.append(new Option(allLabel, "all", true, true));
            Array.from(items).sort().forEach(x => {
                $sel.append(new Option(x, x));
            });
        }

        fillSelect($filterCountry, countries, "All countries");
        fillSelect($filterCity, cities, "All cities");
        fillSelect($filterIndustry, industries, "All industries");
    }
    function applyFilters() {
        const s = (state.search || "").trim().toLowerCase();
        const c = (state.country || "all").toLowerCase();
        const ci = (state.city || "all").toLowerCase();
        const ind = (state.industry || "all").toLowerCase();

        const filtered = allAlumni.filter(a => {
            const country = (a.country || "").toLowerCase();
            const city = (a.city || "").toLowerCase();
            const industry = (a.industry || "").toLowerCase();

            const matchesCountry = (c === "all" || country === c);
            const matchesCity = (ci === "all" || city === ci);
            const matchesIndustry = (ind === "all" || industry === ind);

            // 🔍 search match
            let matchesSearch = true;
            if (s) {
                const text = [
                    a.firstName,
                    a.lastName,
                    a.jobTitle,
                    a.employerOrganization,
                    a.city,
                    a.country
                ].filter(Boolean).join(" ").toLowerCase();

                matchesSearch = text.includes(s);
            }

            // 🧮 joining / leaving year per-item calculate
            const joinYearVal = a.joiningDate ? new Date(a.joiningDate).getFullYear().toString() : null;
            const leaveYearVal = a.leavingDate ? new Date(a.leavingDate).getFullYear().toString() : null;

            const matchJoinYear =
                (state.joinYear === "all" ||
                    (joinYearVal && joinYearVal === state.joinYear));

            const matchLeaveYear =
                (state.leaveYear === "all" ||
                    (leaveYearVal && leaveYearVal === state.leaveYear));

            return matchesCountry &&
                matchesCity &&
                matchesIndustry &&
                matchesSearch &&
                matchJoinYear &&
                matchLeaveYear;
        });

        render(filtered);
    }

    //function applyFilters() {
    //    const s = (state.search || "").trim().toLowerCase();
    //    const c = (state.country || "all").toLowerCase();
    //    const ci = (state.city || "all").toLowerCase();
    //    const ind = (state.industry || "all").toLowerCase();
    //    const matchJoinYear =
    //        (state.joinYear === "all" ||
    //            (a.joiningDate && new Date(a.joiningDate).getFullYear().toString() === state.joinYear));

    //    const matchLeaveYear =
    //        (state.leaveYear === "all" ||
    //            (a.leavingDate && new Date(a.leavingDate).getFullYear().toString() === state.leaveYear));


    //    const filtered = allAlumni.filter(a => {
    //        const country = (a.country || "").toLowerCase();
    //        const city = (a.city || "").toLowerCase();
    //        const industry = (a.industry || "").toLowerCase();

    //        const matchesCountry = (c === "all" || country === c);
    //        const matchesCity = (ci === "all" || city === ci);
    //        const matchesIndustry = (ind === "all" || industry === ind);

    //        let matchesSearch = true;
    //        if (s) {
    //            const text = [
    //                a.firstName, a.lastName,
    //                a.jobTitle, a.employerOrganization,
    //                a.city, a.country
    //            ].filter(Boolean).join(" ").toLowerCase();

    //            matchesSearch = text.includes(s);
    //        }

    //        //return matchesCountry && matchesCity && matchesIndustry && matchesSearch;
    //        return matchesCountry &&
    //            matchesCity &&
    //            matchesIndustry &&
    //            matchesSearch &&
    //            matchJoinYear &&
    //            matchLeaveYear;

    //    });

    //    render(filtered);
    //}

    // ---------- Load all alumni from API ----------
    function loadAll() {
        setLoading();
        $.get("/api/alumni/all")
            .done(list => {
                allAlumni = Array.isArray(list) ? list : [];
                populateFilters();
                applyFilters();  // initial render with default "all"
            })
            .fail(xhr => {
                $grid.html(`<div class="w-100 text-danger py-4">Failed to load: ${xhr.responseText || xhr.status}</div>`);
            });
    }

    // ---------- Debounce for search ----------
    function debounce(fn, ms) {
        let t = null;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), ms);
        };
    }

    const debouncedSearch = debounce(function () {
        state.search = $inputSearch.val() || "";
        applyFilters();
    }, 400);

    // ---------- Wire events ----------
    $btnSearch.on("click", function () {
        state.search = $inputSearch.val() || "";
        applyFilters();
    });

    $inputSearch.on("keyup", debouncedSearch);

    $filterCountry.on("change", function () {
        state.country = $(this).val() || "all";
        applyFilters();
    });

    $filterCity.on("change", function () {
        state.city = $(this).val() || "all";
        applyFilters();
    });

    $filterIndustry.on("change", function () {
        state.industry = $(this).val() || "all";
        applyFilters();
    });
    $("#filterJoinYear").on("change", function () {
        state.joinYear = $(this).val();
        applyFilters();
    });
    $("#filterLeaveYear").on("change", function () {
        state.leaveYear = $(this).val();
        applyFilters();
    });

    loadAll();
})();
