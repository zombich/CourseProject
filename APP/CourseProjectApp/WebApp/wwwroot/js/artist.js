(function () {
            const editBtn = document.getElementById('edit-artist-bio-btn');
            const cancelBtn = document.getElementById('cancel-artist-bio-btn');
            const display = document.getElementById('artist-bio-display');
            const form = document.getElementById('artist-bio-form');
            if (!editBtn || !display || !form) return;

            editBtn.addEventListener('click', function () {
                display.style.display = 'none';
                form.style.display = 'block';
            });

            if (cancelBtn) {
                cancelBtn.addEventListener('click', function () {
                    form.style.display = 'none';
                    display.style.display = 'block';
                });
            }
        })();
