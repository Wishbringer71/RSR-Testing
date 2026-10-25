#!/usr/bin/env python3
"""Ask the C# language server about a symbol: who refers to it, and what it is.

Claude Code starts the language servers of its code intelligence plugins only in terminal sessions; in a cloud session
nothing answers (code.claude.com, "Code intelligence plugins", read 25.10.2026). This runs csharp-ls - the server
behind the official csharp-lsp plugin - for one question and stops it. A text search finds every line that spells a
name; the server finds the references to one symbol across all projects, an overload or a same-named member of
another type left out, and shows the documentation of a Dalamud, ECommons or FFXIVClientStructs member from its
package (A362).

Needs csharp-ls (installed here with `dotnet tool install --global csharp-ls` when missing) and a Debug build of
RotationSolver.SourceGenerators: without it the server holds an unresolved analyzer reference, and every reference
search fails inside Roslyn (measured 25.10.2026). Loading the solution takes one to three minutes.

Usage:
  python3 .github/scripts/lsp_query.py refs  DataCenter.AnyLivingHealer
  python3 .github/scripts/lsp_query.py hover IGameObject.IsDead
  python3 .github/scripts/lsp_query.py --self-test
A name is matched against the server's symbol search; with "Container.Member" the container must match too.
"""
import json
import os
import queue
import shutil
import subprocess
import sys
import threading
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
GENERATOR = ROOT / 'RotationSolver.SourceGenerators'
DOTNET = Path.home() / '.dotnet'
LOAD_TIMEOUT = 420


def environment():
    env = dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT='1')
    if DOTNET.exists():
        env['DOTNET_ROOT'] = str(DOTNET)
        env['PATH'] = os.pathsep.join([str(DOTNET / 'tools'), str(DOTNET), env.get('PATH', '')])
    return env


def prepare(env):
    if not shutil.which('csharp-ls', path=env['PATH']):
        subprocess.run(['dotnet', 'tool', 'install', '--global', 'csharp-ls'], env=env, check=True,
                       stdout=subprocess.DEVNULL)
    if not list((GENERATOR / 'bin' / 'Debug').glob('*/RotationSolver.SourceGenerators.dll')):
        subprocess.run(['dotnet', 'build', '--no-restore', '-c', 'Debug', str(GENERATOR / 'RotationSolver.SourceGenerators.csproj'),
                        '-p:EnableWindowsTargeting=true'], env=env, check=True, stdout=subprocess.DEVNULL)


