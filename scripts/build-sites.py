"""Builds the HoloNET documentation sites from the Markdown in this repo (the single source of truth):

  docs.holonet.oasisomniverse.one   <- docs/*.md (guides), docs/api/*.md (API reference), CHANGELOG.md, INTEGRATION_TESTING.md,
                                       examples/README.md
  learn.holonet.oasisomniverse.one  <- docs/learn/*.md (tutorials)

Usage:  python scripts/build-sites.py [--docs ../HoloNETDocsWebsite] [--learn ../HoloNETLearnWebsite]
Needs:  pip install markdown
The sites are static HTML in the OASIS docs design (assets/ copied from the OASIS docs site), deployed by Vercel from
their own repos (NextGenSoftwareUK/HoloNETDocsWebsite, NextGenSoftwareUK/HoloNETLearnWebsite).
"""
import argparse, html, io, json, os, re, shutil
import markdown

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GITHUB = 'https://github.com/holochain-open-dev/holochain-client-csharp/blob/main/'
GITHUB_TREE = 'https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/'
DOCS_URL = 'https://docs.holonet.oasisomniverse.one'
LEARN_URL = 'https://learn.holonet.oasisomniverse.one'
SANDBOX_URL = 'https://sandbox.holonet.oasisomniverse.one'

# (slug, source, title, section, badge) - slug '' is the home page.
DOCS_PAGES = [
    ('', 'docs/README.md', 'HoloNET Documentation', 'Overview', 'Guide'),
    ('getting-started', 'docs/getting-started.md', 'Getting started', 'Overview', 'Guide'),
    ('templates', 'docs/templates.md', 'Project templates', 'Overview', 'Guide'),
    ('packages', 'docs/packages.md', 'Packages', 'Overview', 'Guide'),
    ('conductor', 'docs/conductor.md', 'Where the conductor comes from', 'Overview', 'Guide'),
    ('examples', 'examples/README.md', 'Examples', 'Overview', 'Examples'),
    ('client-guide', 'docs/client-guide.md', 'HoloNET Client', 'Guides', 'Guide'),
    ('orm-guide', 'docs/orm-guide.md', 'HoloNET ORM', 'Guides', 'Guide'),
    ('api-guide', 'docs/api-guide.md', 'HoloNET API', 'Guides', 'Guide'),
    ('hypernet-guide', 'docs/hypernet-guide.md', 'HoloNET HyperNET', 'Guides', 'Guide'),
    ('manager-guide', 'docs/manager-guide.md', 'HoloNET Manager', 'Guides', 'Guide'),
    ('star-happ-generation', 'docs/star-happ-generation.md', 'Generating hApps (WEB5 STAR)', 'Guides', 'Guide'),
    ('reference/client', 'docs/api/Client.md', 'Client API reference', 'API reference', 'Reference'),
    ('reference/orm', 'docs/api/ORM.md', 'ORM API reference', 'API reference', 'Reference'),
    ('reference/api', 'docs/api/API.md', 'HoloNET API reference', 'API reference', 'Reference'),
    ('reference/hypernet', 'docs/api/HyperNET.md', 'HyperNET API reference', 'API reference', 'Reference'),
    ('testing', 'INTEGRATION_TESTING.md', 'Testing', 'Resources', 'Guide'),
    ('changelog', 'CHANGELOG.md', 'Changelog', 'Resources', 'Changelog'),
]

LEARN_PAGES = [('', None, 'Learn HoloNET', 'Start here', 'Tutorials')] + [
    ('tutorials/' + os.path.splitext(f)[0], 'docs/learn/' + f, None, 'Tutorials', 'Tutorial')
    for f in sorted(os.listdir(os.path.join(REPO, 'docs', 'learn'))) if f.endswith('.md')
]


def read(rel):
    return io.open(os.path.join(REPO, rel), encoding='utf-8-sig').read()


def slug_for_source(src_rel, pages):
    for slug, source, *_ in pages:
        if source and os.path.normpath(source) == os.path.normpath(src_rel):
            return slug
    return None


