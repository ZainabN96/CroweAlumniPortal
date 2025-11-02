//document.addEventListener("click", function (e) {
//    const btn = e.target.closest(".crw-share-btn");
//    if (!btn) return;
//    const id = btn.getAttribute("data-id");
//    const url = `${location.origin}/Dashboard/Eventsdetail?id=${id}`;
//    navigator.clipboard?.writeText(url).then(() => {
//        btn.classList.add("active");
//        setTimeout(() => btn.classList.remove("active"), 800);
//    });
//});
document.addEventListener("click", function (e) {
    const btn = e.target.closest(".crw-share-btn");
    if (!btn) return;
    const id = btn.getAttribute("data-id");
    const url = `${location.origin}/Dashboard/Eventsdetail?id=${id}`;
    navigator.clipboard?.writeText(url).then(() => {
        btn.classList.add("active");
        setTimeout(() => btn.classList.remove("active"), 800);
    });
});