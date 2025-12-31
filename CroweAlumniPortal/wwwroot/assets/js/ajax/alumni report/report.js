
const API = "/api/AlumniReport";

let state = { page: 1, pageSize: 20, total: 0, items: [] };

function qParams(){
    return {
        search: $("#search").val() || "",
        city: $("#city").val() || "",
        country: $("#country").val() || "",
        qualification: $("#qualification").val() || "",
        page: state.page,
        pageSize: state.pageSize
    };
}

function load(){
    $("#tbl tbody").html(`<tr><td colspan="9" class="text-center py-4">Loading…</td></tr>`);

    $.get(API, qParams()).done(res=>{
        state.items = res.items || [];
        state.total = res.total || 0;
        renderTable();
        renderPager();
    }).fail(()=>{
        $("#tbl tbody").html(`<tr><td colspan="9" class="text-center text-danger py-4">Failed to load.</td></tr>`);
    });
}

function renderTable(){
    const rows = [];
    let idx = (state.page - 1) * state.pageSize;

    state.items.forEach(x=>{
        rows.push(`
            <tr>
            <td>${++idx}</td>
            <td>${(x.firstName||"")+" "+(x.lastName||"")}</td>
            <td>${x.emailAddress||""}</td>
            <td><code>${x.loginId||""}</code></td>
            <td>${x.qualification||""}</td>
            <td>${x.city||""}</td>
            <td>${x.country||""}</td>
            <td>${x.employerOrganization||""}</td>
            <td>${x.jobTitle||""}</td>
            </tr>`);
    });

    $("#tbl tbody").html(rows.length ? rows.join("") : `<tr><td colspan="9" class="text-center py-4">No data.</td></tr>`);
    $("#info").text(`${state.total} total alumni`);
}

function renderPager(){
    const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
    const btn = (p, lbl, dis=false, act=false) =>
        `<li class="page-item ${dis?'disabled':''} ${act?'active':''}">
            <a class="page-link" href="#" data-p="${p}">${lbl}</a>
        </li>`;

    let h = "";
    h += btn(Math.max(1, state.page-1), "&laquo;", state.page===1);
    for(let i=1;i<=pages && i<=10;i++){
        h += btn(i, i, false, i===state.page);
    }
    h += btn(Math.min(pages, state.page+1), "&raquo;", state.page===pages);

    $("#pager").html(h);
    $("#pager a").off("click").on("click", function(e){
        e.preventDefault();
        const p = +$(this).data("p");
        if(!isNaN(p)){ state.page = p; load(); }
    });
}

function exportUrl(type){
    const p = qParams();
    delete p.page; delete p.pageSize;
    const qs = $.param(p);
    return `${API}/export/${type}?${qs}`;
}

$("#btnApply").on("click", function(){
    state.pageSize = +$("#pageSize").val();
    state.page = 1;
    load();
});

$("#pageSize").on("change", function(){
    state.pageSize = +$(this).val();
    state.page = 1;
    load();
});

$("#btnExcel").on("click", ()=> window.open(exportUrl("excel"), "_blank"));
$("#btnPdf").on("click", ()=> window.open(exportUrl("pdf"), "_blank"));

$(function(){
    state.pageSize = +$("#pageSize").val();
    load();
});

/*const API = "/api/AlumniReport";

let state = { page: 1, pageSize: 20, total: 0, items: [] };

function qs() {
    return {
        page: state.page,
        pageSize: state.pageSize,
        name: ($("#fName").val() || "").trim(),
        email: ($("#fEmail").val() || "").trim(),
        city: ($("#fCity").val() || "").trim(),
        status: $("#fStatus").val() || "",
        from: $("#fFrom").val() || "",
        to: $("#fTo").val() || ""
    };
}

function buildQuery(obj) {
    const p = new URLSearchParams();
    Object.keys(obj).forEach(k => { if (obj[k] !== "" && obj[k] != null) p.append(k, obj[k]); });
    return p.toString();
}

function load() {
    $("#tbl tbody").html(`<tr><td colspan="7" class="text-center py-4">Loading…</td></tr>`);
    $.getJSON(`${API}/list?${buildQuery(qs())}`)
        .done(res => {
            state.items = res.items || [];
            state.total = res.total || 0;
            render();
            pager();
        })
        .fail(xhr => {
            $("#tbl tbody").html(`<tr><td colspan="7" class="text-center text-danger py-4">Failed (${xhr.status}).</td></tr>`);
        });
}

function render() {
    const rows = [];
    let idx = (state.page - 1) * state.pageSize;
    state.items.forEach(u => {
        rows.push(`
                <tr>
                    <td>${++idx}</td>
                    <td>${u.name || "—"}</td>
                    <td>${u.email || "—"}</td>
                    <td><code>${u.loginId || "—"}</code></td>
                    <td>${u.city || "—"}</td>
                    <td>${u.status || "—"}</td>
                    <td>${u.createdOn ? new Date(u.createdOn).toLocaleDateString() : "—"}</td>
                </tr>
            `);
    });
    $("#tbl tbody").html(rows.length ? rows.join("") : `<tr><td colspan="7" class="text-center py-4">No data found.</td></tr>`);
    $("#resultInfo").text(`${state.total} records`);
}

function pager() {
    const pages = Math.max(1, Math.ceil(state.total / state.pageSize));
    const $p = $("#pager");

    const btn = (p, lbl, dis = false, act = false) =>
        `<li class="page-item ${dis ? 'disabled' : ''} ${act ? 'active' : ''}">
               <a class="page-link" href="#" data-p="${p}">${lbl}</a>
             </li>`;

    let html = "";
    html += btn(Math.max(1, state.page - 1), "&laquo;", state.page === 1);
    for (let i = 1; i <= pages && i <= 10; i++) html += btn(i, i, false, i === state.page);
    html += btn(Math.min(pages, state.page + 1), "&raquo;", state.page === pages);

    $p.html(html);
    $p.find("a").on("click", function (e) {
        e.preventDefault();
        const p = +$(this).data("p");
        if (!isNaN(p)) { state.page = p; load(); }
    });
}

function download(kind) {
    const q = buildQuery(qs());
    window.location.href = `${API}/export/${kind}?${q}`;
}

$(function () {
    $("#btnSearch").on("click", function () { state.page = 1; load(); });
    $("#btnReset").on("click", function () {
        $("#fName,#fEmail,#fCity,#fFrom,#fTo").val("");
        $("#fStatus").val("");
        state.page = 1; load();
    });

    $("#btnExcel").on("click", () => download("excel"));
    $("#btnPdf").on("click", () => download("pdf"));

    load();
});*/