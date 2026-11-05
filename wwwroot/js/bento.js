(function (window) {
  'use strict';

  var AI_IDS = [18, 2, 1, 4, 16];
  var WEB_IDS = [15, 3, 5, 6, 7, 8, 12, 17];
  var IND_IDS = [9, 10, 11, 13, 14];
  var ALLOWED_TAGS = {
    P: true,
    UL: true,
    OL: true,
    LI: true,
    H5: true,
    STRONG: true,
    EM: true,
    BR: true,
    A: true
  };

  function getLang() {
    var lang = (document.documentElement.lang || 'tr').toLowerCase();
    return lang === 'en' ? 'en' : 'tr';
  }

  function fetchJson(name) {
    var lang = getLang();
    var base = lang === 'en' ? '/data-en/' : '/data/';
    var url = base + name + '.json?v=' + Date.now();

    return fetch(url)
      .then(function (res) {
        if (!res.ok) {
          throw new Error('HTTP ' + res.status);
        }
        return res.json();
      })
      .catch(function (err) {
        console.warn('[Bento] fetchJson failed for', name, err);
        return null;
      });
  }

  function escapeHtml(str) {
    if (str == null) return '';
    return String(str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function copyToClipboard(text, buttonEl, doneLabel, selectEl) {
    var label = doneLabel || 'Kopyalandı ✓';
    var original = buttonEl ? buttonEl.textContent : '';

    function showDone() {
      if (!buttonEl) return;
      buttonEl.textContent = label;
      window.setTimeout(function () {
        buttonEl.textContent = original;
      }, 2000);
    }

    function selectAddress() {
      if (!selectEl) return;
      try {
        var range = document.createRange();
        range.selectNodeContents(selectEl);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
      } catch (err) {
        console.warn('[Bento] select address failed', err);
      }
    }

    if (navigator.clipboard && navigator.clipboard.writeText) {
      return navigator.clipboard.writeText(text).then(showDone).catch(function (err) {
        console.warn('[Bento] clipboard write failed', err);
        selectAddress();
      });
    }

    selectAddress();
    try {
      var ta = document.createElement('textarea');
      ta.value = text;
      ta.setAttribute('readonly', '');
      ta.style.position = 'absolute';
      ta.style.left = '-9999px';
      document.body.appendChild(ta);
      ta.select();
      document.execCommand('copy');
      document.body.removeChild(ta);
      showDone();
    } catch (err) {
      console.warn('[Bento] clipboard fallback failed', err);
    }
  }

  function projectCategory(project) {
    var id = project && project.id != null ? Number(project.id) : NaN;
    if (AI_IDS.indexOf(id) !== -1) return 'ai';
    if (WEB_IDS.indexOf(id) !== -1) return 'web';
    if (IND_IDS.indexOf(id) !== -1) return 'ind';

    var title = ((project && project.title) || '').toLocaleLowerCase('tr');
    var tags = ((project && project.tags) || []).join(' ').toLocaleLowerCase('tr');
    var blob = title + ' ' + tags;

    if (/(\bai\b|yapay zeka|yapay zekâ|machine learning|\bml\b|yolo|bert|\bllm\b|claude|tensorflow)/i.test(blob)) {
      return 'ai';
    }

    if (
      /(fabrika|bakım|süreç|maintenance|factory|process)/i.test(title) ||
      /(mssql|asp\.net mvc)/i.test(blob)
    ) {
      return 'ind';
    }

    return 'web';
  }

  function monogram(title) {
    var base = String(title || '').split(' - ')[0].trim();
    if (!base) return '';
    var words = base.split(/\s+/).filter(Boolean);
    var result;
    if (words.length === 1) {
      result = words[0].slice(0, 2);
    } else {
      result = (words[0].charAt(0) || '') + (words[1].charAt(0) || '');
    }
    return result.toLocaleUpperCase('tr');
  }

  function yearOf(dateRange) {
    var matches = String(dateRange || '').match(/\d{4}/g);
    if (!matches || !matches.length) return '';
    return matches[matches.length - 1];
  }

  function shortTitle(title) {
    return String(title || '').split(' - ')[0].trim();
  }

  function titleSubtitle(title) {
    var raw = String(title || '');
    var idx = raw.indexOf(' - ');
    if (idx === -1) return '';
    return raw.slice(idx + 3).trim();
  }

  function categoryLabel(cat, lang) {
    if (cat === 'ai') return 'AI';
    if (cat === 'web') return 'Web';
    return lang === 'en' ? 'Industry' : 'Endüstri';
  }

  function projectsCategoryLabel(cat, lang) {
    if (cat === 'ai') return lang === 'en' ? 'AI' : 'Yapay zekâ';
    if (cat === 'web') return 'Web';
    return lang === 'en' ? 'Industrial' : 'Endüstriyel';
  }

  function hasRealImage(project) {
    var url = project && project.imageUrl ? String(project.imageUrl) : '';
    if (!url) return false;
    return url.indexOf('placehold.co') === -1;
  }

  function sanitizeHtml(html) {
    var parser = new DOMParser();
    var doc = parser.parseFromString('<div id="bento-sanitize-root">' + String(html || '') + '</div>', 'text/html');
    var root = doc.getElementById('bento-sanitize-root');
    if (!root) return '';

    function cleanAttributes(el) {
      var attrs = Array.prototype.slice.call(el.attributes || []);
      attrs.forEach(function (attr) {
        var name = attr.name.toLowerCase();
        if (name.indexOf('on') === 0 || name === 'style') {
          el.removeAttribute(attr.name);
          return;
        }
        if (el.tagName === 'A') {
          if (name === 'href') {
            var href = String(attr.value || '').trim();
            if (!/^https?:\/\//i.test(href)) {
              el.removeAttribute('href');
            } else {
              el.setAttribute('href', href);
              el.setAttribute('target', '_blank');
              el.setAttribute('rel', 'noopener');
            }
          } else if (name !== 'target' && name !== 'rel') {
            el.removeAttribute(attr.name);
          }
        } else {
          el.removeAttribute(attr.name);
        }
      });
    }

    function walk(parent) {
      var nodes = Array.prototype.slice.call(parent.childNodes);
      nodes.forEach(function (node) {
        if (node.nodeType === Node.TEXT_NODE) return;
        if (node.nodeType !== Node.ELEMENT_NODE) {
          parent.removeChild(node);
          return;
        }

        var tag = node.tagName;
        if (!ALLOWED_TAGS[tag]) {
          var text = doc.createTextNode(node.textContent || '');
          parent.replaceChild(text, node);
          return;
        }

        cleanAttributes(node);
        walk(node);
      });
    }

    walk(root);
    return root.innerHTML;
  }

  function shortRange(dateRange) {
    var raw = String(dateRange || '').trim();
    if (!raw) return '';

    var parts = raw.split(/\s*[-–—]\s*/);
    if (parts.length < 2) return raw;

    var start = parts[0].trim();
    var end = parts.slice(1).join(' - ').trim();

    function parsePart(part) {
      var mdy = part.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})$/);
      if (mdy) {
        return { month: mdy[2].padStart(2, '0'), year: mdy[3], kind: 'full' };
      }
      var my = part.match(/^(\d{1,2})\.(\d{4})$/);
      if (my) {
        return { month: my[1].padStart(2, '0'), year: my[2], kind: 'my' };
      }
      var y = part.match(/^(\d{4})$/);
      if (y) {
        return { year: y[1], kind: 'y' };
      }
      return { raw: part, kind: 'text' };
    }

    var s = parsePart(start);
    var e = parsePart(end);

    if (e.kind === 'text') {
      if (s.kind === 'full' || s.kind === 'my') {
        return s.month + '.' + s.year + ' — ' + e.raw;
      }
      return raw;
    }

    if ((s.kind === 'full' || s.kind === 'my') && (e.kind === 'full' || e.kind === 'my')) {
      return s.month + ' — ' + e.month + '.' + e.year;
    }

    if (s.kind === 'y' && e.kind === 'y') {
      return s.year + ' — ' + e.year;
    }

    return raw;
  }

  /* Shared <dialog> open/close */
  var activeDialog = null;

  function dialogHandleClose(el) {
    if (!el) return;
    document.body.classList.remove('dialog-open');
    if (activeDialog === el) activeDialog = null;
    var trigger = el.__bentoTrigger || null;
    el.__bentoTrigger = null;
    if (typeof el.__bentoOnClose === 'function') {
      el.__bentoOnClose();
    }
    if (trigger && typeof trigger.focus === 'function') {
      trigger.focus();
    }
  }

  function dialogOpen(el, triggerEl) {
    if (!el) return;
    el.__bentoTrigger = triggerEl || null;
    activeDialog = el;
    document.body.classList.add('dialog-open');
    if (typeof el.showModal === 'function') {
      if (!el.open) el.showModal();
    } else {
      el.setAttribute('open', '');
    }
  }

  function dialogClose(el) {
    el = el || activeDialog;
    if (!el) return;
    if (typeof el.close === 'function' && el.open) {
      el.close();
    } else {
      el.removeAttribute('open');
      dialogHandleClose(el);
    }
  }

  function dialogBind(el, options) {
    if (!el || el.__bentoBound) return;
    el.__bentoBound = true;
    options = options || {};
    el.__bentoOnClose = options.onClose || null;

    el.addEventListener('close', function () {
      dialogHandleClose(el);
    });

    el.addEventListener('click', function (e) {
      if (e.target === el) dialogClose(el);
    });

    el.querySelectorAll('[data-dialog-close]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        dialogClose(el);
      });
    });
  }

  function confirmDialog(message, options) {
    options = options || {};
    var lang = getLang();
    var confirmLabel = options.confirmLabel || (lang === 'en' ? 'Confirm' : 'Onayla');
    var cancelLabel = options.cancelLabel || (lang === 'en' ? 'Cancel' : 'Vazgeç');
    var danger = !!options.danger;

    return new Promise(function (resolve) {
      var dialog = document.getElementById('bento-confirm-dialog');
      if (!dialog) {
        dialog = document.createElement('dialog');
        dialog.id = 'bento-confirm-dialog';
        dialog.className = 'bento-confirm-dialog';
        dialog.innerHTML =
          '<div class="bento-confirm-dialog__inner">' +
          '<p class="bento-confirm-dialog__message"></p>' +
          '<div class="bento-confirm-dialog__actions">' +
          '<button type="button" class="btn btn-secondary-custom" data-confirm-cancel></button>' +
          '<button type="button" data-confirm-ok></button>' +
          '</div>' +
          '</div>';
        document.body.appendChild(dialog);
        dialogBind(dialog);
      }

      var msgEl = dialog.querySelector('.bento-confirm-dialog__message');
      var okBtn = dialog.querySelector('[data-confirm-ok]');
      var cancelBtn = dialog.querySelector('[data-confirm-cancel]');
      if (msgEl) msgEl.textContent = message || '';
      if (cancelBtn) cancelBtn.textContent = cancelLabel;
      if (okBtn) {
        okBtn.textContent = confirmLabel;
        okBtn.className = danger ? 'btn--danger' : 'btn-primary-custom';
      }

      var settled = false;
      function finish(value) {
        if (settled) return;
        settled = true;
        dialog.removeEventListener('close', onClose);
        if (okBtn) okBtn.onclick = null;
        if (cancelBtn) cancelBtn.onclick = null;
        if (dialog.open) dialogClose(dialog);
        resolve(!!value);
      }

      function onClose() {
        finish(false);
      }

      if (okBtn) {
        okBtn.onclick = function () {
          finish(true);
        };
      }
      if (cancelBtn) {
        cancelBtn.onclick = function () {
          finish(false);
        };
      }
      dialog.addEventListener('close', onClose);

      dialogOpen(dialog, document.activeElement);
      window.setTimeout(function () {
        try {
          if (cancelBtn) cancelBtn.focus();
        } catch (err) {
          /* ignore */
        }
      }, 0);
    });
  }

  function firstCompanyWord(company) {
    var word = String(company || '').trim().split(/\s+/)[0] || '';
    return word.replace(/[.,;:]+$/, '');
  }

  function getWorkExperience(groups) {
    if (!Array.isArray(groups)) return [];
    for (var i = 0; i < groups.length; i++) {
      var g = groups[i];
      if (g && g.type === 'İş Deneyimi' && Array.isArray(g.experience)) {
        return g.experience;
      }
    }
    return [];
  }

  function showError(el, message) {
    if (!el) return;
    el.innerHTML = '<p class="load-error">' + escapeHtml(message) + '</p>';
    el.classList.add('is-error');
  }

  function renderNow(el, jobs, lang) {
    if (!el) return;
    if (!jobs || !jobs.length) {
      showError(el, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      return;
    }

    var current = jobs[0];
    var prev = jobs.slice(1, 4).map(function (job) {
      return firstCompanyWord(job && job.company);
    }).filter(Boolean);
    var prevPrefix = lang === 'en' ? 'previously: ' : 'önceden: ';
    var prevHtml = prev.length
      ? '<p class="now-prev">' + escapeHtml(prevPrefix + prev.join(' · ')) + '</p>'
      : '';

    el.innerHTML =
      '<p class="eyebrow">' + escapeHtml(lang === 'en' ? 'Now' : 'Şu an') + '</p>' +
      '<h3 class="now-company">' + escapeHtml(current.company || '') + '</h3>' +
      '<p class="now-title">' + escapeHtml(current.title || '') + '</p>' +
      prevHtml;
  }

  function renderFeatured(el, project, lang) {
    if (!el) return;
    if (!project) {
      showError(el, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      return;
    }

    var tags = (project.tags || []).slice(0, 4);
    var chips = tags.map(function (t) {
      return '<span class="chip chip--on-accent">' + escapeHtml(t) + '</span>';
    }).join('');

    el.setAttribute('href', '/' + lang + '/Home/Projects#project-' + encodeURIComponent(project.id));
    el.innerHTML =
      '<p class="eyebrow">' + escapeHtml(lang === 'en' ? 'Featured project' : 'Öne çıkan proje') + '</p>' +
      '<h3>' + escapeHtml(shortTitle(project.title)) + '</h3>' +
      '<p>' + escapeHtml(project.description || '') + '</p>' +
      (chips ? '<div class="chip-row">' + chips + '</div>' : '');
  }

  function renderSelected(el, projects, lang) {
    if (!el) return;
    if (!projects || projects.length < 2) {
      showError(el, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      return;
    }

    var selected = projects.slice(1, 6);
    var html = '';

    selected.forEach(function (project, index) {
      var cat = projectCategory(project);
      var label = categoryLabel(cat, lang);
      var year = yearOf(project.dateRange);
      var mono = monogram(project.title);
      var href = '/' + lang + '/Home/Projects#project-' + encodeURIComponent(project.id);
      var title = shortTitle(project.title);
      var desc = project.description || '';

      if (index === 0) {
        html +=
          '<a class="tile home-work home-work--large span-2 row-2" href="' + href + '">' +
            '<div class="home-work__cover home-work__cover--' + cat + '">' +
              '<span class="home-work__monogram" aria-hidden="true">' + escapeHtml(mono) + '</span>' +
            '</div>' +
            '<p class="home-work__meta">' + escapeHtml(label.toLocaleUpperCase('tr') + (year ? ' · ' + year : '')) + '</p>' +
            '<h3>' + escapeHtml(title) + '</h3>' +
            '<p class="home-work__desc">' + escapeHtml(desc) + '</p>' +
          '</a>';
      } else {
        var tags = (project.tags || []).slice(0, 3).join(' · ');
        html +=
          '<a class="tile home-work home-work--small" href="' + href + '">' +
            '<div class="home-work__icon home-work__icon--' + cat + '" aria-hidden="true">' +
              '<span>' + escapeHtml(mono) + '</span>' +
            '</div>' +
            '<h3 class="home-work__title">' + escapeHtml(title) + '</h3>' +
            '<p class="home-work__desc home-work__desc--clamp">' + escapeHtml(desc) + '</p>' +
            '<p class="home-work__tags">' + escapeHtml(tags) + '</p>' +
          '</a>';
      }
    });

    el.innerHTML = html;
  }

  function renderJourney(el, jobs, lang) {
    if (!el) return;
    if (!jobs || !jobs.length) {
      showError(el, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      return;
    }

    var items = jobs.slice(0, 4);
    var html = '';
    items.forEach(function (job, index) {
      var currentClass = index === 0 ? ' tile--current' : '';
      html +=
        '<div class="tile tile--static' + currentClass + '">' +
          '<p class="journey-date">' + escapeHtml(shortRange(job.dateRange)) + '</p>' +
          '<h3 class="journey-company">' + escapeHtml(job.company || '') + '</h3>' +
          '<p class="journey-title">' + escapeHtml(job.title || '') + '</p>' +
        '</div>';
    });
    el.innerHTML = html;
  }

  function updateProjectsLink(total, lang) {
    var link = document.getElementById('home-all-projects');
    if (!link) return;
    var label = lang === 'en'
      ? total + ' projects →'
      : total + ' projenin tümü →';
    link.textContent = label;
  }

  function formatContributionDate(iso, lang) {
    var parts = String(iso || '').split('-');
    if (parts.length !== 3) return iso || '';
    var y = Number(parts[0]);
    var m = Number(parts[1]);
    var d = Number(parts[2]);
    if (!y || !m || !d) return iso || '';
    var date = new Date(y, m - 1, d);
    if (Number.isNaN(date.getTime())) return iso || '';
    return date.toLocaleDateString(lang === 'en' ? 'en-US' : 'tr-TR', {
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    });
  }

  function renderGitHubHeatmapUnavailable(root, lang) {
    if (!root) return;
    var meta = root.querySelector('[data-github-meta]');
    var summary = root.querySelector('[data-github-summary]');
    if (meta) {
      meta.textContent = lang === 'en' ? 'data unavailable' : 'veri alınamadı';
    }
    if (summary) {
      summary.textContent = '';
    }
  }

  function renderGitHubHeatmap(root, data, lang) {
    if (!root || !data || !Array.isArray(data.weeks)) {
      renderGitHubHeatmapUnavailable(root, lang);
      return;
    }

    var heatmap = root.querySelector('[data-github-heatmap]');
    var meta = root.querySelector('[data-github-meta]');
    var summary = root.querySelector('[data-github-summary]');
    var weeks = data.weeks;
    var total = typeof data.total === 'number' ? data.total : 0;
    var contribWord = lang === 'en' ? 'contributions' : 'katkı';
    var html = '';

    for (var w = 0; w < weeks.length; w++) {
      var days = Array.isArray(weeks[w]) ? weeks[w] : [];
      for (var d = 0; d < 7; d++) {
        var day = days[d] || {};
        if (!day.d) {
          html += '<span class="heatmap__cell heatmap__cell--empty"></span>';
          continue;
        }
        var level = Math.max(0, Math.min(4, Number(day.l) || 0));
        var count = Number(day.c) || 0;
        var dateLabel = formatContributionDate(day.d, lang);
        var title = dateLabel
          ? dateLabel + ': ' + count + ' ' + contribWord
          : count + ' ' + contribWord;
        html +=
          '<span class="heatmap__cell" data-level="' +
          level +
          '" title="' +
          escapeHtml(title) +
          '"></span>';
      }
    }

    if (heatmap) {
      heatmap.innerHTML = html || heatmap.innerHTML;
    }

    if (meta) {
      meta.textContent =
        lang === 'en'
          ? 'last 30 weeks · ' + total + ' contributions'
          : 'son 30 hafta · ' + total + ' katkı';
    }

    if (summary) {
      summary.textContent =
        lang === 'en'
          ? total + ' contributions in the last 30 weeks'
          : 'Son 30 haftada ' + total + ' katkı';
    }
  }

  function initGitHubHeatmap() {
    var root = document.getElementById('home-github');
    if (!root || root.getAttribute('data-github-bound') === '1') return;
    root.setAttribute('data-github-bound', '1');

    var lang = getLang();
    var loaded = false;

    function load() {
      if (loaded) return;
      loaded = true;
      fetch('/api/github/contributions', {
        headers: { Accept: 'application/json' }
      })
        .then(function (res) {
          if (res.status === 204) {
            renderGitHubHeatmapUnavailable(root, lang);
            return null;
          }
          if (!res.ok) {
            renderGitHubHeatmapUnavailable(root, lang);
            return null;
          }
          return res.json();
        })
        .then(function (data) {
          if (!data) return;
          renderGitHubHeatmap(root, data, lang);
        })
        .catch(function () {
          renderGitHubHeatmapUnavailable(root, lang);
        });
    }

    if (!('IntersectionObserver' in window)) {
      load();
      return;
    }

    var observer = new IntersectionObserver(
      function (entries) {
        for (var i = 0; i < entries.length; i++) {
          if (entries[i].isIntersecting) {
            observer.disconnect();
            load();
            break;
          }
        }
      },
      { rootMargin: '200px' }
    );

    observer.observe(root);
  }

  function homeInit() {
    var lang = getLang();
    var nowEl = document.getElementById('home-now');
    var featuredEl = document.getElementById('home-featured');
    var selectedEl = document.getElementById('home-selected');
    var journeyEl = document.getElementById('home-journey');

    initGitHubHeatmap();

    return Promise.all([fetchJson('projects'), fetchJson('experience')]).then(function (results) {
      var projects = results[0];
      var experience = results[1];
      var jobs = getWorkExperience(experience);

      if (!projects) {
        showError(featuredEl, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
        showError(selectedEl, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      } else {
        updateProjectsLink(projects.length, lang);
        renderFeatured(featuredEl, projects[0], lang);
        renderSelected(selectedEl, projects, lang);
      }

      if (!experience) {
        showError(nowEl, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
        showError(journeyEl, lang === 'en' ? "Couldn't load right now" : 'Şu an yüklenemedi');
      } else {
        renderNow(nowEl, jobs, lang);
        renderJourney(journeyEl, jobs, lang);
      }
    });
  }

  /* ------------------------------------------------------------------ */
  /* Projects page                                                      */
  /* ------------------------------------------------------------------ */

  function coverHtml(project, cat, sizeClass) {
    var mono = monogram(project.title);
    var label = projectsCategoryLabel(cat, getLang());
    var coverClass = 'project-card__cover project-card__cover--' + cat + (sizeClass ? ' ' + sizeClass : '');

    if (hasRealImage(project)) {
      return (
        '<div class="' + coverClass + '">' +
          '<img src="' + escapeHtml(project.imageUrl) + '" loading="lazy" decoding="async" width="600" height="150" alt="' +
            escapeHtml(shortTitle(project.title) + (getLang() === 'en' ? ' screenshot' : ' ekran görüntüsü')) + '">' +
          '<span class="project-card__cat">' + escapeHtml(label) + '</span>' +
        '</div>'
      );
    }

    return (
      '<div class="' + coverClass + '">' +
        '<span class="project-card__monogram" aria-hidden="true">' + escapeHtml(mono) + '</span>' +
        '<span class="project-card__cat">' + escapeHtml(label) + '</span>' +
      '</div>'
    );
  }

  function projectsInit() {
    var lang = getLang();
    var root = document.getElementById('projects-page');
    if (!root) return;

    var grid = document.getElementById('projects-grid');
    var statusEl = document.getElementById('projects-status');
    var totalEl = document.getElementById('projects-total');
    var totalMetaEl = document.getElementById('projects-total-meta');
    var searchInput = document.getElementById('projects-search');
    var segment = document.getElementById('projects-segment');
    var dialog = document.getElementById('project-dialog');
    var dialogBody = document.getElementById('project-dialog-body');
    var dialogGithub = document.getElementById('project-dialog-github');
    var allProjects = [];
    var activeFilter = 'all';
    var searchQuery = '';
    var searchTimer = null;
    var openProjectId = null;

    var t = {
      showing: function (n) {
        return lang === 'en' ? n + ' projects showing' : n + ' proje gösteriliyor';
      },
      empty: lang === 'en' ? 'No projects match this filter.' : 'Bu filtreyle eşleşen proje yok.',
      clear: lang === 'en' ? 'Clear filter' : 'Filtreyi temizle',
      loadError: lang === 'en' ? "Projects couldn't be loaded right now." : 'Projeler şu an yüklenemedi.',
      retry: lang === 'en' ? 'Try again' : 'Tekrar dene',
      githubAria: lang === 'en' ? 'GitHub repository' : 'GitHub deposu',
      githubCta: lang === 'en' ? 'View on GitHub ↗' : "GitHub'da incele ↗",
      close: lang === 'en' ? 'Close' : 'Kapat',
      projectWord: lang === 'en' ? 'projects' : 'proje',
      all: lang === 'en' ? 'All' : 'Tümü',
      web: 'Web',
      ai: lang === 'en' ? 'AI' : 'Yapay zekâ',
      ind: lang === 'en' ? 'Industrial' : 'Endüstriyel'
    };

    function parseHashId() {
      var hash = window.location.hash || '';
      var match = hash.match(/^#project-(.+)$/);
      return match ? match[1] : null;
    }

    function setHash(id) {
      var url = window.location.pathname + window.location.search + (id ? '#project-' + id : '');
      window.history.replaceState(null, '', url);
    }

    function countByCategory(list) {
      var counts = { all: list.length, web: 0, ai: 0, ind: 0 };
      list.forEach(function (p) {
        var cat = projectCategory(p);
        if (counts[cat] != null) counts[cat] += 1;
      });
      return counts;
    }

    function updateSegmentCounts(list) {
      if (!segment) return;
      var counts = countByCategory(list);
      segment.querySelectorAll('.segment__btn').forEach(function (btn) {
        var key = btn.getAttribute('data-filter');
        var countEl = btn.querySelector('.segment__count');
        if (countEl && counts[key] != null) {
          countEl.textContent = String(counts[key]);
        }
      });
    }

    function updateTotal(list) {
      if (!totalEl || !totalMetaEl) return;
      totalEl.textContent = String(list.length);
      var years = [];
      list.forEach(function (p) {
        var matches = String(p.dateRange || '').match(/\d{4}/g) || [];
        matches.forEach(function (y) { years.push(Number(y)); });
      });
      if (!years.length) {
        totalMetaEl.textContent = t.projectWord;
        return;
      }
      var minY = Math.min.apply(null, years);
      var maxY = Math.max.apply(null, years);
      totalMetaEl.textContent = t.projectWord + ' · ' + minY + ' — ' + maxY;
    }

    function matchesQuery(project, q) {
      if (!q) return true;
      var hay = [
        project.title || '',
        project.description || '',
        (project.tags || []).join(' ')
      ].join(' ').toLocaleLowerCase('tr');
      return hay.indexOf(q) !== -1;
    }

    function filteredProjects() {
      return allProjects.filter(function (p) {
        var cat = projectCategory(p);
        if (activeFilter !== 'all' && cat !== activeFilter) return false;
        return matchesQuery(p, searchQuery);
      });
    }

    function renderCoverOnly(project, cat) {
      return coverHtml(project, cat, 'project-card__cover--dialog');
    }

    function cardHtml(project) {
      var cat = projectCategory(project);
      var title = shortTitle(project.title);
      var subtitle = titleSubtitle(project.title);
      var tags = (project.tags || []).slice(0, 4).join(' · ');
      var github = project.githubUrl
        ? '<a class="project-card__github" href="' + escapeHtml(project.githubUrl) +
          '" target="_blank" rel="noopener" aria-label="' + escapeHtml(t.githubAria) + '">' +
          '<i class="bi bi-github" aria-hidden="true"></i></a>'
        : '';

      return (
        '<div class="project-card-wrap">' +
          '<button type="button" class="project-card" data-project-id="' + escapeHtml(String(project.id)) + '">' +
            coverHtml(project, cat) +
            '<div class="project-card__body">' +
              '<div class="project-card__meta"><span>' + escapeHtml(project.dateRange || '') + '</span><span aria-hidden="true">↗</span></div>' +
              '<h3 class="project-card__title">' + escapeHtml(title) + '</h3>' +
              (subtitle ? '<p class="project-card__subtitle">' + escapeHtml(subtitle) + '</p>' : '') +
              '<p class="project-card__desc project-card__desc--3">' + escapeHtml(project.description || '') + '</p>' +
              '<p class="project-card__tags">' + escapeHtml(tags) + '</p>' +
            '</div>' +
          '</button>' +
          github +
        '</div>'
      );
    }

    function renderGrid(list) {
      if (!grid) return;

      if (!list.length) {
        grid.innerHTML =
          '<div class="projects-empty tile tile--static">' +
            '<p>' + escapeHtml(t.empty) + '</p>' +
            '<button type="button" class="btn btn--secondary" data-clear-filters>' + escapeHtml(t.clear) + '</button>' +
          '</div>';
        if (statusEl) statusEl.textContent = t.showing(0);
        return;
      }

      grid.innerHTML = list.map(cardHtml).join('');
      if (statusEl) statusEl.textContent = t.showing(list.length);
    }

    function setFilterPressed() {
      if (!segment) return;
      segment.querySelectorAll('.segment__btn').forEach(function (btn) {
        btn.setAttribute('aria-pressed', btn.getAttribute('data-filter') === activeFilter ? 'true' : 'false');
      });
    }

    function applyFilters() {
      setFilterPressed();
      renderGrid(filteredProjects());
    }

    function clearFilters() {
      activeFilter = 'all';
      searchQuery = '';
      if (searchInput) searchInput.value = '';
      applyFilters();
    }

    function findProject(id) {
      var sid = String(id);
      for (var i = 0; i < allProjects.length; i++) {
        if (String(allProjects[i].id) === sid) return allProjects[i];
      }
      return null;
    }

    function fillDialog(project) {
      if (!dialogBody || !project) return;
      var cat = projectCategory(project);
      var label = projectsCategoryLabel(cat, lang).toLocaleUpperCase('tr');
      var title = project.title || '';
      var main = shortTitle(title);
      var sub = titleSubtitle(title);
      var modalHtml = Array.isArray(project.modalContent) ? project.modalContent.join('') : String(project.modalContent || '');
      var safe = sanitizeHtml(modalHtml);
      var tags = (project.tags || []).map(function (tag) {
        return '<span class="chip">' + escapeHtml(tag) + '</span>';
      }).join('');

      dialogBody.innerHTML =
        renderCoverOnly(project, cat) +
        '<p class="project-dialog__meta">' + escapeHtml(label + (project.dateRange ? ' · ' + project.dateRange : '')) + '</p>' +
        '<h2 id="project-dialog-title">' + escapeHtml(main) + '</h2>' +
        (sub ? '<p class="project-dialog__subtitle">' + escapeHtml(sub) + '</p>' : '') +
        '<p class="lead project-dialog__lead">' + escapeHtml(project.description || '') + '</p>' +
        '<div class="project-dialog__content">' + safe + '</div>' +
        (tags ? '<div class="chip-row project-dialog__tags">' + tags + '</div>' : '');

      if (dialogGithub) {
        if (project.githubUrl) {
          dialogGithub.hidden = false;
          dialogGithub.href = project.githubUrl;
          dialogGithub.textContent = t.githubCta;
        } else {
          dialogGithub.hidden = true;
          dialogGithub.removeAttribute('href');
        }
      }
    }

    function openProject(id, triggerEl) {
      var project = findProject(id);
      if (!project || !dialog) return;
      var trigger = triggerEl || document.querySelector('.project-card[data-project-id="' + String(id).replace(/"/g, '') + '"]');
      openProjectId = String(project.id);
      fillDialog(project);
      setHash(project.id);
      dialogOpen(dialog, trigger);
    }

    function showLoadError() {
      if (!grid) return;
      grid.innerHTML =
        '<div class="projects-empty tile tile--static">' +
          '<p class="load-error">' + escapeHtml(t.loadError) + '</p>' +
          '<button type="button" class="btn btn--secondary" data-retry>' + escapeHtml(t.retry) + '</button>' +
        '</div>';
      if (statusEl) statusEl.textContent = t.showing(0);
    }

    function showSkeletons() {
      if (!grid) return;
      var html = '';
      for (var i = 0; i < 6; i++) {
        html +=
          '<div class="project-card-wrap">' +
            '<div class="project-card project-card--skeleton tile--static">' +
              '<span class="skeleton skeleton--block"></span>' +
              '<span class="skeleton skeleton--line skeleton--md"></span>' +
              '<span class="skeleton skeleton--line skeleton--lg"></span>' +
              '<span class="skeleton skeleton--line"></span>' +
            '</div>' +
          '</div>';
      }
      grid.innerHTML = html;
    }

    function load() {
      showSkeletons();
      return fetchJson('projects').then(function (data) {
        if (!data || !Array.isArray(data)) {
          allProjects = [];
          showLoadError();
          return;
        }
        allProjects = data;
        updateSegmentCounts(allProjects);
        updateTotal(allProjects);
        applyFilters();

        var hashId = parseHashId();
        if (hashId) {
          window.setTimeout(function () {
            openProject(hashId);
          }, 0);
        }
      });
    }

    if (segment) {
      segment.addEventListener('click', function (e) {
        var btn = e.target.closest('.segment__btn');
        if (!btn || !segment.contains(btn)) return;
        activeFilter = btn.getAttribute('data-filter') || 'all';
        applyFilters();
      });
    }

    if (searchInput) {
      searchInput.addEventListener('input', function () {
        window.clearTimeout(searchTimer);
        searchTimer = window.setTimeout(function () {
          searchQuery = String(searchInput.value || '').trim().toLocaleLowerCase('tr');
          applyFilters();
        }, 150);
      });
    }

    if (grid) {
      grid.addEventListener('click', function (e) {
        var clearBtn = e.target.closest('[data-clear-filters]');
        if (clearBtn) {
          clearFilters();
          return;
        }
        var retryBtn = e.target.closest('[data-retry]');
        if (retryBtn) {
          load();
          return;
        }
        var card = e.target.closest('.project-card');
        if (card && grid.contains(card)) {
          openProject(card.getAttribute('data-project-id'), card);
        }
      });
    }

    if (dialog) {
      dialogBind(dialog, {
        onClose: function () {
          openProjectId = null;
          setHash(null);
        }
      });
    }

    window.addEventListener('hashchange', function () {
      var id = parseHashId();
      if (id) {
        if (String(openProjectId) !== String(id)) openProject(id);
      } else if (dialog && dialog.open) {
        dialogClose(dialog);
      }
    });

    return load();
  }

  /* ------------------------------------------------------------------ */
  /* Experience page                                                    */
  /* ------------------------------------------------------------------ */

  function parseExperienceGroups(groups) {
    var result = {
      work: [],
      internships: [],
      education: [],
      languages: [],
      hasWork: false,
      hasInternships: false,
      hasEducation: false,
      hasLanguages: false
    };
    if (!Array.isArray(groups)) return result;

    var expGroups = [];
    var itemGroups = [];
    groups.forEach(function (g) {
      if (!g) return;
      if (Array.isArray(g.experience)) expGroups.push(g);
      else if (Array.isArray(g.items)) itemGroups.push(g);
    });

    if (expGroups[0]) {
      result.work = expGroups[0].experience || [];
      result.hasWork = true;
    }
    if (expGroups[1]) {
      result.internships = expGroups[1].experience || [];
      result.hasInternships = true;
    }

    itemGroups.forEach(function (g) {
      var hasPct = (g.items || []).some(function (it) {
        return it && typeof it.percentage === 'number';
      });
      if (hasPct && !result.hasLanguages) {
        result.languages = g.items || [];
        result.hasLanguages = true;
      } else if (!hasPct && !result.hasEducation) {
        result.education = g.items || [];
        result.hasEducation = true;
      }
    });

    return result;
  }

  function languageLevel(percentage, lang) {
    var p = Number(percentage);
    if (isNaN(p)) return '';
    if (p < 40) return lang === 'en' ? 'Beginner' : 'Başlangıç';
    if (p < 70) return lang === 'en' ? 'Intermediate' : 'Orta';
    if (p < 90) return lang === 'en' ? 'Advanced' : 'İleri';
    return lang === 'en' ? 'Fluent' : 'Akıcı';
  }

  function languageCode(item) {
    var title = String((item && item.title) || '').trim();
    if (title.length >= 2) return title.slice(0, 2).toLocaleUpperCase('en');
    return String((item && item.flagCode) || '').slice(0, 2).toLocaleUpperCase('en');
  }

  function cleanCertificateDate(date) {
    return String(date || '')
      .replace(/^\s*Yayınlanma:\s*/i, '')
      .replace(/^\s*Issued:\s*/i, '')
      .trim();
  }

  function resolveMediaUrl(url) {
    var raw = String(url || '').trim();
    if (!raw) return '';
    if (raw.indexOf('~/') === 0) return '/' + raw.slice(2);
    return raw;
  }

  function toTitleCaseTr(str) {
    return String(str || '')
      .replace(/\s+/g, ' ')
      .trim()
      .split(' ')
      .filter(Boolean)
      .map(function (word) {
        return word.charAt(0).toLocaleUpperCase('tr') + word.slice(1).toLocaleLowerCase('tr');
      })
      .join(' ');
  }

  function experienceInit() {
    var lang = getLang();
    var root = document.getElementById('experience-page');
    if (!root) return;

    var statsEl = document.getElementById('exp-stats');
    var nowEl = document.getElementById('exp-now');
    var timelineEl = document.getElementById('exp-timeline');
    var eduLangEl = document.getElementById('exp-edu-lang');
    var certsEl = document.getElementById('exp-certs');
    var refsEl = document.getElementById('exp-refs');
    var certsSection = document.getElementById('exp-certs-section');
    var refsSection = document.getElementById('exp-refs-section');
    var certDialog = document.getElementById('cert-dialog');
    var certDialogBody = document.getElementById('cert-dialog-body');

    var t = {
      retry: lang === 'en' ? 'Try again' : 'Tekrar dene',
      loadExp: lang === 'en' ? "Experience couldn't be loaded right now." : 'Deneyim şu an yüklenemedi.',
      loadCerts: lang === 'en' ? "Certificates couldn't be loaded right now." : 'Sertifikalar şu an yüklenemedi.',
      loadRefs: lang === 'en' ? "References couldn't be loaded right now." : 'Referanslar şu an yüklenemedi.',
      roles: lang === 'en' ? 'roles' : 'pozisyon',
      internships: lang === 'en' ? 'internships' : 'staj',
      certificates: lang === 'en' ? 'certificates' : 'sertifika',
      now: lang === 'en' ? 'Now' : 'Şu an',
      work: lang === 'en' ? 'Work experience' : 'İş deneyimi',
      internTitle: lang === 'en' ? 'Internships' : 'Stajlar',
      readMore: lang === 'en' ? 'Read more' : 'Devamını oku',
      readLess: lang === 'en' ? 'Show less' : 'Daha az göster',
      imgMissing: lang === 'en' ? 'Image not found' : 'Görsel bulunamadı',
      certAlt: lang === 'en' ? ' certificate' : ' sertifikası',
      close: lang === 'en' ? 'Close' : 'Kapat',
      refNote: lang === 'en' ? 'Contact details available on request.' : 'İletişim bilgileri talep üzerine paylaşılır.'
    };

    var certsCache = [];

    function sectionError(el, message, retryAttr) {
      if (!el) return;
      el.hidden = false;
      el.innerHTML =
        '<div class="tile tile--static exp-section-error">' +
          '<p class="load-error">' + escapeHtml(message) + '</p>' +
          '<button type="button" class="btn btn--secondary" data-retry="' + escapeHtml(retryAttr) + '">' +
            escapeHtml(t.retry) +
          '</button>' +
        '</div>';
    }

    function wireExpandable(scope) {
      if (!scope) return;
      scope.querySelectorAll('[data-expandable]').forEach(function (block) {
        var text = block.querySelector('[data-clamp]');
        var btn = block.querySelector('[data-expand]');
        if (!text || !btn) return;
        if (text.scrollHeight <= text.clientHeight + 2) {
          btn.hidden = true;
          return;
        }
        btn.hidden = false;
        btn.addEventListener('click', function () {
          var open = btn.getAttribute('aria-expanded') === 'true';
          var next = !open;
          btn.setAttribute('aria-expanded', next ? 'true' : 'false');
          text.classList.toggle('is-expanded', next);
          btn.textContent = next ? t.readLess : t.readMore;
        });
      });
    }

    function timelineItemHtml(item, index, total, opts) {
      opts = opts || {};
      var isCurrent = !!opts.current;
      var small = !!opts.small;
      var tags = (item.tags || []).map(function (tag) {
        return '<span class="chip">' + escapeHtml(tag) + '</span>';
      }).join('');
      var date = shortRange(item.dateRange);
      var itemClass = 'timeline__item' + (isCurrent ? ' timeline__item--current' : '') + (small ? ' timeline__item--small' : '');

      return (
        '<article class="' + itemClass + '" data-expandable>' +
          '<div class="timeline__date">' + escapeHtml(date) + '</div>' +
          '<div class="timeline__rail"><span class="timeline__dot" aria-hidden="true"></span></div>' +
          '<div class="timeline__content">' +
            '<span class="timeline__date-mobile">' + escapeHtml(date) + '</span>' +
            '<h3 class="timeline__role">' + escapeHtml(item.company || '') + '</h3>' +
            '<div class="timeline__company">' + escapeHtml(item.title || '') + '</div>' +
            '<p class="timeline__desc" data-clamp>' + escapeHtml(item.description || '') + '</p>' +
            '<button type="button" class="timeline__more" data-expand aria-expanded="false" hidden>' + escapeHtml(t.readMore) + '</button>' +
            (tags ? '<div class="chip-row">' + tags + '</div>' : '') +
          '</div>' +
        '</article>'
      );
    }

    function renderStats(workCount, internCount, certCount) {
      if (!statsEl) return;
      statsEl.innerHTML =
        '<div class="exp-stat"><span class="exp-stat__num">' + escapeHtml(String(workCount)) + '</span><span class="exp-stat__label">' + escapeHtml(t.roles) + '</span></div>' +
        '<div class="exp-stat"><span class="exp-stat__num">' + escapeHtml(String(internCount)) + '</span><span class="exp-stat__label">' + escapeHtml(t.internships) + '</span></div>' +
        '<div class="exp-stat"><span class="exp-stat__num">' + escapeHtml(String(certCount)) + '</span><span class="exp-stat__label">' + escapeHtml(t.certificates) + '</span></div>';
    }

    function renderNow(work) {
      if (!nowEl) return;
      if (!work || !work.length) {
        nowEl.hidden = true;
        return;
      }
      nowEl.hidden = false;
      var current = work[0];
      nowEl.innerHTML =
        '<p class="eyebrow">' + escapeHtml(t.now) + '</p>' +
        '<h3 class="now-company">' + escapeHtml(current.company || '') + '</h3>' +
        '<p class="now-title">' + escapeHtml(current.title || '') + '</p>';
    }

    function renderTimeline(parsed) {
      if (!timelineEl) return;
      if (!parsed.hasWork && !parsed.hasInternships) {
        timelineEl.hidden = true;
        return;
      }
      timelineEl.hidden = false;

      var html = '<div class="tile tile--static span-4 exp-timeline-tile">';
      if (parsed.hasWork) {
        html +=
          '<p class="eyebrow">' + escapeHtml(lang === 'en' ? 'Career' : 'Kariyer') + '</p>' +
          '<h2>' + escapeHtml(t.work) + '</h2>' +
          '<div class="timeline">';
        parsed.work.forEach(function (item, i) {
          html += timelineItemHtml(item, i, parsed.work.length, { current: i === 0 });
        });
        html += '</div>';
      }

      if (parsed.hasInternships && parsed.internships.length) {
        html +=
          '<div class="exp-internships">' +
            '<h3 class="exp-subhead">' + escapeHtml(t.internTitle) + '</h3>' +
            '<div class="timeline timeline--compact">';
        parsed.internships.forEach(function (item, i) {
          html += timelineItemHtml(item, i, parsed.internships.length, { small: true });
        });
        html += '</div></div>';
      }

      html += '</div>';
      timelineEl.innerHTML = html;
      window.setTimeout(function () {
        wireExpandable(timelineEl);
      }, 0);
    }

    function renderEduLang(parsed) {
      if (!eduLangEl) return;
      if (!parsed.hasEducation && !parsed.hasLanguages) {
        eduLangEl.hidden = true;
        return;
      }
      eduLangEl.hidden = false;
      var html = '';

      if (parsed.hasEducation) {
        parsed.education.forEach(function (item, index) {
          var span = index === 0 ? ' span-2' : '';
          var sub = item.company
            ? escapeHtml(item.company)
            : escapeHtml(item.description || '');
          var extra = item.company && item.description
            ? '<p class="edu-desc">' + escapeHtml(item.description) + '</p>'
            : '';
          html +=
            '<div class="tile tile--static' + span + '">' +
              '<p class="edu-date">' + escapeHtml(item.dateRange || '') + '</p>' +
              '<h3>' + escapeHtml(item.title || '') + '</h3>' +
              (sub ? '<p class="edu-sub">' + sub + '</p>' : '') +
              extra +
            '</div>';
        });
      }

      if (parsed.hasLanguages && parsed.languages.length) {
        html += '<div class="tile tile--static exp-lang-tile"><p class="eyebrow">' +
          escapeHtml(lang === 'en' ? 'Languages' : 'Diller') + '</p>';
        parsed.languages.forEach(function (item) {
          html +=
            '<div class="exp-lang">' +
              '<span class="exp-lang__code">' + escapeHtml(languageCode(item)) + '</span>' +
              '<div class="exp-lang__text">' +
                '<strong>' + escapeHtml(item.title || '') + '</strong>' +
                '<span>' + escapeHtml(languageLevel(item.percentage, lang)) + '</span>' +
              '</div>' +
            '</div>';
        });
        html += '</div>';
      }

      eduLangEl.innerHTML = html;
    }

    function openCert(id, triggerEl) {
      var cert = null;
      for (var i = 0; i < certsCache.length; i++) {
        if (String(certsCache[i].id) === String(id)) {
          cert = certsCache[i];
          break;
        }
      }
      if (!cert || !certDialog || !certDialogBody) return;

      var src = resolveMediaUrl(cert.imageUrl);
      var date = cleanCertificateDate(cert.date);
      var skills = (cert.skills || []).map(function (s) {
        return '<span class="chip">' + escapeHtml(s) + '</span>';
      }).join('');
      var alt = escapeHtml((cert.title || '') + t.certAlt);

      certDialogBody.innerHTML =
        '<div class="cert-dialog__media">' +
          (src
            ? '<img src="' + escapeHtml(src) + '" alt="' + alt + '" loading="lazy" decoding="async" width="960" height="680" data-cert-img>'
            : '<p class="load-error">' + escapeHtml(t.imgMissing) + '</p>') +
        '</div>' +
        '<h2 id="cert-dialog-title">' + escapeHtml(cert.title || '') + '</h2>' +
        '<p class="cert-dialog__provider">' + escapeHtml(cert.provider || '') + '</p>' +
        (date ? '<p class="project-dialog__meta">' + escapeHtml(date) + '</p>' : '') +
        (skills ? '<div class="chip-row">' + skills + '</div>' : '');

      dialogOpen(certDialog, triggerEl);

      var img = certDialogBody.querySelector('[data-cert-img]');
      if (img) {
        img.addEventListener('error', function () {
          img.replaceWith((function () {
            var p = document.createElement('p');
            p.className = 'load-error';
            p.textContent = t.imgMissing;
            return p;
          })());
        });
      }
    }

    function renderCerts(list) {
      if (!certsEl || !certsSection) return;
      if (!list || !list.length) {
        certsSection.hidden = true;
        return;
      }
      certsSection.hidden = false;
      certsCache = list;
      certsEl.innerHTML = list.map(function (cert) {
        return (
          '<button type="button" class="tile tile--static cert-card" data-cert-id="' + escapeHtml(String(cert.id)) + '">' +
            '<p class="cert-card__date">' + escapeHtml(cleanCertificateDate(cert.date)) + '</p>' +
            '<h3 class="cert-card__title">' + escapeHtml(cert.title || '') + '</h3>' +
            '<p class="cert-card__provider">' + escapeHtml(cert.provider || '') + '</p>' +
          '</button>'
        );
      }).join('');
    }

    function renderRefs(list) {
      if (!refsEl || !refsSection) return;
      if (!list || !list.length) {
        refsSection.hidden = true;
        return;
      }
      refsSection.hidden = false;
      refsEl.innerHTML = list.map(function (ref) {
        return (
          '<div class="tile tile--static ref-card">' +
            '<h3 class="ref-card__name">' + escapeHtml(toTitleCaseTr(ref.name)) + '</h3>' +
            '<p class="ref-card__title">' + escapeHtml(ref.title || '') + '</p>' +
            '<p class="ref-card__company">' + escapeHtml(ref.company || '') + '</p>' +
            '<p class="ref-card__note">' + escapeHtml(t.refNote) + '</p>' +
          '</div>'
        );
      }).join('');
    }

    function loadExperience() {
      if (statsEl) {
        statsEl.innerHTML =
          '<span class="skeleton skeleton--line skeleton--sm"></span>' +
          '<span class="skeleton skeleton--line skeleton--lg"></span>';
      }
      if (nowEl) {
        nowEl.hidden = false;
        nowEl.innerHTML =
          '<span class="skeleton skeleton--line skeleton--sm"></span>' +
          '<span class="skeleton skeleton--line skeleton--lg"></span>' +
          '<span class="skeleton skeleton--line"></span>';
      }
      if (timelineEl) {
        timelineEl.hidden = false;
        timelineEl.innerHTML =
          '<div class="tile tile--static span-4">' +
            '<span class="skeleton skeleton--line skeleton--md"></span>' +
            '<span class="skeleton skeleton--line skeleton--lg"></span>' +
            '<span class="skeleton skeleton--line"></span>' +
          '</div>';
      }
      if (eduLangEl) {
        eduLangEl.hidden = false;
        eduLangEl.innerHTML =
          '<div class="tile tile--static span-2"><span class="skeleton skeleton--line"></span><span class="skeleton skeleton--line skeleton--lg"></span></div>' +
          '<div class="tile tile--static"><span class="skeleton skeleton--line"></span><span class="skeleton skeleton--line skeleton--md"></span></div>';
      }

      return fetchJson('experience').then(function (data) {
        if (!data) {
          sectionError(timelineEl, t.loadExp, 'experience');
          if (eduLangEl) sectionError(eduLangEl, t.loadExp, 'experience');
          if (statsEl) {
            statsEl.innerHTML = '<p class="load-error">' + escapeHtml(t.loadExp) + '</p>';
          }
          if (nowEl) nowEl.hidden = true;
          return null;
        }
        var parsed = parseExperienceGroups(data);
        renderNow(parsed.work);
        renderTimeline(parsed);
        renderEduLang(parsed);
        return parsed;
      });
    }

    function loadCertificates() {
      if (certsSection) certsSection.hidden = false;
      if (certsEl) {
        certsEl.innerHTML = '';
        for (var i = 0; i < 5; i++) {
          certsEl.innerHTML +=
            '<div class="tile tile--static cert-card"><span class="skeleton skeleton--line skeleton--sm"></span><span class="skeleton skeleton--line"></span><span class="skeleton skeleton--line skeleton--md"></span></div>';
        }
      }
      return fetchJson('certificates').then(function (data) {
        if (!data) {
          if (certsSection) certsSection.hidden = false;
          sectionError(certsEl, t.loadCerts, 'certificates');
          return [];
        }
        renderCerts(data);
        return data;
      });
    }

    function loadReferences() {
      if (refsSection) refsSection.hidden = false;
      if (refsEl) {
        refsEl.innerHTML = '';
        for (var i = 0; i < 4; i++) {
          refsEl.innerHTML +=
            '<div class="tile tile--static"><span class="skeleton skeleton--line skeleton--lg"></span><span class="skeleton skeleton--line"></span><span class="skeleton skeleton--line skeleton--sm"></span></div>';
        }
      }
      return fetchJson('references').then(function (data) {
        if (!data) {
          sectionError(refsEl, t.loadRefs, 'references');
          return [];
        }
        renderRefs(data);
        return data;
      });
    }

    function refreshStats(parsed, certs) {
      var workCount = parsed && parsed.hasWork ? parsed.work.length : 0;
      var internCount = parsed && parsed.hasInternships ? parsed.internships.length : 0;
      var certCount = Array.isArray(certs) ? certs.length : 0;
      if (parsed || Array.isArray(certs)) {
        renderStats(workCount, internCount, certCount);
      }
    }

    var parsedState = null;
    var certsState = null;

    loadExperience().then(function (parsed) {
      parsedState = parsed;
      refreshStats(parsedState, certsState);
    });
    loadCertificates().then(function (certs) {
      certsState = certs;
      refreshStats(parsedState, certsState);
    });
    loadReferences();

    if (certDialog) {
      dialogBind(certDialog);
    }

    root.addEventListener('click', function (e) {
      var retry = e.target.closest('[data-retry]');
      if (retry && root.contains(retry)) {
        var kind = retry.getAttribute('data-retry');
        if (kind === 'experience') {
          loadExperience().then(function (parsed) {
            parsedState = parsed;
            refreshStats(parsedState, certsState);
          });
        } else if (kind === 'certificates') {
          loadCertificates().then(function (certs) {
            certsState = certs;
            refreshStats(parsedState, certsState);
          });
        } else if (kind === 'references') {
          loadReferences();
        }
        return;
      }

      var certBtn = e.target.closest('[data-cert-id]');
      if (certBtn && root.contains(certBtn)) {
        openCert(certBtn.getAttribute('data-cert-id'), certBtn);
      }
    });
  }

  /* ------------------------------------------------------------------ */
  /* About page                                                         */
  /* ------------------------------------------------------------------ */

  function splitSentences(text) {
    var raw = String(text || '').replace(/\r\n/g, '\n');
    var blocks = raw.split(/\n+/).map(function (b) { return b.trim(); }).filter(Boolean);
    var sentences = [];

    blocks.forEach(function (block) {
      var parts = block.split('. ');
      parts.forEach(function (part, index) {
        var piece = part.trim();
        if (!piece) return;
        if (index < parts.length - 1 && !/[.!?…]$/.test(piece)) {
          piece += '.';
        }
        sentences.push(piece);
      });
    });

    return sentences;
  }

  function splitAboutParagraphs(text) {
    var sentences = splitSentences(text);
    if (!sentences.length) return [];
    if (sentences.length === 1) return [sentences[0]];
    var mid = Math.ceil(sentences.length / 2);
    return [
      sentences.slice(0, mid).join(' '),
      sentences.slice(mid).join(' ')
    ].filter(Boolean);
  }

  function firstSentence(text) {
    var sentences = splitSentences(text);
    return sentences[0] || String(text || '').trim();
  }

  function firstParagraphPlain(descParts) {
    var html = Array.isArray(descParts) ? descParts.join('') : String(descParts || '');
    var doc = new DOMParser().parseFromString('<div id="about-plain-root">' + html + '</div>', 'text/html');
    var root = doc.getElementById('about-plain-root');
    if (!root) return '';
    var p = root.querySelector('p');
    return ((p && p.textContent) || root.textContent || '').replace(/\s+/g, ' ').trim();
  }

  function normalizeCompanyKey(name) {
    var s = String(name || '').trim();
    var idx = s.indexOf(' - ');
    if (idx !== -1) s = s.slice(0, idx);
    return s.trim().toLocaleLowerCase('tr');
  }

  function uniqueCompanyCount(experienceData) {
    if (!Array.isArray(experienceData)) return null;
    var seen = {};
    var count = 0;
    experienceData.forEach(function (group) {
      (group && group.experience ? group.experience : []).forEach(function (item) {
        var key = normalizeCompanyKey(item && item.company);
        if (!key || seen[key]) return;
        seen[key] = true;
        count += 1;
      });
    });
    return count;
  }

  function aboutInit() {
    var lang = getLang();
    var root = document.getElementById('about-page');
    if (!root) return;

    var introEl = document.getElementById('about-intro-copy');
    var goalEl = document.getElementById('about-goal');
    var statsEl = document.getElementById('about-stats');
    var beyondEl = document.getElementById('about-beyond');
    var beyondSection = document.getElementById('about-beyond-section');
    var goalDialog = document.getElementById('about-goal-dialog');
    var goalDialogBody = document.getElementById('about-goal-dialog-body');
    var galleryDialog = document.getElementById('about-gallery-dialog');
    var galleryMain = document.getElementById('about-gallery-main');
    var galleryCounter = document.getElementById('about-gallery-counter');
    var galleryThumbs = document.getElementById('about-gallery-thumbs');
    var galleryCopy = document.getElementById('about-gallery-copy');
    var galleryPrev = document.getElementById('about-gallery-prev');
    var galleryNext = document.getElementById('about-gallery-next');

    var activitiesCache = [];
    var galleryIndex = 0;
    var galleryImages = [];
    var stats = { companies: null, projects: null, certificates: null };

    var t = {
      retry: lang === 'en' ? 'Try again' : 'Tekrar dene',
      loadAbout: lang === 'en' ? "About couldn't be loaded right now." : 'Hakkımda şu an yüklenemedi.',
      loadBeyond: lang === 'en' ? "Activities couldn't be loaded right now." : 'Aktiviteler şu an yüklenemedi.',
      goalEyebrow: lang === 'en' ? 'Goal' : 'Hedef',
      readFull: lang === 'en' ? 'Read full' : 'Tamamını oku',
      companies: lang === 'en' ? 'companies' : 'şirket',
      projects: lang === 'en' ? 'projects' : 'proje',
      certificates: lang === 'en' ? 'certificates' : 'sertifika',
      gallery: function (n) {
        return lang === 'en' ? 'Gallery · ' + n + ' photos' : 'Galeri · ' + n + ' fotoğraf';
      },
      prev: lang === 'en' ? '‹ Previous' : '‹ Önceki',
      next: lang === 'en' ? 'Next ›' : 'Sonraki ›',
      prevAria: lang === 'en' ? 'Previous photo' : 'Önceki fotoğraf',
      nextAria: lang === 'en' ? 'Next photo' : 'Sonraki fotoğraf',
      photoAlt: function (title, i) {
        return title + (lang === 'en' ? ' — photo ' : ' — fotoğraf ') + i;
      },
      freeTitle: lang === 'en' ? 'In my free time' : 'Boş zamanlarda',
      freeItems: lang === 'en'
        ? ['Football & sports', 'Visiting historical sites', 'Photography']
        : ['Futbol ve spor', 'Tarihi yerleri gezmek', 'Fotoğraf çekmek']
    };

    function dash(value) {
      return value == null ? '—' : String(value);
    }

    function renderStats() {
      if (!statsEl) return;
      statsEl.innerHTML =
        '<div class="about-stat">' +
          '<span class="about-stat__num">' + escapeHtml(dash(stats.companies)) + '</span>' +
          '<span class="about-stat__label">' + escapeHtml(t.companies) + '</span>' +
        '</div>' +
        '<div class="about-stat about-stat--accent">' +
          '<span class="about-stat__num">' + escapeHtml(dash(stats.projects)) + '</span>' +
          '<span class="about-stat__label">' + escapeHtml(t.projects) + '</span>' +
        '</div>' +
        '<div class="about-stat">' +
          '<span class="about-stat__num">' + escapeHtml(dash(stats.certificates)) + '</span>' +
          '<span class="about-stat__label">' + escapeHtml(t.certificates) + '</span>' +
        '</div>';
    }

    function renderIntro(aboutText) {
      if (!introEl) return;
      var paragraphs = splitAboutParagraphs(aboutText);
      introEl.innerHTML = paragraphs.map(function (p) {
        return '<p class="about-copy">' + escapeHtml(p) + '</p>';
      }).join('');
    }

    function renderGoal(goalText) {
      if (!goalEl) return;
      var text = String(goalText || '').trim();
      var long = text.length > 220;
      var shown = long ? firstSentence(text) : text;

      goalEl.innerHTML =
        '<p class="eyebrow">' + escapeHtml(t.goalEyebrow) + '</p>' +
        '<p class="about-goal__text">' + escapeHtml(shown) + '</p>' +
        (long
          ? '<button type="button" class="btn btn--secondary about-goal__more" data-open-goal>' +
              escapeHtml(t.readFull) +
            '</button>'
          : '');

      if (goalDialogBody) {
        goalDialogBody.innerHTML =
          '<p class="eyebrow">' + escapeHtml(t.goalEyebrow) + '</p>' +
          '<h2 id="about-goal-dialog-title" class="visually-hidden">' + escapeHtml(t.goalEyebrow) + '</h2>' +
          '<p class="about-goal__text about-goal__text--full">' + escapeHtml(text) + '</p>';
      }
    }

    function freeTimeCardHtml() {
      var rows = t.freeItems.map(function (item) {
        return '<span class="about-freetime__row">' + escapeHtml(item) + '</span>';
      }).join('');
      return (
        '<div class="tile about-freetime tile--static">' +
          '<p class="eyebrow">' + escapeHtml(lang === 'en' ? 'Off the clock' : 'Kod dışında') + '</p>' +
          '<h3>' + escapeHtml(t.freeTitle) + '</h3>' +
          '<div class="about-freetime__list">' + rows + '</div>' +
        '</div>'
      );
    }

    function activityCardHtml(activity, index) {
      var images = (activity.carouselImages || []).map(resolveMediaUrl).filter(Boolean);
      var cover = images[0] || '';
      var excerpt = firstParagraphPlain(activity.description);
      var span = index === 0 ? ' span-2' : '';
      var n = images.length;

      return (
        '<div class="tile tile--static about-activity' + span + '">' +
          (cover
            ? '<img class="about-activity__bg" src="' + escapeHtml(cover) + '" alt="' +
                escapeHtml(t.photoAlt(activity.title || '', 1)) +
                '" loading="lazy" decoding="async" width="800" height="600" data-activity-img>'
            : '') +
          '<div class="about-activity__panel">' +
            '<div class="about-activity__text">' +
              '<h3>' + escapeHtml(activity.title || '') + '</h3>' +
              '<p class="about-activity__excerpt">' + escapeHtml(excerpt) + '</p>' +
            '</div>' +
            (n
              ? '<button type="button" class="btn btn--secondary about-activity__gallery" data-gallery-id="' +
                  escapeHtml(String(activity.id)) + '">' + escapeHtml(t.gallery(n)) + '</button>'
              : '') +
          '</div>' +
        '</div>'
      );
    }

    function renderBeyond(activities) {
      if (!beyondEl || !beyondSection) return;
      if (!activities || !activities.length) {
        beyondSection.hidden = true;
        return;
      }
      beyondSection.hidden = false;
      activitiesCache = activities;
      beyondEl.innerHTML = activities.map(activityCardHtml).join('') + freeTimeCardHtml();

      beyondEl.querySelectorAll('[data-activity-img]').forEach(function (img) {
        img.addEventListener('error', function () {
          var card = img.closest('.about-activity');
          if (card) card.classList.add('about-activity--fallback');
          img.remove();
        });
      });
    }

    function findActivity(id) {
      for (var i = 0; i < activitiesCache.length; i++) {
        if (String(activitiesCache[i].id) === String(id)) return activitiesCache[i];
      }
      return null;
    }

    function preloadUrl(url) {
      if (!url) return;
      var img = new Image();
      img.src = url;
    }

    function ensureThumbSrc(index) {
      if (!galleryThumbs) return;
      var img = galleryThumbs.querySelector('[data-thumb-index="' + index + '"] img');
      if (!img) return;
      var src = img.getAttribute('data-src');
      if (src && img.getAttribute('src') !== src) {
        img.src = src;
      }
    }

    function updateGalleryView() {
      if (!galleryImages.length || !galleryMain) return;
      var title = galleryMain.getAttribute('data-activity-title') || '';
      var current = galleryImages[galleryIndex];
      var next = galleryImages[(galleryIndex + 1) % galleryImages.length];
      galleryMain.src = current;
      galleryMain.alt = t.photoAlt(title, galleryIndex + 1);
      if (galleryCounter) {
        galleryCounter.textContent = (galleryIndex + 1) + ' / ' + galleryImages.length;
      }
      preloadUrl(next);
      ensureThumbSrc(galleryIndex);
      ensureThumbSrc((galleryIndex + 1) % galleryImages.length);

      if (galleryThumbs) {
        galleryThumbs.querySelectorAll('[data-thumb-index]').forEach(function (btn) {
          var active = Number(btn.getAttribute('data-thumb-index')) === galleryIndex;
          btn.classList.toggle('is-active', active);
          btn.setAttribute('aria-current', active ? 'true' : 'false');
        });
        var activeBtn = galleryThumbs.querySelector('[data-thumb-index="' + galleryIndex + '"]');
        if (activeBtn && typeof activeBtn.scrollIntoView === 'function') {
          activeBtn.scrollIntoView({ inline: 'center', block: 'nearest', behavior: 'smooth' });
        }
      }
    }

    function openGallery(activityId, triggerEl) {
      var activity = findActivity(activityId);
      if (!activity || !galleryDialog) return;

      galleryImages = (activity.carouselImages || []).map(resolveMediaUrl).filter(Boolean);
      galleryIndex = 0;
      if (!galleryImages.length) return;

      if (galleryMain) {
        galleryMain.setAttribute('data-activity-title', activity.title || '');
        galleryMain.width = 1200;
        galleryMain.height = 800;
      }

      if (galleryCopy) {
        var html = Array.isArray(activity.description) ? activity.description.join('') : String(activity.description || '');
        galleryCopy.innerHTML =
          '<h2 class="gallery-dialog__title">' + escapeHtml(activity.title || '') + '</h2>' +
          '<div class="project-dialog__content">' + sanitizeHtml(html) + '</div>';
      }

      if (galleryThumbs) {
        galleryThumbs.innerHTML = galleryImages.map(function (src, index) {
          return (
            '<button type="button" class="gallery-dialog__thumb" data-thumb-index="' + index + '" aria-label="' +
              escapeHtml(t.photoAlt(activity.title || '', index + 1)) + '">' +
              '<img data-src="' + escapeHtml(src) + '" alt="" width="72" height="54">' +
            '</button>'
          );
        }).join('');
      }

      if (galleryPrev) {
        galleryPrev.textContent = t.prev;
        galleryPrev.setAttribute('aria-label', t.prevAria);
      }
      if (galleryNext) {
        galleryNext.textContent = t.next;
        galleryNext.setAttribute('aria-label', t.nextAria);
      }

      updateGalleryView();
      dialogOpen(galleryDialog, triggerEl);
    }

    function stepGallery(delta) {
      if (!galleryImages.length) return;
      galleryIndex = (galleryIndex + delta + galleryImages.length) % galleryImages.length;
      updateGalleryView();
    }

    function showAboutError() {
      if (introEl) {
        introEl.innerHTML =
          '<p class="load-error">' + escapeHtml(t.loadAbout) + '</p>' +
          '<button type="button" class="btn btn--secondary" data-retry="about">' + escapeHtml(t.retry) + '</button>';
      }
      if (goalEl) {
        goalEl.innerHTML = '<p class="load-error">' + escapeHtml(t.loadAbout) + '</p>';
      }
      if (beyondSection) beyondSection.hidden = false;
      if (beyondEl) {
        beyondEl.innerHTML =
          '<div class="tile tile--static span-4 exp-section-error">' +
            '<p class="load-error">' + escapeHtml(t.loadBeyond) + '</p>' +
            '<button type="button" class="btn btn--secondary" data-retry="about">' + escapeHtml(t.retry) + '</button>' +
          '</div>';
      }
    }

    function loadAbout() {
      if (introEl) {
        introEl.innerHTML =
          '<span class="skeleton skeleton--line skeleton--lg"></span>' +
          '<span class="skeleton skeleton--line"></span>' +
          '<span class="skeleton skeleton--line skeleton--md"></span>';
      }
      if (goalEl) {
        goalEl.innerHTML =
          '<span class="skeleton skeleton--line skeleton--sm skeleton--on-accent"></span>' +
          '<span class="skeleton skeleton--line skeleton--lg skeleton--on-accent"></span>' +
          '<span class="skeleton skeleton--line skeleton--on-accent"></span>';
      }
      if (beyondEl) {
        beyondEl.innerHTML =
          '<div class="tile tile--static span-2 about-activity about-activity--skeleton"></div>' +
          '<div class="tile tile--static about-activity about-activity--skeleton"></div>' +
          '<div class="tile tile--static about-freetime"><span class="skeleton skeleton--line"></span></div>';
      }

      return fetchJson('about').then(function (data) {
        if (!data) {
          showAboutError();
          return;
        }
        renderIntro(data.aboutMeText || '');
        renderGoal(data.futureGoalText || '');
        renderBeyond(data.socialActivities || []);
      });
    }

    function loadCompanies() {
      return fetchJson('experience').then(function (data) {
        stats.companies = data ? uniqueCompanyCount(data) : null;
        renderStats();
      });
    }

    function loadProjects() {
      return fetchJson('projects').then(function (data) {
        stats.projects = data && Array.isArray(data) ? data.length : null;
        renderStats();
      });
    }

    function loadCertificates() {
      return fetchJson('certificates').then(function (data) {
        stats.certificates = data && Array.isArray(data) ? data.length : null;
        renderStats();
      });
    }

    renderStats();
    loadAbout();
    loadCompanies();
    loadProjects();
    loadCertificates();

    if (goalDialog) dialogBind(goalDialog);
    if (galleryDialog) {
      dialogBind(galleryDialog);
      galleryDialog.addEventListener('keydown', function (e) {
        if (!galleryDialog.open) return;
        if (e.key === 'ArrowLeft') {
          e.preventDefault();
          stepGallery(-1);
        } else if (e.key === 'ArrowRight') {
          e.preventDefault();
          stepGallery(1);
        }
      });
    }

    if (galleryPrev) {
      galleryPrev.addEventListener('click', function () { stepGallery(-1); });
    }
    if (galleryNext) {
      galleryNext.addEventListener('click', function () { stepGallery(1); });
    }

    if (galleryThumbs) {
      galleryThumbs.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-thumb-index]');
        if (!btn || !galleryThumbs.contains(btn)) return;
        var idx = Number(btn.getAttribute('data-thumb-index'));
        if (isNaN(idx)) return;
        galleryIndex = idx;
        updateGalleryView();
      });
    }

    root.addEventListener('click', function (e) {
      var retry = e.target.closest('[data-retry="about"]');
      if (retry && root.contains(retry)) {
        loadAbout();
        return;
      }

      var goalBtn = e.target.closest('[data-open-goal]');
      if (goalBtn && root.contains(goalBtn) && goalDialog) {
        dialogOpen(goalDialog, goalBtn);
        return;
      }

      var galleryBtn = e.target.closest('[data-gallery-id]');
      if (galleryBtn && root.contains(galleryBtn)) {
        openGallery(galleryBtn.getAttribute('data-gallery-id'), galleryBtn);
      }
    });
  }

  window.Bento = {
    getLang: getLang,
    fetchJson: fetchJson,
    escapeHtml: escapeHtml,
    copyToClipboard: copyToClipboard,
    projectCategory: projectCategory,
    monogram: monogram,
    yearOf: yearOf,
    shortRange: shortRange,
    sanitizeHtml: sanitizeHtml,
    confirm: confirmDialog,
    dialog: {
      open: dialogOpen,
      close: dialogClose,
      bind: dialogBind
    },
    home: {
      init: homeInit
    },
    projects: {
      init: projectsInit
    },
    experience: {
      init: experienceInit
    },
    about: {
      init: aboutInit
    },
    contact: {
      init: contactInit
    }
  };

  function contactInit() {
    var lang = getLang();
    var root = document.getElementById('contact-page');
    var form = document.getElementById('contact-form');
    var panel = document.getElementById('contact-form-panel');
    if (!root || !form || !panel) return;

    var nameInput = document.getElementById('contact-name');
    var emailInput = document.getElementById('contact-email');
    var messageInput = document.getElementById('contact-message');
    var konuInput = document.getElementById('contact-konu');
    var subjectInput = document.getElementById('contact-subject');
    var gotchaInput = document.getElementById('contact-gotcha');
    var submitBtn = document.getElementById('contact-submit');
    var formError = document.getElementById('contact-form-error');
    var copyBtn = document.getElementById('contact-copy');
    var emailDisplay = document.getElementById('contact-email-display');
    var topics = root.querySelectorAll('.contact-topic');
    var formMarkup = form.outerHTML;
    var eyebrowHtml = panel.querySelector('.eyebrow') ? panel.querySelector('.eyebrow').outerHTML : '';
    var titleHtml = panel.querySelector('.contact-title') ? panel.querySelector('.contact-title').outerHTML : '';

    var t = {
      required: lang === 'en' ? 'This field is required' : 'Bu alan zorunlu',
      email: lang === 'en' ? 'Enter a valid email' : 'Geçerli bir e-posta gir',
      messageShort: lang === 'en' ? 'Could you add a bit more detail?' : 'Biraz daha detay yazar mısın?',
      sending: lang === 'en' ? 'Sending…' : 'Gönderiliyor…',
      send: lang === 'en' ? 'Send →' : 'Gönder →',
      successTitle: lang === 'en' ? 'Message received!' : 'Mesajın ulaştı!',
      successBody: lang === 'en' ? "I'll get back to you soon." : 'En kısa sürede dönüş yapacağım.',
      again: lang === 'en' ? 'Write another message' : 'Yeni mesaj yaz',
      genericError: lang === 'en'
        ? 'Something went wrong. Please try again or email me directly.'
        : 'Bir hata oluştu, lütfen tekrar dene ya da doğrudan e-posta gönder.',
      copied: lang === 'en' ? 'Copied ✓' : 'Kopyalandı ✓'
    };

    function fieldErrorEl(input) {
      return document.getElementById(input.id + '-error');
    }

    function clearFieldError(input) {
      var err = fieldErrorEl(input);
      input.removeAttribute('aria-invalid');
      input.removeAttribute('aria-describedby');
      if (err) {
        err.hidden = true;
        err.textContent = '';
      }
    }

    function setFieldError(input, message) {
      var err = fieldErrorEl(input);
      input.setAttribute('aria-invalid', 'true');
      if (err) {
        err.hidden = false;
        err.textContent = message;
        input.setAttribute('aria-describedby', err.id);
      }
    }

    function clearFormError() {
      if (!formError) return;
      formError.hidden = true;
      formError.textContent = '';
    }

    function setFormError(message) {
      if (!formError) return;
      formError.hidden = false;
      formError.textContent = message;
    }

    function isValidEmail(value) {
      return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(value || '').trim());
    }

    function selectTopic(btn) {
      if (!btn) return;
      topics.forEach(function (el) {
        el.setAttribute('aria-pressed', el === btn ? 'true' : 'false');
      });
      var topic = btn.getAttribute('data-topic') || btn.textContent.trim();
      if (konuInput) konuInput.value = topic;
      if (subjectInput) subjectInput.value = 'Portfolyo · ' + topic;
    }

    function applyTopicFromQuery() {
      var params = new URLSearchParams(window.location.search);
      var key = (params.get('konu') || '').toLocaleLowerCase('tr');
      if (!key) return;
      var map = { is: 'is', proje: 'proje', merhaba: 'merhaba' };
      var matched = map[key];
      if (!matched) return;
      var btn = root.querySelector('.contact-topic[data-topic-key="' + matched + '"]');
      if (btn) selectTopic(btn);
    }

    function validate() {
      clearFormError();
      [nameInput, emailInput, messageInput].forEach(clearFieldError);
      var firstInvalid = null;

      if (!String(nameInput.value || '').trim()) {
        setFieldError(nameInput, t.required);
        firstInvalid = firstInvalid || nameInput;
      }

      var emailVal = String(emailInput.value || '').trim();
      if (!emailVal) {
        setFieldError(emailInput, t.required);
        firstInvalid = firstInvalid || emailInput;
      } else if (!isValidEmail(emailVal)) {
        setFieldError(emailInput, t.email);
        firstInvalid = firstInvalid || emailInput;
      }

      var msg = String(messageInput.value || '').trim();
      if (!msg) {
        setFieldError(messageInput, t.required);
        firstInvalid = firstInvalid || messageInput;
      } else if (msg.length < 10) {
        setFieldError(messageInput, t.messageShort);
        firstInvalid = firstInvalid || messageInput;
      }

      if (firstInvalid) {
        firstInvalid.focus();
        return false;
      }
      return true;
    }

    function showSuccess() {
      panel.innerHTML =
        eyebrowHtml +
        '<div class="contact-success">' +
          '<div class="contact-success__icon" aria-hidden="true"><i class="bi bi-check-lg"></i></div>' +
          '<h2 class="contact-success__title" tabindex="-1">' + escapeHtml(t.successTitle) + '</h2>' +
          '<p class="contact-success__body">' + escapeHtml(t.successBody) + '</p>' +
          '<button type="button" class="btn btn--secondary" id="contact-reset">' + escapeHtml(t.again) + '</button>' +
        '</div>';

      var heading = panel.querySelector('.contact-success__title');
      if (heading) heading.focus();

      var resetBtn = document.getElementById('contact-reset');
      if (resetBtn) {
        resetBtn.addEventListener('click', restoreForm);
      }
    }

    function wireForm(currentForm) {
      currentForm.addEventListener('submit', onSubmit);
      currentForm.querySelectorAll('.contact-topic').forEach(function (btn) {
        btn.addEventListener('click', function () {
          selectTopic(btn);
        });
      });
    }

    function restoreForm() {
      panel.innerHTML = eyebrowHtml + titleHtml + formMarkup;
      form = document.getElementById('contact-form');
      nameInput = document.getElementById('contact-name');
      emailInput = document.getElementById('contact-email');
      messageInput = document.getElementById('contact-message');
      konuInput = document.getElementById('contact-konu');
      subjectInput = document.getElementById('contact-subject');
      gotchaInput = document.getElementById('contact-gotcha');
      submitBtn = document.getElementById('contact-submit');
      formError = document.getElementById('contact-form-error');
      topics = panel.querySelectorAll('.contact-topic');
      wireForm(form);
      applyTopicFromQuery();
      if (nameInput) nameInput.focus();
    }

    function onSubmit(e) {
      e.preventDefault();
      if (gotchaInput && String(gotchaInput.value || '').trim()) {
        return;
      }
      if (!validate()) return;

      var pressed = form.querySelector('.contact-topic[aria-pressed="true"]');
      if (pressed) selectTopic(pressed);

      var action = form.getAttribute('action');
      var data = new FormData(form);
      submitBtn.disabled = true;
      submitBtn.textContent = t.sending;
      clearFormError();

      fetch(action, {
        method: 'POST',
        body: data,
        headers: { Accept: 'application/json' }
      })
        .then(function (res) {
          return res.json().catch(function () { return {}; }).then(function (payload) {
            return { ok: res.ok, payload: payload };
          });
        })
        .then(function (result) {
          if (result.ok) {
            showSuccess();
            return;
          }
          var msg = t.genericError;
          var errors = result.payload && result.payload.errors;
          if (errors) {
            if (Array.isArray(errors)) {
              msg = errors.map(function (item) {
                return typeof item === 'string' ? item : (item.message || JSON.stringify(item));
              }).join(' ');
            } else if (typeof errors === 'object') {
              msg = Object.keys(errors).map(function (key) {
                var val = errors[key];
                return Array.isArray(val) ? val.join(' ') : String(val);
              }).join(' ');
            } else if (typeof errors === 'string') {
              msg = errors;
            }
          } else if (result.payload && result.payload.error) {
            msg = String(result.payload.error);
          }
          setFormError(msg || t.genericError);
          submitBtn.disabled = false;
          submitBtn.textContent = t.send;
        })
        .catch(function () {
          setFormError(t.genericError);
          submitBtn.disabled = false;
          submitBtn.textContent = t.send;
        });
    }

    topics.forEach(function (btn) {
      btn.addEventListener('click', function () {
        selectTopic(btn);
      });
    });

    applyTopicFromQuery();
    form.addEventListener('submit', onSubmit);

    if (copyBtn) {
      copyBtn.addEventListener('click', function () {
        var email = copyBtn.getAttribute('data-email') || 'trkuraf@gmail.com';
        copyToClipboard(email, copyBtn, t.copied, emailDisplay);
      });
    }
  }
})(window);