class Server:
    def __init__(self, env):
        self.process = subprocess.Popen(['csharp-ls', '--solution', 'RotationSolver.sln'], cwd=ROOT, env=env,
                                        stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
        self.messages = queue.Queue()
        self.next_id = 0
        threading.Thread(target=self._read, daemon=True).start()

    def _read(self):
        stream = self.process.stdout
        while True:
            line = stream.readline()
            if not line:
                return
            if line.lower().startswith(b'content-length'):
                length = int(line.split(b':')[1])
                while stream.readline().strip():
                    pass
                self.messages.put(json.loads(stream.read(length)))

    def _send(self, message):
        body = json.dumps(message).encode()
        self.process.stdin.write(b'Content-Length: %d\r\n\r\n' % len(body) + body)
        self.process.stdin.flush()

    def notify(self, method, params):
        self._send({'jsonrpc': '2.0', 'method': method, 'params': params})

    def request(self, method, params, timeout=240):
        self.next_id += 1
        own = self.next_id
        self._send({'jsonrpc': '2.0', 'id': own, 'method': method, 'params': params})
        end = time.time() + timeout
        while time.time() < end:
            try:
                message = self.messages.get(timeout=5)
            except queue.Empty:
                continue
            if 'method' in message and 'id' in message:  # a request from the server: answer it empty
                self._send({'jsonrpc': '2.0', 'id': message['id'], 'result': None})
            elif message.get('id') == own:
                return message.get('result')
        return None

    def close(self):
        self.process.kill()


def start():
    env = environment()
    prepare(env)
    server = Server(env)
    uri = ROOT.as_uri()
    server.request('initialize', {'processId': os.getpid(), 'rootUri': uri, 'workspaceFolders': [{'uri': uri, 'name': 'rsr'}],
                                  'capabilities': {'textDocument': {'hover': {'contentFormat': ['plaintext', 'markdown']}}}})
    server.notify('initialized', {})
    return server


def locate(server, name):
    """The symbol search's hits for `name`, waiting while the solution loads."""
    container, _, member = name.rpartition('.')
    end = time.time() + LOAD_TIMEOUT
    while time.time() < end:
        hits = server.request('workspace/symbol', {'query': member}) or []
        exact = [h for h in hits if h['name'].split('(')[0].split()[-1].split('.')[-1] == member
                 and (not container or f'{container}.{member}' in h['name'] or h.get('containerName', '').endswith(container))]
        if exact:
            return exact
        time.sleep(10)
    return []


def relative(location):
    path = Path(location['uri'].removeprefix('file://'))
    try:
        path = path.relative_to(ROOT)
    except ValueError:
        pass
    return f"{path}:{location['range']['start']['line'] + 1}"


def references(server, name):
    found = []
    for hit in locate(server, name):
        location = hit['location']
        server.notify('textDocument/didOpen', {'textDocument': {'uri': location['uri'], 'languageId': 'csharp', 'version': 1,
                                                                'text': Path(location['uri'].removeprefix('file://')).read_text(encoding='utf-8-sig')}})
        position = location['range']['start']
        line = Path(location['uri'].removeprefix('file://')).read_text(encoding='utf-8-sig').split('\n')[position['line']]
        column = line.find(name.rpartition('.')[2], position['character'])
        position = {'line': position['line'], 'character': column if column >= 0 else position['character']}
        for attempt in range(4):
            result = server.request('textDocument/references', {'textDocument': {'uri': location['uri']}, 'position': position,
                                                                'context': {'includeDeclaration': False}})
            if result:
                found.extend(relative(r) for r in result)
                break
            time.sleep(15)
    return sorted(set(found))


def hover(server, name):
    texts = []
    for hit in locate(server, name):
        location = hit['location']
        if not location['uri'].startswith('file://'):
            continue
        text = Path(location['uri'].removeprefix('file://')).read_text(encoding='utf-8-sig')
        server.notify('textDocument/didOpen', {'textDocument': {'uri': location['uri'], 'languageId': 'csharp', 'version': 1, 'text': text}})
        line = text.split('\n')[location['range']['start']['line']]
        column = line.find(name.rpartition('.')[2])
        result = server.request('textDocument/hover', {'textDocument': {'uri': location['uri']},
                                                       'position': {'line': location['range']['start']['line'], 'character': max(0, column)}})
        if result:
            contents = result['contents']
            texts.append(contents['value'] if isinstance(contents, dict) else str(contents))
    return texts


def self_test():
    server = start()
    try:
        found = references(server, 'DataCenter.AnyLivingHealer')
    finally:
        server.close()
    projects = {f.split('/')[0] for f in found}
    assert len(found) >= 3 and len(projects) >= 2, f'references to DataCenter.AnyLivingHealer across projects: {found}'
    print(f'self-test ok: {len(found)} references to DataCenter.AnyLivingHealer in {len(projects)} projects')


def main():
    if sys.argv[1:] == ['--self-test']:
        self_test()
        return
    if len(sys.argv) != 3 or sys.argv[1] not in ('refs', 'hover'):
        sys.exit(__doc__)
    server = start()
    try:
        lines = references(server, sys.argv[2]) if sys.argv[1] == 'refs' else hover(server, sys.argv[2])
    finally:
        server.close()
    print('\n'.join(lines) if lines else f'nothing found for {sys.argv[2]}')


if __name__ == '__main__':
    main()
