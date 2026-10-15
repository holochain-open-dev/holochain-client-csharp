"""Lists HandleError(string ...) calls inside response decode/raise/process methods. That overload can throw
(ErrorHandlingBehaviour), which skips completing the awaiting call; decoders should use HandleError(args, ...)."""
import re, glob, os
os.chdir(os.path.join(os.path.dirname(__file__), '..', 'NextGenSoftware.Holochain.HoloNET.Client', 'HoloNET Client'))
for f in glob.glob('**/*.cs', recursive=True):
    s = open(f, encoding='utf-8-sig').read()
    for m in re.finditer(r'(private|protected) [\w<>\[\], ]+ (Decode\w+|Raise\w+|Process\w+)\([^)]*\)\s*\{(.*?)\n        \}', s, re.S):
        for h in re.finditer(r'HandleError\((\$?"|msg\b|string\.Concat|message\b)', m.group(3)):
            line = s[:m.start(3) + h.start()].count('\n') + 1
            print(os.path.basename(f), m.group(2), line, s.splitlines()[line - 1].strip()[:110])
