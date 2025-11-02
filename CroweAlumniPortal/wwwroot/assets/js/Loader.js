/*Loader*/

$(window).on('load', function () {
    $('#loader').delay(100).fadeOut('slow');
    $('#loader-wrapper').delay(500).fadeOut('slow');
});

var loader = document.getElementById("loader");
window.addEventListener("load", function () {
    loader.style.display = "none";
})