// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById('sidebarToggle')?.addEventListener('click', function () {

    // En desktop usa la clase en body
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