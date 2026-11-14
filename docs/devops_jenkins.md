# DevOps e integración continua

Este documento describe las prácticas DevOps utilizadas en Aim Trainer Platform y su evolución durante el desarrollo del proyecto.

El objetivo no es incorporar todas las herramientas de automatización desde el inicio, sino ampliar progresivamente el proceso de integración y validación conforme evolucionan los distintos componentes de la plataforma.

## Entorno Jenkins

Jenkins se ejecuta localmente mediante un contenedor Docker sobre Docker Desktop.

La imagen utilizada por el proyecto se construye a partir de `jenkins/jenkins:lts` mediante un `Dockerfile` propio almacenado en el repositorio.

Esta imagen incorpora las herramientas necesarias para ejecutar las validaciones actuales del pipeline, incluyendo Python y las dependencias necesarias para la validación de los archivos JSON generados por Unity.

La configuración persistente de Jenkins se almacena en el volumen Docker:

```text
jenkins_home
```

montado dentro del contenedor en:

```text
/var/jenkins_home
```

De esta forma es posible recrear o actualizar el contenedor sin perder:

- jobs;
- Multibranch Pipelines;
- plugins;
- credenciales;
- configuración;
- historial de ejecuciones.

## Pipeline as Code

El pipeline se define en el archivo:

```text
Jenkinsfile
```

situado en la raíz del repositorio.

Mantener el pipeline bajo control de versiones permite:

- conservar el histórico de cambios;
- revisar modificaciones mediante Git;
- asociar la configuración de integración continua al código del proyecto;
- reproducir el proceso de validación;
- ampliar el pipeline de forma incremental.

## Iteración 0 — Pipeline mínimo inicial

Durante la Iteración 0 se incorpora una primera versión del pipeline.

El objetivo de esta versión no es realizar todavía compilaciones completas ni ejecutar automáticamente el pipeline ante cada cambio del repositorio.

Su función es comprobar que Jenkins puede obtener correctamente el repositorio y ejecutar validaciones reproducibles sobre su contenido.

### Flujo inicial

```text
Repositorio Git
      ↓
Checkout
      ↓
Validación de estructura
      ↓
Validación de track de archivos en Unity
      ↓
Información del commit
      ↓
SUCCESS / FAILURE
```

## Validaciones iniciales

### Estructura del repositorio

El pipeline comprueba la existencia de elementos necesarios para el proyecto, entre ellos:

- `README.md`;
- documentación técnica;
- `Assets`;
- `Packages`;
- `ProjectSettings`.

La ausencia de alguno de estos elementos provoca que la ejecución falle.

### Archivos generados por Unity

También se comprueba que Git no esté versionando directorios generados localmente por Unity, como:

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
```

Estos archivos no deben formar parte del repositorio y se encuentran excluidos mediante `.gitignore`.

Esta validación permite evitar que archivos generados automáticamente por Unity formen parte del control de versiones.

## Información de ejecución

Cada ejecución muestra información básica del estado analizado:

- rama;
- commit;
- mensaje del último commit;
- resultado de la validación.

Esto permite relacionar una ejecución concreta de Jenkins con un estado específico del repositorio.

## Ejecución en la Iteración 0

Durante la Iteración 0 el pipeline se ejecuta manualmente desde Jenkins.

Esta primera fase permite separar dos objetivos:

1. comprobar inicialmente que el pipeline funciona correctamente;
2. automatizar posteriormente su ejecución cuando la base de CI esté validada.

## Iteración 1 — Automatización y validación JSON

Durante la Iteración 1 el pipeline se amplía con dos funcionalidades principales:

- ejecución automática ante cambios del repositorio;
- validación automática del contrato JSON generado por Unity.

El flujo general pasa a ser:

```text
Cambio en Git
      ↓
Push a GitHub
      ↓
Webhook
      ↓
Cloudflare Tunnel
      ↓
Jenkins Multibranch Pipeline
      ↓
Jenkinsfile
      ↓
Validaciones
      ↓
SUCCESS / FAILURE
```

## Multibranch Pipeline

Jenkins utiliza un **Multibranch Pipeline** conectado al repositorio mediante GitHub Branch Source.

Esta configuración permite descubrir automáticamente las ramas del repositorio que contienen un `Jenkinsfile`.

Cada rama descubierta dispone de su propio subjob dentro del Multibranch Pipeline.

La estructura conceptual es:

```text
AimTrainer-Multibranch
│
├── main
├── develop
├── feature/...
└── ...
```

Cada subjob ejecuta el `Jenkinsfile` correspondiente a la revisión de la rama analizada.

Esto evita tener que crear y mantener manualmente un job independiente para cada rama.

## Ejecución automática mediante webhook

GitHub notifica a Jenkins los cambios realizados en el repositorio mediante un webhook.

El webhook utiliza el endpoint:

```text
/github-webhook/
```

Jenkins se ejecuta localmente, por lo que durante el desarrollo se utiliza Cloudflare Tunnel para exponer temporalmente el servicio mediante una URL HTTPS accesible desde GitHub.

El flujo es:

```text
GitHub
   ↓
Webhook
   ↓
URL HTTPS de Cloudflare Tunnel
   ↓
localhost:8080
   ↓
