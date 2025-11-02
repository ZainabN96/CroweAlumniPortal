function cardTemplate(a) {
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

(function () {
    const $grid = $("#alumniGrid");
    const $empty = $("#noData");
    const $input = $("#alumniSearch");
    const $btn = $("#btnSearch");

    // Utility: render list
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

    // Fetch helpers
    function loadAll() {
        setLoading();
        $.get("/api/alumni/all")
            .done(render)
            .fail(xhr => {
                $grid.html(`<div class="w-100 text-danger py-4">Failed to load: ${xhr.responseText || xhr.status}</div>`);
            });
    }

    function searchNow() {
        const q = $input.val() || "";
        loadAll(q);
    }

    // Debounce for typing
    function debounce(fn, ms) {
        let t = null;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), ms);
        };
    }

    const debouncedSearch = debounce(searchNow, 400);

    // Wire events
    $btn.on("click", searchNow);
    $input.on("keyup", debouncedSearch);

    // Initial load
    loadAll();
})();