def rewrite_link(href, page_source, own_pages, own_base, other_pages, other_base):
    """Links between Markdown files become site URLs; anything else in the repo goes to GitHub."""
    if re.match(r'^(https?:|mailto:|#)', href):
        return href
    path, _, frag = href.partition('#')
    frag = ('#' + frag) if frag else ''
    target = os.path.normpath(os.path.join(os.path.dirname(page_source), path)).replace('\\', '/')
    slug = slug_for_source(target, own_pages)
    if slug is not None:
        return own_base + '/' + slug + frag if slug else (own_base + '/' + frag if frag else own_base + '/')
    slug = slug_for_source(target, other_pages)
    if slug is not None:
        return other_base + '/' + slug + frag
    full = os.path.join(REPO, target)
    return (GITHUB_TREE if os.path.isdir(full) else GITHUB) + target + frag


def render(md_text, page_source, own_pages, own_base, other_pages, other_base):
    md = markdown.Markdown(extensions=['tables', 'fenced_code', 'toc', 'sane_lists', 'attr_list'],
                           extension_configs={'toc': {'permalink': False}})
    body = md.convert(md_text)

    # The page title is the first <h1>; take it out of the body (it goes in the page header).
    m = re.search(r'<h1[^>]*>(.*?)</h1>', body, re.S)
    title = re.sub('<[^>]+>', '', m.group(1)) if m else None
    if m:
        body = body[:m.start()] + body[m.end():]

    # The first paragraph is the page description.
    p = re.search(r'<p>(.*?)</p>', body, re.S)
    desc = p.group(1).strip() if p else ''
    if p and len(re.sub('<[^>]+>', '', desc)) < 700:
        body = body[:p.start()] + body[p.end():]
    else:
        desc = ''

    body = re.sub(r'href="([^"]+)"', lambda m: 'href="' + rewrite_link(html.unescape(m.group(1)), page_source, own_pages, own_base, other_pages, other_base) + '"', body)

    # Code blocks: the OASIS docs code-block frame with a language label and a copy button.
    def code(m):
        lang = m.group(1) or 'code'
        return f'<div class="code-block"><div class="code-block-header"><span class="code-lang">{lang}</span><button class="copy-btn">Copy</button></div><pre>{m.group(2)}</pre></div>'
    body = re.sub(r'<pre><code(?: class="language-([\w#+-]+)")?>(.*?)</code></pre>', code, body, flags=re.S)

    # Blockquotes become note callouts.
    body = re.sub(r'<blockquote>\s*(.*?)\s*</blockquote>', r'<div class="callout callout-note"><div class="callout-icon">ℹ️</div><div class="callout-body">\1</div></div>', body, flags=re.S)

    # Wide tables scroll on small screens.
    body = re.sub(r'<table>', '<div class="table-wrap"><table>', body).replace('</table>', '</table></div>')

    toc = [(t['id'], t['name']) for t in md.toc_tokens for t in [t] + t.get('children', [])]
    toc = [(i, n) for i, n in toc if i] [:40]
    return title, desc, body, toc


