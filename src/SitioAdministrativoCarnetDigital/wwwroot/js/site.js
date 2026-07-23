// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById('sidebarToggle')?.addEventListener('click', function ()
{

    // En desktop 
    if (window.innerWidth > 768)
    {
        document.body.classList.toggle('sidebar-collapsed');
    }
    else
    {
        // En móvil usa la clase .open en la sidebar directamente
        document.querySelector('.sidebar').classList.toggle('open');
    }
});

// Navegación activa
(function ()
{
    var path = window.location.pathname.toLowerCase();
    var isHome = path === '/' || path === '/index';

    document.querySelectorAll('.sidebar-nav .nav-link').forEach(function (link)
    {
        var href = (link.getAttribute('href') || '').toLowerCase();

        if ((isHome && (href === '/' || href === '/index')) ||
            (!isHome && href && href !== '/' && href !== '/index' && path.indexOf(href) === 0))
        {
            link.classList.add('active');
        }
        else
        {
            link.classList.remove('active');
        }
    });
})();


document.addEventListener("DOMContentLoaded", function () {

const avatar = document.getElementById("userAvatar");
const dropdown = document.getElementById("userDropdown");

avatar.addEventListener("click", function (e) {

  e.stopPropagation();

  dropdown.classList.toggle("show");

});

document.addEventListener("click", function () {

    dropdown.classList.remove("show");

});

});