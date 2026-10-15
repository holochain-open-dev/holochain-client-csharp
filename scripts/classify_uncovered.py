"""Splits the uncovered lines of the HoloNET client into those inside catch blocks (defensive handlers) and the rest,
and prints the rest. Usage: python scripts/classify_uncovered.py"""
import glob, os, collections, xml.etree.ElementTree as ET

hits = collections.defaultdict(int)
owner = {}
for f in glob.glob('.holochain-test/coverage-*/*/coverage.cobertura.xml'):
    for pkg in ET.parse(f).getroot().iter('package'):
        if pkg.get('name') != os.environ.get('HOLONET_COV_PACKAGE', 'NextGenSoftware.Holochain.HoloNET.Client'):
            continue
        for c in pkg.iter('class'):
            for m in c.iter('method'):
                for l in m.iter('line'):
                    k = (c.get('filename'), int(l.get('number')))
                    hits[k] += int(l.get('hits'))
                    owner[k] = c.get('name').split('.')[-1] + '::' + m.get('name')

sources = {}


def src(fn):
    if fn not in sources:
        path = fn if os.path.exists(fn) else os.path.join('..', fn)
        sources[fn] = open(path, encoding='utf-8-sig').read().split('\n')
    return sources[fn]


def in_catch(fn, ln):
    """True if line ln (1-based) sits inside a catch { ... } block (or is its header)."""
    lines = src(fn)
    depth = 0
    for i in range(ln - 1, -1, -1):
        text = lines[i].strip()
        if i < ln - 1:
            depth += text.count('}') - text.count('{')
        if text.startswith('catch') and depth <= 0:
            return True
        if depth < 0 and not text.startswith('catch'):
            # we've left the enclosing block without meeting a catch
            if text.startswith('{'):
                continue
            return False
    return False


catch_lines, other = 0, []
for (fn, ln), h in sorted(hits.items()):
    if h:
        continue
    if in_catch(fn, ln):
        catch_lines += 1
    else:
        other.append((fn, ln))
print('uncovered lines in catch blocks:', catch_lines)
print('other uncovered lines:', len(other))
for fn, ln in other:
    print(f"{os.path.basename(fn)[:38]}:{ln} [{owner[(fn, ln)][:42]}] {src(fn)[ln - 1].strip()[:95]}")
