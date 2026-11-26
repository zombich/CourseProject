document.querySelectorAll('.picker-box').forEach(function (box) {
    const targetId = box.getAttribute('data-picker');
    const isMulti = box.getAttribute('data-multi') === 'true';
    const fieldName = box.getAttribute('data-field');
    const targetEl = document.getElementById(targetId);
    const searchInput = box.querySelector('.picker-search');
    const items = box.querySelectorAll('.picker-item');

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const q = searchInput.value.trim().toLowerCase();
            items.forEach(function (item) {
                const text = item.textContent.trim().toLowerCase();
                item.style.display = text.includes(q) ? '' : 'none';
            });
        });
    }

    items.forEach(function (item) {
        item.addEventListener('click', function () {
            if (isMulti && targetEl && fieldName) {
                item.classList.toggle('is-selected');
                targetEl.innerHTML = '';
                items.forEach(function (i) {
                    if (i.classList.contains('is-selected')) {
                        const input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = fieldName;
                        input.value = i.getAttribute('data-value');
                        targetEl.appendChild(input);
                    }
                });
            } else {
                items.forEach(i => i.classList.remove('is-selected'));
                item.classList.add('is-selected');
                if (targetEl) {
                    targetEl.value = item.getAttribute('data-value');
                }
            }
        });
    });
});

document.querySelectorAll('.manage-filter-input').forEach(function (input) {
    const list = document.getElementById(input.getAttribute('data-target'));
    if (!list) return;
    const rows = list.querySelectorAll('.manage-filter-item');
    input.addEventListener('input', function () {
        const q = input.value.trim().toLowerCase();
        rows.forEach(function (row) {
            const text = row.textContent.trim().toLowerCase();
            row.style.display = text.includes(q) ? 'flex' : 'none';
        });
    });
});
