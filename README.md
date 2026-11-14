# Aim Trainer Platform

## ¿Qué es Aim Trainer Platform?

Proyecto desarrollado como Trabajo de Fin de Grado de Ingeniería de Computadores.

El objetivo del proyecto es desarrollar de forma iterativa e incremental una plataforma de Aim Trainer formada por un videojuego desarrollado en Unity y un conjunto de herramientas para registrar, analizar y visualizar los datos generados durante las partidas.

El proyecto incorpora además prácticas y herramientas DevOps para organizar y automatizar progresivamente el proceso de desarrollo, integración, pruebas y documentación.

## Estado actual

El proyecto se encuentra actualmente en desarrollo.

La Iteración 1 incorpora la captura y exportación de datos reales de las
sesiones del Aim Trainer, junto con su validación automática mediante un
pipeline de integración continua.

El componente Unity permite actualmente:

- ejecutar partidas configurables;
- registrar información de sesión;
- registrar targets, hits, timeouts y tiempos de reacción;
- registrar clics acertados y fallidos;
- exportar los datos de cada sesión en formato JSON.

El proyecto dispone además de:

- un contrato definido para los datos exportados;
- validación estructural mediante JSON Schema;
- validaciones adicionales de coherencia mediante Python;
- ejemplos reales de sesiones;
- Jenkins configurado como Multibranch Pipeline;
- ejecución automática mediante webhooks de GitHub;
- validación automática de los JSON dentro del pipeline.

## Tecnologías

Las principales tecnologías previstas o utilizadas en el proyecto son:

- **Unity 2020.3.33f1** — desarrollo del Aim Trainer.
- **C#** — lógica del videojuego.
- **Python** — validación de datos, análisis y futuro backend.
- **Jenkins** — integración continua y automatización.
- **Git / GitHub** — control de versiones y gestión del desarrollo.
- **Docker / Docker Desktop** — ejecución reproducible del entorno Jenkins y futura contenerización de servicios.

Las tecnologías correspondientes al backend, frontend y persistencia se concretarán durante las iteraciones en las que se desarrollen dichos componentes.

## Estructura del repositorio

La estructura principal del repositorio es:

```text
Aim_trainer_platform/
│
├── DevOps/
│   └── json-validation/
│       ├── samples/
│       ├── session.schema.json
│       ├── validate_json.py
│       └── requirements.txt
│
├── docs/
│   ├── images/
│   ├── devops_jenkins.md
│   ├── unity.md
│   └── web_app.md
│
├── unity_game/
│   └── AimTrainer/
│       ├── Assets/
│       ├── Packages/
│       └── ProjectSettings/
│
├── web_app/
│
├── Dockerfile
├── Jenkinsfile
├── .gitignore
└── README.md
```
### `docs/`

Contiene distintos archivos .md en los que se explican las funcionalidades, avances o estado actual de cada una de las partes del proyecto en el momento actual.

También tiene una carpeta de imágenes relacionadas con la documentación, como esquemas o capturas importantes para la explicación.

### `unity_game/AimTrainer`

Contiene el proyecto del Aim Trainer desarrollado con Unity.

Los directorios importantes que deben mantenerse bajo control de versiones son:

- `Assets/` — escenas, scripts y recursos del juego.
- `Packages/` — configuración de paquetes utilizados por Unity.
- `ProjectSettings/` — configuración compartida del proyecto.

Directorios generados localmente por Unity como `Library`, `Temp`, `Logs`, `Build`, `Builds` o `UserSettings` no forman parte del repositorio.

La estructura irá creciendo conforme se incorporen el backend, frontend, scripts de análisis y configuración DevOps.

## Estrategia de ramas

El proyecto utiliza un flujo de trabajo basado en ramas.

### `main`

Contiene las versiones estables y validadas de la plataforma.

Las versiones globales del proyecto se generan a partir de estados estables integrados en esta rama.

### `develop`

Es la rama principal de integración durante el desarrollo.

