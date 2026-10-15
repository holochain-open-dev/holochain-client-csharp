"""Prints each uncovered source line of the HoloNET client (merged reports), optionally only for methods whose name
contains a filter. Usage: python scripts/uncovered_lines.py [method-substring] [file-substring]"""
import glob, sys, os, collections, xml.etree.ElementTree as ET

method_filter = sys.argv[1] if len(sys.argv) > 1 else ''
file_filter = sys.argv[2] if len(sys.argv) > 2 else ''
hits = collections.defaultdict(int)
owner = {}
for f in glob.glob('.holochain-test/coverage-*/*/coverage.cobertura.xml'):
    for pkg in ET.parse(f).getroot().iter('package'):
        if pkg.get('name') != 'NextGenSoftware.Holochain.HoloNET.Client':
            continue
        for c in pkg.iter('class'):
            for m in c.iter('method'):
                for l in m.iter('line'):
                    k = (c.get('filename'), int(l.get('number')))
                    hits[k] += int(l.get('hits'))
                    owner[k] = c.get('name').split('.')[-1] + '::' + m.get('name')
sources = {}
for (fn, ln), h in sorted(hits.items()):
    if h or method_filter not in owner[(fn, ln)] or file_filter not in fn:
        continue
    if fn not in sources:
        path = fn if os.path.exists(fn) else os.path.join('..', fn)  # report paths are relative to C:\Source
        sources[fn] = open(path, encoding='utf-8-sig').read().split('\n')
    print(f"{os.path.basename(fn)[:40]}:{ln} [{owner[(fn, ln)][:45]}] {sources[fn][ln - 1].strip()[:100]}")
