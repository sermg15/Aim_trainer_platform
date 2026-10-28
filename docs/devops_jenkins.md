# DevOps e integración continua

Este documento describe las prácticas DevOps utilizadas en Aim Trainer Platform y su evolución durante el desarrollo del proyecto.

El objetivo no es incorporar todas las herramientas de automatización desde el inicio, sino ampliar progresivamente el proceso de integración y validación conforme evolucionan los distintos componentes de la plataforma.

## Entorno Jenkins

Jenkins se ejecuta localmente mediante un contenedor Docker sobre Docker Desktop.

Esta configuración permite disponer de una instancia de Jenkins reproducible y aislada del sistema principal utilizada durante el desarrollo del TFG.

En esta primera fase Jenkins se utiliza para ejecutar manualmente un Pipeline definido mediante un `Jenkinsfile` almacenado en el propio repositorio.

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

El objetivo de esta versión no es realizar todavía compilaciones completas ni ejecutar automáticamente el pipeline ante cada Pull Request.

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

La ausencia de alguno de estos elementos provoca que la ejecución falle. Se han realizado pruebas eliminando archivos y se muestra el fallo correspondiente.

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

Esta validación se incorpora debido a problemas detectados durante las primeras fases del desarrollo, en las que algunos archivos generados llegaron a ser versionados accidentalmente.

## Información de ejecución

Cada ejecución muestra información básica del estado analizado:

- rama;
- commit;
- mensaje del último commit;
- resultado de la validación.

Esto permite relacionar una ejecución concreta de Jenkins con un estado específico del repositorio.

## Ejecución en la Iteración 0

En esta primera iteración el pipeline se ejecuta manualmente desde Jenkins.

La automatización del disparo ante cambios del repositorio se incorporará en una iteración posterior.

Esto permite separar dos objetivos:

1. comprobar inicialmente que el pipeline funciona correctamente;
2. automatizar posteriormente su ejecución cuando la base de CI esté validada.

## Evolución prevista

### Iteración 1

Se ampliará el pipeline para:

- ejecutarse automáticamente ante cambios relevantes;
- validar el contrato y estructura de los datos JSON generados por Unity.

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

No es necesario registrar aquí cada ejecución individual de Jenkins. Las pruebas y resultados relevantes de cada tarea se registrarán en su correspondiente seguimiento del proyecto.