def page_html(site, slug, title, desc, body, toc, nav, badge, section):
    depth = slug.count('/') + (1 if slug else 0)
    root = '../' * slug.count('/') if slug else ''
    assets = (root or './') + 'assets'
    nav_html = []
    for sec, items in nav:
        nav_html.append(f'      <div class="nav-section-title">{html.escape(sec)}</div>')
        for s, t, ext in items:
            href = s if ext else '/' + s
            active = ' active' if (s == slug and not ext) else ''
            target = ' target="_blank"' if ext else ''
            nav_html.append(f'      <a class="nav-item{active}" href="{href}"{target}>{html.escape(t)}</a>')
    rail = ''
    if toc:
        links = '\n'.join(f'      <a href="#{i}">{html.escape(n)}</a>' for i, n in toc)
        rail = f'''  <aside class="rail">
    <div class="rail-title">On this page</div>
    <nav class="rail-nav">
{links}
    </nav>
  </aside>
'''
    crumbs = f'<a href="/">{site["name"]}</a>' + (f'<span class="breadcrumb-sep">/</span><span>{html.escape(section)}</span>' if slug else '')
    layout_class = '' if toc else ' layout-no-rail'
    return f'''<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>{html.escape(title)} — {site["name"]}</title>
<meta name="description" content="{html.escape(re.sub('<[^>]+>', '', desc) or site["tagline"])}">
<link rel="preconnect" href="https://fonts.googleapis.com">
<link href="https://fonts.googleapis.com/css2?family=Orbitron:wght@700&family=Titillium+Web:wght@400;600;700&family=Share+Tech+Mono&display=swap" rel="stylesheet">
<link rel="stylesheet" href="{assets}/css/style.css">
<link rel="stylesheet" href="{assets}/css/holonet.css">
</head>
<body>
<!-- Generated by holochain-client-csharp/scripts/build-sites.py from the Markdown in that repo: edit it there, not here. -->
<button class="mobile-menu-btn">&#9776;</button>
<div class="layout{layout_class}">

  <aside class="sidebar">
    <a class="sidebar-logo" href="/">
      <div class="sidebar-logo-icon">H</div>
      <div class="sidebar-logo-text">
        <span class="site-name">{site["name"]}</span>
        <span class="site-sub">{site["sub"]}</span>
      </div>
    </a>
    <div class="sidebar-search"><input type="search" placeholder="Search…" aria-label="Search"></div>
    <nav class="sidebar-nav">
{chr(10).join(nav_html)}
    </nav>
    <div class="sidebar-footer">
      <a href="https://holonet.oasisomniverse.one" target="_blank">holonet.oasisomniverse.one</a>
      <a href="https://github.com/holochain-open-dev/holochain-client-csharp" target="_blank">GitHub</a>
      <a href="https://www.nuget.org/packages?q=NextGenSoftware.Holochain.HoloNET" target="_blank">NuGet</a>
      <a href="https://oasisomniverse.one" target="_blank">oasisomniverse.one</a>
    </div>
  </aside>

  <main class="main">
    <div class="page-header">
      <div class="breadcrumb">{crumbs}</div>
      <div class="page-meta"><span class="badge badge-reference">{html.escape(badge)}</span><span class="badge badge-version">HoloNET 4.x · Holochain 0.7.0 / 0.7.1</span></div>
      <h1 class="page-title">{html.escape(title)}</h1>
      {f'<p class="page-desc">{desc}</p>' if desc else ''}
    </div>
    <div class="content">
{body}
    </div>
  </main>

{rail}</div>
<script src="{assets}/js/site.js"></script>
</body>
</html>
'''


def build(site, pages, out, other_pages, other_base, nav_extra, extra_home=None):
    if not os.path.isdir(out):
        raise SystemExit(f'Output folder {out} does not exist (clone the site repo next to this one).')
    # Fresh output, keeping the repo's own files.
    keep = {'.git', 'README.md', 'LICENSE', '.gitignore'}
    for name in os.listdir(out):
        if name not in keep:
            p = os.path.join(out, name)
            shutil.rmtree(p) if os.path.isdir(p) else os.remove(p)

    shutil.copytree(os.path.join(REPO, 'scripts', 'site-assets'), os.path.join(out, 'assets'))

    rendered = []
    for slug, source, title, section, badge in pages:
        if source is None:
            continue
        t, desc, body, toc = render(read(source), source, pages, '', other_pages, other_base)
        rendered.append((slug, title or t or slug, desc, body, toc, section, badge))

    nav = []
    for slug, title, desc, body, toc, section, badge in rendered:
        if not nav or nav[-1][0] != section:
            nav.append((section, []))
        nav[-1][1].append((slug, title, False))
    nav.append(('Resources' if not any(s == 'Resources' for s, _ in nav) else 'More', nav_extra))

    if extra_home:
        slug, title, desc, body = extra_home(rendered)
        rendered.insert(0, (slug, title, desc, body, [], 'Start here', 'Tutorials'))
        nav.insert(0, ('Start here', [('', 'Home', False)]))

    vercel_rewrites = []
    for slug, title, desc, body, toc, section, badge in rendered:
        path = os.path.join(out, (slug or 'index') + '.html')
        os.makedirs(os.path.dirname(path), exist_ok=True)
        io.open(path, 'w', encoding='utf-8', newline='\n').write(page_html(site, slug, title, desc, body, toc, nav, badge, section))
    for d in sorted({os.path.dirname(s) for s, *_ in rendered if '/' in s}):
        vercel_rewrites.append({'source': f'/{d}/:slug', 'destination': f'/{d}/:slug.html'})

    io.open(os.path.join(out, 'vercel.json'), 'w', encoding='utf-8', newline='\n').write(json.dumps({
        'outputDirectory': '.',
        'cleanUrls': True,
        'trailingSlash': False,
        'rewrites': vercel_rewrites + [{'source': '/', 'destination': '/index.html'}],
        'headers': [
            {'source': '/(.*)', 'headers': [
                {'key': 'X-Content-Type-Options', 'value': 'nosniff'},
                {'key': 'X-Frame-Options', 'value': 'DENY'},
                {'key': 'Referrer-Policy', 'value': 'strict-origin-when-cross-origin'}]},
            {'source': '/assets/(.*)', 'headers': [{'key': 'Cache-Control', 'value': 'public, max-age=3600'}]}]
    }, indent=2) + '\n')
    io.open(os.path.join(out, 'package.json'), 'w', encoding='utf-8', newline='\n').write(json.dumps({
        'name': site['package'], 'version': '1.0.0', 'private': True,
        'description': f'{site["name"]} — {site["domain"]} (generated from holochain-client-csharp)',
        'scripts': {'dev': 'npx serve . -p 3003', 'build': "echo 'Static site - no build step required'"}
    }, indent=2) + '\n')
    print(f'{site["domain"]}: {len(rendered)} pages -> {out}')


