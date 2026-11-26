async function uploadAnnotationImageFile(file) {
            const form = document.getElementById('annotation-form');
            const tokenInput = form ? form.querySelector('input[name="__RequestVerificationToken"]') : null;
            const fd = new FormData();
            fd.append('file', file);
            if (tokenInput) {
                fd.append('__RequestVerificationToken', tokenInput.value);
            }
            const resp = await fetch('/Track?handler=UploadAnnotationImage', {
                method: 'POST',
                body: fd
            });
            if (!resp.ok) throw new Error('Ошибка загрузки изображения');
            const data = await resp.json();
            return data.url;
        }

        let easyMdeInstance = null;
        if (typeof EasyMDE !== 'undefined' && document.getElementById('annotation-content')) {
            const hiddenFileInput = document.getElementById('easymde-image-input');
            easyMdeInstance = new EasyMDE({
                element: document.getElementById('annotation-content'),
                spellChecker: false,
                status: false,
                uploadImage: true,
                imageUploadFunction: function (file, onSuccess, onError) {
                    uploadAnnotationImageFile(file)
                        .then(url => onSuccess(url))
                        .catch(err => onError(err.message));
                },
                placeholder: 'Опишите смысл выбранных строк в Markdown...',
                toolbar: [
                    'bold', 'italic', 'heading', '|',
                    'quote', 'unordered-list', 'ordered-list', '|',
                    'link',
                    {
                        name: 'image',
                        action: function () {
                            if (hiddenFileInput) hiddenFileInput.click();
                        },
                        className: 'fa fa-image',
                        title: 'Загрузить картинку'
                    },
                    '|', 'preview'
                ]
            });

            if (hiddenFileInput) {
                hiddenFileInput.addEventListener('change', async function () {
                    if (!this.files || this.files.length === 0) return;
                    const file = this.files[0];
                    try {
                        const url = await uploadAnnotationImageFile(file);
                        const cm = easyMdeInstance.codemirror;
                        cm.replaceSelection(`![Изображение](${url})`);
                        cm.focus();
                    } catch (e) {
                        alert('Не удалось загрузить картинку.');
                    } finally {
                        this.value = '';
                    }
                });
            }
        }

        (function () {
            const lyricsBlock = document.getElementById('lyrics-text-block');
            const lineStartInput = document.getElementById('line-start');
            const lineEndInput = document.getElementById('line-end');
            const charStartInput = document.getElementById('char-start');
            const charEndInput = document.getElementById('char-end');
            const hint = document.getElementById('selection-hint');
            const saveBtn = document.getElementById('save-annotation-btn');
            const openBtn = document.getElementById('open-add-annotation-btn');
            const closeBtn = document.getElementById('close-add-annotation-btn');
            const createBox = document.getElementById('create-annotation-box');
            const viewBox = document.getElementById('annotation-view-box');

            function showCreateBox() {
                if (createBox) createBox.style.display = 'block';
                if (viewBox) viewBox.style.display = 'none';
                if (openBtn) openBtn.style.display = 'none';
                if (easyMdeInstance && easyMdeInstance.codemirror) { setTimeout(() => easyMdeInstance.codemirror.refresh(), 20); }
            }

            function hideCreateBox() {
                if (createBox) createBox.style.display = 'none';
                if (viewBox) viewBox.style.display = 'block';
                if (openBtn) openBtn.style.display = 'block';
            }

            if (openBtn) openBtn.addEventListener('click', showCreateBox);
            if (closeBtn) closeBtn.addEventListener('click', hideCreateBox);

            const editLyricsBtn = document.getElementById('edit-lyrics-btn');
            const cancelLyricsBtn = document.getElementById('cancel-lyrics-btn');
            const lyricsEditForm = document.getElementById('lyrics-edit-form');
            if (editLyricsBtn && lyricsBlock && lyricsEditForm) {
                editLyricsBtn.addEventListener('click', function () {
                    lyricsBlock.style.display = 'none';
                    lyricsEditForm.style.display = 'block';
                });
                if (cancelLyricsBtn) {
                    cancelLyricsBtn.addEventListener('click', function () {
                        lyricsEditForm.style.display = 'none';
                        lyricsBlock.style.display = 'block';
                    });
                }
            }

            const editAboutBtn = document.getElementById('edit-about-btn');
            const cancelAboutBtn = document.getElementById('cancel-about-btn');
            const aboutDisplay = document.getElementById('about-display');
            const aboutEditForm = document.getElementById('about-edit-form');
            if (editAboutBtn && aboutDisplay && aboutEditForm) {
                editAboutBtn.addEventListener('click', function () {
                    aboutDisplay.style.display = 'none';
                    aboutEditForm.style.display = 'block';
                });
                if (cancelAboutBtn) {
                    cancelAboutBtn.addEventListener('click', function () {
                        aboutEditForm.style.display = 'none';
                        aboutDisplay.style.display = 'block';
                    });
                }
            }

            if (!lyricsBlock || !lineStartInput || !lineEndInput) return;

            function findLineSpan(node) {
                while (node && node !== lyricsBlock) {
                    if (node.nodeType === 1 && node.classList.contains('lyric-line')) {
                        return node;
                    }
                    node = node.parentNode;
                }
                return null;
            }

            lyricsBlock.addEventListener('mouseup', function () {
                const sel = window.getSelection();
                if (!sel || sel.isCollapsed) return;

                const selectedText = sel.toString().trim();
                if (!selectedText) return;

                const startSpan = findLineSpan(sel.anchorNode);
                const endSpan = findLineSpan(sel.focusNode);
                if (!startSpan || !endSpan) return;

                const l1 = parseInt(startSpan.getAttribute('data-line'), 10);
                const l2 = parseInt(endSpan.getAttribute('data-line'), 10);
                const lineMin = Math.min(l1, l2);
                const lineMax = Math.max(l1, l2);

                lineStartInput.value = lineMin;
                lineEndInput.value = lineMax;

                if (lineMin === lineMax && charStartInput && charEndInput) {
                    const fullText = startSpan.textContent || '';
                    const idx = fullText.indexOf(selectedText);
                    if (idx >= 0) {
                        charStartInput.value = idx;
                        charEndInput.value = idx + selectedText.length;
                    } else {
                        charStartInput.value = 0;
                        charEndInput.value = 0;
                    }
                } else if (charStartInput && charEndInput) {
                    charStartInput.value = 0;
                    charEndInput.value = 0;
                }

                if (hint) {
                    const preview = selectedText.length > 60 ? selectedText.slice(0, 60) + '…' : selectedText;
                    hint.innerHTML = `Выбран фрагмент: <strong>«${preview.replace(/</g, '&lt;')}»</strong>`;
                }
                if (saveBtn) {
                    saveBtn.disabled = false;
                }
                if (openBtn) {
                    openBtn.textContent = '+ Аннотировать выделенный фрагмент';
                }
            });
        })();
