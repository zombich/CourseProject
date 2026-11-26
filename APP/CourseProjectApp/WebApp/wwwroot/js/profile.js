(function () {
            const editBtn = document.getElementById('edit-bio-btn');
            const cancelBtn = document.getElementById('cancel-bio-btn');
            const display = document.getElementById('bio-display');
            const form = document.getElementById('bio-edit-form');
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
