import argparse
import json
import re
import subprocess

from datetime import datetime, timezone
from pathlib import Path

CONTEXT_VERSION = "1.0"
MAX_DIFF_LENGTH = 30000

TASK_PATTERN = re.compile(r"\bTFG-\d+\b", re.IGNORECASE)

STATUS_MAP = {
    "A": "added",
    "M": "modified",
    "D": "deleted",
    "R": "renamed",
}

def run_git(*args):
    """Ejecuta un comando Git y devuelve su salida."""
    result = subprocess.run(
        ["git", *args],
        capture_output=True,
        text=True,
        check=True
    )
    return result.stdout.strip()


def get_commits(base, head):
    """Obtiene los commits entre las dos referencias."""
    output = run_git(
        "log",
        "--reverse",
        "--format=%H%x00%B%x00",
        f"{base}..{head}"
    )

    parts = output.split("\x00")
    commits = []

    for i in range(0, len(parts) - 1, 2):
        sha = parts[i].strip()
        message = parts[i + 1].strip()

        if sha:
            commits.append({
                "sha": sha,
                "message": message
            })

    return commits


def get_changed_files(base, head):
    """Obtiene los archivos modificados y su estado."""
    output = run_git(
        "diff",
        "--name-status",
        "--no-renames",
        base,
        head
    )

    files = []

    for line in output.splitlines():
        status, path = line.split("\t", 1)

        files.append({
            "path": path,
            "status": STATUS_MAP[status[0]]
        })

    return files

def get_diff(base, head):
    """Obtiene el diff filtrando archivos generados."""
    content = run_git(
        "diff",
        "--no-ext-diff",
        "--no-renames",
        base,
        head,
        "--",
        ".",
        ":(exclude)**/Library/**",
        ":(exclude)**/Temp/**",
        ":(exclude)**/obj/**",
        ":(exclude)**/Logs/**",
        ":(exclude)**/*.meta"
    )

    truncated = len(content) > MAX_DIFF_LENGTH

    if truncated:
        content = content[:MAX_DIFF_LENGTH]

    return {
        "summary": "",
        "content": content,
        "truncated": truncated
    }

def extract_task_ids(commits):
    """Extrae identificadores TFG-XX de los commits."""
    messages = "\n".join(
        commit["message"] for commit in commits
    )

    task_ids = TASK_PATTERN.findall(messages)

    return sorted(set(task.upper() for task in task_ids))


def generate_context(base_ref, head_ref):
    base = run_git("merge-base", base_ref, head_ref)
    head = run_git("rev-parse", head_ref)

    commits = get_commits(base, head)
    changed_files = get_changed_files(base, head)
    diff = get_diff(base, head)

    branch = run_git("branch", "--show-current")
    repository = Path(run_git("rev-parse", "--show-toplevel"))

    return {
        "contextVersion": CONTEXT_VERSION,
        "eventType": "commit",
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "taskIds": extract_task_ids(commits),
        "repository": {
            "name": repository.name,
            "sourceBranch": branch or None,
            "targetBranch": None,
            "headCommit": head
        },
        "pullRequest": None,
        "commits": commits,
        "changedFiles": changed_files,
        "diff": diff,
        "ci": None
    }


def main():
    parser = argparse.ArgumentParser(
        description="Generate documentation context from Git."
    )

    parser.add_argument("--base", required=True)
    parser.add_argument("--head", default="HEAD")
    parser.add_argument(
        "--output",
        default="DevOps/documentation/output/context.json"
    )

    args = parser.parse_args()

    context = generate_context(args.base, args.head)

    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    with output_path.open("w", encoding="utf-8") as file:
        json.dump(context, file, indent=2, ensure_ascii=False)

    print(f"Context generated: {output_path}")
    print(f"Commits: {len(context['commits'])}")
    print(f"Changed files: {len(context['changedFiles'])}")
    print(f"Tasks: {context['taskIds']}")


if __name__ == "__main__":
    main()