Jenkins
```

El túnel puede levantarse mediante:

```text
cloudflared tunnel --url http://localhost:8080
```

Al utilizar un Quick Tunnel, la URL pública puede cambiar cuando se reinicia el proceso, por lo que el webhook de GitHub debe apuntar siempre a la URL activa seguida de:

```text
/github-webhook/
```

Una vez configurado, un `push` a una rama descubierta por Jenkins provoca automáticamente una nueva ejecución del pipeline sin necesidad de iniciarla manualmente.

## Imagen Docker personalizada de Jenkins

El entorno de Jenkins se define mediante un `Dockerfile` versionado en el repositorio.

La imagen parte de:

```text
jenkins/jenkins:lts
```

y añade las herramientas necesarias para las validaciones actuales.

Actualmente incluye:

- Python;
- `pip`;
- soporte para entornos virtuales;
- dependencias Python necesarias para el validador JSON.

La imagen se construye mediante:

```text
docker build -t aimtrainer-jenkins .
```

El contenedor se ejecuta reutilizando el volumen persistente:

```text
jenkins_home:/var/jenkins_home
```

De esta forma se separan:

```text
Dockerfile
    ↓
define el entorno de ejecución

jenkins_home
    ↓
mantiene configuración y datos de Jenkins

Jenkinsfile
    ↓
define el pipeline
```

## Validación del JSON generado por Unity

La Iteración 1 incorpora un nuevo stage:

```text
Validate JSON
```

Su objetivo es comprobar que los archivos JSON generados por Unity respetan el contrato de datos definido para las sesiones del Aim Trainer.

Los elementos relacionados con esta validación se encuentran en:

```text
DevOps/
└── json-validation/
    ├── session1.schema.json
    ├── validate_json.py
    ├── requirements.txt
    └── samples/
        └── *.json
```

### JSON Schema

`session1.schema.json` define la estructura que debe cumplir una sesión válida.

Entre otras restricciones se comprueban:

- campos obligatorios;
- tipos de datos;
- valores permitidos;
- rangos numéricos;
- estructura de targets;
- estructura de clicks.

El schema utiliza JSON Schema Draft 2020-12.

### Script de validación

`validate_json.py` realiza dos tipos de comprobaciones:

```text
JSON
 ↓
Validación estructural
 ↓
JSON Schema
 ↓
Validación lógica
 ↓
Python
 ↓
PASS / FAIL
```

La validación estructural comprueba el cumplimiento del schema.

La validación lógica añade comprobaciones de coherencia entre distintos valores de la sesión, targets y clicks.

El script procesa los archivos almacenados en:

```text
DevOps/json-validation/samples/
```

Estos archivos corresponden a sesiones reales seleccionadas durante las pruebas de Unity.

### Dependencias

Las dependencias Python necesarias para ejecutar el validador se declaran en:

```text
DevOps/json-validation/requirements.txt
```

Actualmente se utiliza:

```text
jsonschema[format]
```

Las dependencias se instalan durante la construcción de la imagen Docker personalizada de Jenkins.

De esta forma no es necesario volver a instalarlas durante cada ejecución del pipeline.

## Stage de validación JSON

El `Jenkinsfile` ejecuta el script de validación mediante el entorno Python preparado en la imagen de Jenkins.

El comportamiento esperado es:

```text
validate_json.py
      ↓
Todos los JSON válidos
      ↓
exit code 0
      ↓
Jenkins SUCCESS
```

o:

```text
validate_json.py
      ↓
Alguna validación falla
      ↓
exit code != 0
      ↓
Jenkins FAILURE
```

Esto permite utilizar directamente el resultado del script como criterio de éxito o fallo del stage.

En la implementación actual, la validación JSON se ejecuta cada vez que se ejecuta el pipeline.

En futuras iteraciones podrá condicionarse a cambios relacionados con el contrato JSON, el script de validación, los archivos de prueba o el código de exportación de Unity.

## Pipeline actual

Tras las ampliaciones de la Iteración 1, el pipeline contiene actualmente las siguientes etapas principales:

```text
Checkout
      ↓
Validate repository structure
      ↓
Validate Unity repository hygiene
      ↓
Build information
      ↓
Validate JSON
      ↓
SUCCESS / FAILURE
```

Cada ejecución queda asociada a una rama y a un commit concreto del repositorio.

## Evolución prevista

### Iteración 2

Se incorporará la automatización relacionada con documentación.

### Iteración 3

Se integrarán los tests del analizador Python y del backend.

### Iteración 4

Se añadirán validaciones del frontend y del flujo integrado.

### Iteración 5

Se incorporarán pruebas relacionadas con persistencia.

### Iteración 6

Se consolidará el pipeline final con las validaciones acumuladas, builds, contenedores y las automatizaciones necesarias para la entrega final.

## Criterio de mantenimiento

Este documento se actualizará cuando se introduzcan cambios relevantes en:

- estructura del pipeline;
- validaciones;
- triggers;
- tests automáticos;
- gestión de secretos;
- Docker;
- procesos de build;
- despliegue.

No es necesario registrar aquí cada ejecución individual de Jenkins.

Las decisiones de diseño, problemas encontrados, alternativas evaluadas y evidencias de las pruebas se documentan en las tareas correspondientes del seguimiento técnico del proyecto.