document.addEventListener('DOMContentLoaded', function () {
    var mainNav = document.getElementById('mainNav');
    if (mainNav) mainNav.style.display = 'none';

    // Post type toggle
    document.querySelectorAll('.cp-type-card').forEach(function(card) {
        card.addEventListener('click', function () {
            document.querySelectorAll('.cp-type-card').forEach(function(c) { c.classList.remove('cp-type-active'); });
            this.classList.add('cp-type-active');
        });
    });

    // Occupant radio
    document.querySelectorAll('input[name="MaxOccupants"]').forEach(function(r) {
        r.addEventListener('change', function () {
            document.querySelectorAll('.cp-occupant-opt').forEach(function(o) { o.classList.remove('active'); });
            this.closest('.cp-occupant-opt').classList.add('active');
        });
    });

    // Package toggle
    document.querySelectorAll('input[name="packageType"]').forEach(function(r) {
        r.addEventListener('change', function () {
            document.querySelectorAll('.cp-package-opt').forEach(function(o) { o.classList.remove('active'); });
            this.closest('.cp-package-opt').classList.add('active');
            var el = document.getElementById('isVipHidden');
            if (el) el.value = (this.value === 'vip') ? 'true' : 'false';
        });
    });

    // Amenity chips
    document.querySelectorAll('input[name="selectedAmenities"]').forEach(function(cb) {
        cb.addEventListener('change', function () {
            this.closest('.cp-habit-chip').classList.toggle('active', this.checked);
        });
    });

    // Rules chips + sync hidden
    function updateRules() {
        var selected = Array.from(document.querySelectorAll('input[name="rulesList"]:checked')).map(function(c) { return c.value; });
        var el = document.getElementById('rulesInput');
        if (el) el.value = selected.join(',');
    }
    document.querySelectorAll('input[name="rulesList"]').forEach(function(cb) {
        cb.addEventListener('change', function () {
            this.closest('.cp-habit-chip').classList.toggle('active', this.checked);
            updateRules();
        });
    });
    updateRules();

    // Live preview
    var titleInput = document.querySelector('input[name="Title"]');
    var priceInput = document.querySelector('input[name="Price"]');
    var addrInput  = document.querySelector('input[name="Address"]');

    function updatePreview() {
        var t = (titleInput && titleInput.value) ? titleInput.value : 'Tiêu đề bài viết...';
        var p = (priceInput && priceInput.value) ? Number(priceInput.value).toLocaleString('vi-VN') + ' đ/tháng' : '— đ/tháng';
        var a = (addrInput  && addrInput.value)  ? addrInput.value  : 'Địa chỉ...';
        var pt = document.getElementById('previewTitle');
        var pp = document.getElementById('previewPrice');
        var pa = document.getElementById('previewAddr');
        if (pt) pt.textContent = t.length > 40 ? t.slice(0, 40) + '...' : t;
        if (pp) pp.textContent = p;
        if (pa) pa.innerHTML = '<i class="fas fa-map-marker-alt"></i> ' + (a.length > 35 ? a.slice(0, 35) + '...' : a);
    }

    [titleInput, priceInput, addrInput].forEach(function(el) {
        if (el) el.addEventListener('input', updatePreview);
    });

    // Image upload
    var fileInput  = document.getElementById('imageUpload');
    var previewRow = document.getElementById('previewRow');
    var uploadZone = document.getElementById('uploadZone');

    if (uploadZone) {
        uploadZone.addEventListener('dragover', function(e) { e.preventDefault(); uploadZone.classList.add('cp-upload-drag'); });
        uploadZone.addEventListener('dragleave', function() { uploadZone.classList.remove('cp-upload-drag'); });
        uploadZone.addEventListener('drop', function(e) {
            e.preventDefault();
            uploadZone.classList.remove('cp-upload-drag');
            handleFiles(e.dataTransfer.files);
        });
    }

    if (fileInput) {
        fileInput.addEventListener('change', function () { handleFiles(this.files); });
    }

    function handleFiles(files) {
        Array.from(files).slice(0, 5).forEach(function(file) {
            if (!file.type.startsWith('image/')) return;
            var reader = new FileReader();
            reader.onload = function(ev) {
                var thumb = document.createElement('div');
                thumb.className = 'cp-thumb';
                var img = document.createElement('img');
                img.src = ev.target.result;
                img.alt = 'preview';
                var btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'cp-thumb-remove';
                btn.innerHTML = '<i class="fas fa-times"></i>';
                btn.addEventListener('click', function() { thumb.remove(); });
                thumb.appendChild(img);
                thumb.appendChild(btn);
                if (previewRow) previewRow.appendChild(thumb);
            };
            reader.readAsDataURL(file);
        });
    }
});

function saveDraft() {
    var btn = document.querySelector('.cp-btn-draft');
    if (!btn) return;
    btn.innerHTML = '<i class="fas fa-check"></i> Đã lưu nháp';
    btn.style.color = '#10b981';
    setTimeout(function() { btn.innerHTML = '<i class="fas fa-save"></i> Lưu nháp'; btn.style.color = ''; }, 2000);
}