Las nuevas funcionalidades y correcciones se integran en `develop` mediante Pull Requests después de ser probadas.

### Ramas de trabajo

Para cada tarea se crea una rama específica a partir de `develop`.

Ejemplos:

```text
feature/main-menu
feature/session-data
fix/target-spawn
docs/update-readme
```

De forma general se utilizan:

- `feature/...` para nuevas funcionalidades;
- `fix/...` para correcciones;
- `docs/...` para cambios exclusivamente documentales;
- `hotfix/...` para correcciones críticas sobre una versión estable cuando sea necesario.

Una vez terminado y probado el trabajo, se realiza un Pull Request hacia `develop`.

## Convención de commits

Se utilizan prefijos que permitan identificar rápidamente el propósito de cada cambio:

```text
feat: nueva funcionalidad
fix: corrección de errores
docs: documentación
test: pruebas
refactor: cambios internos sin modificar comportamiento
ci: cambios relacionados con integración continua
chore: tareas de mantenimiento
```

Ejemplo:

```text
feat: add initial main menu navigation
```

## Ejecutar el proyecto

### Requisitos

Para abrir el proyecto se recomienda utilizar:

```text
Unity 2020.3.33f1
```

Utilizar otra versión de Unity puede provocar modificaciones automáticas de archivos del proyecto o problemas de compatibilidad.

### Abrir el proyecto

1. Clonar el repositorio:

```bash
git clone https://github.com/sermg15/Aim_trainer_platform.git
```

2. Abrir **Unity Hub**.

3. Seleccionar **Open / Add project from disk**.

4. Seleccionar el directorio:

```text
unity_game/AimTrainer
```

5. Abrir el proyecto utilizando Unity `2020.3.33f1`.

6. Esperar a que Unity importe y prepare los recursos necesarios.

## Probar el Aim Trainer

Una vez abierto el proyecto en Unity:

1. Abrir la escena inicial incluida en la configuración de Build Settings.
2. Pulsar **Play**.
3. Utilizar el menú inicial para acceder al juego.
4. Iniciar una partida.
5. Completar el ciclo de juego hasta que finalice la sesión.
6. Es posible volver a iniciar una nueva partida al terminar.

Al finalizar una sesión, el juego registra los datos de la partida y genera
un archivo JSON con la información de sesión, targets y clics.

Los archivos seleccionados como casos de referencia pueden validarse mediante
las herramientas disponibles en `DevOps/json-validation/`.

## Versionado

El proyecto distingue entre dos niveles de versionado.

### Versiones de componentes

Los diferentes componentes evolucionan de forma independiente.

Ejemplos:

```text
unity-v0.1.0
unity-v0.2.0
pipeline-v0.1.0
backend-v0.1.0
frontend-v0.1.0
```

Los tags de componente se crean sobre el commit de `develop` en el que
dicho componente queda integrado y validado.

Los tags globales se crean sobre estados estables de `main` y representan
una versión integrada de toda la plataforma.

### Versiones globales

Las versiones globales representan un estado integrado de toda la plataforma.

Durante el desarrollo se utilizan versiones `v0.x.0`, mientras que `v1.0.0` representará la primera versión completa del sistema dentro del alcance del TFG.

## Metodología de desarrollo

El proyecto sigue una metodología **iterativa e incremental**.

Cada iteración introduce nuevas funcionalidades o mejoras sobre el estado anterior del sistema. Paralelamente se incorporan progresivamente prácticas DevOps y comprobaciones automáticas al proceso de integración.

El objetivo es que cada incremento pueda ser probado y documentado antes de continuar con el siguiente.

## Documentación técnica

La documentación específica de cada componente se encuentra en la carpeta docs/.

Actualmente disponible:

- [Unity](docs/unity.md)
- [DevOps y Jenkins](docs/devops_jenkins.md)
- [Aplicación web](docs/web_app.md)

## Autor

Proyecto desarrollado por Sergio Martínez González como Trabajo de Fin de Grado de Ingeniería de Computadores.