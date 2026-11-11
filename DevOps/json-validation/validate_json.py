import json
import sys
from pathlib import Path
from datetime import datetime, timezone, timedelta

from jsonschema import Draft202012Validator

BASE_DIR = Path(__file__).resolve().parent

SCHEMA_PATH = BASE_DIR / "session1.schema.json"
SAMPLES_DIR = BASE_DIR / "samples"

# Tolerancias
TIME_TOLERANCE_MS = 50.0
FUTURE_TIMESTAMP_TOLERANCE_SECONDS = 60

def load_json(path):
    with path.open("r", encoding="utf-8") as file:
        return json.load(file)

def validate_schema(session, schema):
    errors = []

    validator = Draft202012Validator(
        schema,
        format_checker=Draft202012Validator.FORMAT_CHECKER
    )

    for error in validator.iter_errors(session):
        path = ".".join(str(part) for part in error.absolute_path)

        if path:
            errors.append(f"{path}: {error.message}")
        else:
            errors.append(error.message)

    return errors

def validate_session(session):
    errors = []

    targets = session["targets"]
    max_targets = session["maxTargets"]

    # Número de targets
    if len(targets) != max_targets:
        errors.append(
            f"targets contiene {len(targets)} elementos, "
            f"pero maxTargets es {max_targets}"
        )

    # Timestamp
    timestamp = datetime.fromisoformat(
        session["timestamp"].replace("Z", "+00:00")
    )

    now = datetime.now(timezone.utc)

    if timestamp > now + timedelta(
        seconds=FUTURE_TIMESTAMP_TOLERANCE_SECONDS
    ):
        errors.append(
            f"timestamp está en el futuro: {session['timestamp']}"
        )

    return errors

def validate_targets(session):
    errors = []

    targets = session["targets"]
    lifetime_ms = session["targetLifetimeSeconds"] * 1000

    target_ids = [target["targetId"] for target in targets]

    # IDs únicos (con conjuntos set para eliminar duplicados)
    if len(target_ids) != len(set(target_ids)):
        errors.append("Existen targetId duplicados")

    for target in targets:
        target_id = target["targetId"]

        spawn_time = target["spawnTimeMs"]
        despawn_time = target["despawnTimeMs"]

        duration = despawn_time - spawn_time

        # Orden temporal
        if despawn_time < spawn_time:
            errors.append(
                f"Target {target_id}: despawnTimeMs es menor que spawnTimeMs"
            )

        # Timeout
        if target["result"] == "timeout":
            difference = abs(duration - lifetime_ms)

            if difference > TIME_TOLERANCE_MS:
                errors.append(
                    f"Target {target_id}: duración incorrecta para timeout. "
                    f"Esperado aproximadamente {lifetime_ms:.2f} ms, "
                    f"obtenido {duration:.2f} ms"
                )

        # Hit
        if target["result"] == "hit":
            reaction_time = target["reactionTimeMs"]

            if reaction_time > lifetime_ms + TIME_TOLERANCE_MS:
                errors.append(
                    f"Target {target_id}: reactionTimeMs supera "
                    f"el tiempo máximo de vida"
                )

            expected_reaction = despawn_time - spawn_time

            if abs(reaction_time - expected_reaction) > TIME_TOLERANCE_MS:
                errors.append(
                    f"Target {target_id}: reactionTimeMs no coincide "
                    f"con despawnTimeMs - spawnTimeMs"
                )

    return errors

def validate_clicks(session):
    errors = []

    targets_by_id = {
        target["targetId"]: target
        for target in session["targets"]
    }

    for index, click in enumerate(session["clicks"]):
        target_id = click["targetId"]

        # Los clicks fallidos con targetId null no necesitan
        # más comprobaciones contra un target.
        if target_id is None:
            continue

        # targetId debe existir
        if target_id not in targets_by_id:
            errors.append(
                f"Click {index}: targetId {target_id} no existe"
            )
            continue

        target = targets_by_id[target_id]

        click_time = click["timeMs"]
        spawn_time = target["spawnTimeMs"]
        despawn_time = target["despawnTimeMs"]

        # Clic dentro del tiempo de vida
        if not (
            spawn_time - TIME_TOLERANCE_MS
            <= click_time
            <= despawn_time + TIME_TOLERANCE_MS
        ):
            errors.append(
                f"Click {index}: se produjo fuera del tiempo "
                f"de vida del target {target_id}"
            )

        if click["hit"]:
            # El target correspondiente debe haber terminado en hit
            if target["result"] != "hit":
                errors.append(
                    f"Click {index}: está marcado como hit, "
                    f"pero el target {target_id} terminó como "
                    f"{target['result']}"
                )

            # El clic correcto debe coincidir aproximadamente
            # con el despawn del target.
            if (
                abs(click_time - despawn_time)
                > TIME_TOLERANCE_MS
            ):
                errors.append(
                    f"Click {index}: el tiempo del hit no coincide "
                    f"con el despawnTimeMs del target {target_id}"
                )

    return errors

def validate_file(json_path, schema):
    print(f"\nValidando: {json_path.name}")

    try:
        session = load_json(json_path)

    except json.JSONDecodeError as error:
        print(f"  [ERROR] JSON inválido: {error}")
        return False

    # Primero comprobamos la estructura.
    schema_errors = validate_schema(session, schema)

    if schema_errors:
        print("  [ERROR] Fallos de JSON Schema:")

        for error in schema_errors:
            print(f"    - {error}")

        # Si la estructura no es válida, no tiene sentido ejecutar
        # comprobaciones lógicas que esperan campos concretos.
        return False

    errors = []

    errors.extend(validate_session(session))
    errors.extend(validate_targets(session))
    errors.extend(validate_clicks(session))

    if errors:
        print("  [ERROR] Fallos de coherencia:")

        for error in errors:
            print(f"    - {error}")

        return False

    print("  [OK] JSON válido")

    return True


def main():
    schema = load_json(SCHEMA_PATH)

    # Comprobamos primero que nuestro propio schema sea válido.
    Draft202012Validator.check_schema(schema)

    json_files = sorted(SAMPLES_DIR.glob("*.json"))

    if not json_files:
        print("No se encontraron archivos JSON en samples/")
        return 1

    all_valid = True

    for json_path in json_files:
        if not validate_file(json_path, schema):
            all_valid = False

    print()

    if all_valid:
        print("Todos los JSON cumplen el contrato.")
        return 0

    print("Se han encontrado errores en uno o más JSON.")
    return 1


if __name__ == "__main__":
    sys.exit(main())