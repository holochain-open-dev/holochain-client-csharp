'use strict';

// ── Copy-to-clipboard ──────────────────────────────────────
document.querySelectorAll('.copy-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    const pre = btn.closest('.code-block').querySelector('pre');
    navigator.clipboard.writeText(pre.innerText).then(() => {
      btn.textContent = 'Copied!';
      btn.classList.add('copied');
      setTimeout(() => { btn.textContent = 'Copy'; btn.classList.remove('copied'); }, 2000);
    });
  });
});

// ── Scroll-spy: highlight rail links ───────────────────────
const railLinks = document.querySelectorAll('.rail-nav a');
if (railLinks.length) {
  const headings = Array.from(document.querySelectorAll('h2[id], h3[id]'));
  const observer = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        railLinks.forEach(l => l.classList.remove('active'));
        const match = document.querySelector(`.rail-nav a[href="#${e.target.id}"]`);
        if (match) match.classList.add('active');
      }
    });
  }, { rootMargin: '-10% 0px -80% 0px' });
  headings.forEach(h => observer.observe(h));
}

// ── Slide-out menu (animated hamburger at every width) ───────
const menuBtn = document.querySelector('.mobile-menu-btn');
const sidebar = document.querySelector('.sidebar');
if (menuBtn && sidebar) {
  menuBtn.innerHTML = '<span></span><span></span><span></span>';
  menuBtn.setAttribute('aria-label', 'Menu');
  const setOpen = o => { sidebar.classList.toggle('open', o); menuBtn.classList.toggle('open', o); };
  menuBtn.addEventListener('click', () => setOpen(!sidebar.classList.contains('open')));
  document.addEventListener('click', e => {
    if (sidebar.classList.contains('open') && !sidebar.contains(e.target) && !menuBtn.contains(e.target)) setOpen(false);
  });
  sidebar.addEventListener('click', e => { if (e.target.closest('a')) setOpen(false); });
  document.addEventListener('keydown', e => { if (e.key === 'Escape') setOpen(false); });
}

// ── Mark current nav item active ───────────────────────────
const currentPath = window.location.pathname.replace(/\/$/, '') || '/';
document.querySelectorAll('.nav-item').forEach(link => {
  const href = link.getAttribute('href');
  if (!href) return;
  const linkPath = href.replace(/\/$/, '') || '/';
  if (currentPath === linkPath || currentPath.endsWith(linkPath))
    link.classList.add('active');
});

// ── Sidebar search (client-side filter) ────────────────────
const searchInput = document.querySelector('.sidebar-search input');
if (searchInput) {
  searchInput.addEventListener('input', () => {
    const q = searchInput.value.toLowerCase();
    document.querySelectorAll('.nav-item').forEach(item => {
      item.style.display = (!q || item.textContent.toLowerCase().includes(q)) ? '' : 'none';
    });
    document.querySelectorAll('.nav-section-title').forEach(title => {
      const next = title.nextElementSibling;
      title.style.display = (next && next.style.display !== 'none') ? '' : 'none';
    });
  });
}
