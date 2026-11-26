document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-share]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            if (navigator.clipboard) {
                navigator.clipboard.writeText(window.location.href);
            }
            const span = btn.querySelector('span');
            if (span) {
                span.textContent = 'Скопировано';
            }
        });
    });
});