def learn_home(rendered):
    cards = []
    for i, (slug, title, desc, body, toc, section, badge) in enumerate(rendered, 1):
        # Plain text only (a link inside the card's link breaks the page): the "You'll build:" sentence, and the time.
        text = html.unescape(re.sub('<[^>]+>', '', desc))
        build = re.search(r"You'll build:\s*(.*?)\s*Time:", text, re.S)
        mins = re.search(r'Time:\s*([^.]+)\.', text)
        summary = (build.group(1) if build else text)[:220]
        cards.append(f'<a class="track-card" href="/{slug}"><h3>{i}. {html.escape(title)}</h3><p>{html.escape(summary)}</p>'
                     + (f'<p class="card-time">{html.escape(mins.group(1))}</p>' if mins else '') + '</a>')
    body = f'''<p>Step-by-step tutorials for building on Holochain with .NET: each one takes you from nothing to something that works, using code that HoloNET runs against real Holochain 0.7.0 and 0.7.1 conductors on every release.</p>
<div class="track-grid">
{chr(10).join(cards)}
</div>
<h2 id="next">Next</h2>
<p>Every guide and the full API reference are at <a href="{DOCS_URL}">docs.holonet.oasisomniverse.one</a>, and the runnable examples are in <a href="{GITHUB_TREE}examples">examples/</a>.</p>'''
    return '', 'Learn HoloNET', 'Build .NET apps, sites and games on Holochain, one tutorial at a time.', body


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--docs', default=os.path.join(os.path.dirname(REPO), 'HoloNETDocsWebsite'))
    ap.add_argument('--learn', default=os.path.join(os.path.dirname(REPO), 'HoloNETLearnWebsite'))
    a = ap.parse_args()

    docs_site = {'name': 'HOLONET DOCS', 'sub': 'Guides & API reference', 'tagline': 'HoloNET documentation: guides and the full API reference for the HoloNET Client, ORM, API, HyperNET, Manager and hApp generation.', 'domain': 'docs.holonet.oasisomniverse.one', 'package': 'holonet-docs-website'}
    learn_site = {'name': 'LEARN HOLONET', 'sub': 'Tutorials', 'tagline': 'Step-by-step tutorials for building .NET apps, sites and games on Holochain with HoloNET.', 'domain': 'learn.holonet.oasisomniverse.one', 'package': 'holonet-learn-website'}

    build(docs_site, DOCS_PAGES, a.docs, LEARN_PAGES, LEARN_URL,
          [(LEARN_URL, 'Tutorials ↗', True), (SANDBOX_URL, 'Sandbox ↗', True), (GITHUB_TREE + 'examples', 'Examples on GitHub ↗', True), ('https://github.com/holochain-open-dev/holochain-client-csharp', 'GitHub ↗', True)])
    build(learn_site, LEARN_PAGES, a.learn, DOCS_PAGES, DOCS_URL,
          [(DOCS_URL, 'Docs & API reference ↗', True), (SANDBOX_URL, 'Sandbox ↗', True), (GITHUB_TREE + 'examples', 'Examples ↗', True), ('https://github.com/holochain-open-dev/holochain-client-csharp', 'GitHub ↗', True)],
          extra_home=learn_home)


if __name__ == '__main__':
    main()
