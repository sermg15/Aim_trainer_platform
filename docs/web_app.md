# Aplicación web

## Objetivo

La aplicación web dentro de este proyecto se usará como herramienta de visualización de datos y del análisis de estos por parte del usuario. 

Servirá como intermediario entre el usuario y la API de análisis que se creará para analizar la forma de jugar del usuario en cuestión. En ella aparecerán los análisis de las partidas; el estudio del progreso durante el histórico de estas; los puntos a mejorar; recomendaciones y más. 

## Estado actual

Actualmente la aplicación es una demo prefuncional que utiliza datos de prueba y no está conectada a un backend real.

Su objetivo en esta fase es representar visualmente cómo podría presentarse al usuario el análisis de las sesiones y validar la idea general de la futura aplicación web.

## Funcionalidades de la demo

- Carga o uso de sesiones de ejemplo.
- Visualización de métricas principales.
- Representación de aciertos y errores.
- Tiempos de reacción.
- Posiciones/clics.
- Gráficas disponibles.
- Comparación de partidas, si ya existe en el prototipo.

## Estructura del componente

El prototipo se encuentra en `web_app/` y utiliza HTML, CSS y JavaScript sin frameworks ni proceso de compilación.

- `index.html`: estructura principal de la interfaz.
- `styles.css`: estilos y diseño responsive.
- `app.js`: carga, normalización, análisis y visualización de los datos.
- `aim-trainer-example.json`: conjunto de datos de ejemplo utilizado por la demo.
- `aim-trainer-template.csv`: ejemplo del formato CSV admitido.

## Ejecución local

`python -m http.server [puerto]`

## Flujo actual de datos

Actualmente:

Datos dummy / archivos de ejemplo
        ↓
Aplicación web
        ↓
Métricas y visualizaciones

## Arquitectura prevista

En versiones posteriores:

Unity
  ↓
Backend / API
  ↓
Análisis
  ↓
Aplicación web

Posteriormente:

Unity
  ↓
Backend
  ↓
Base de datos
  ↓
Aplicación web

## Pruebas realizadas

- La aplicación arranca correctamente.
- La navegación funciona.
- Se carga una sesión de ejemplo.
- Las métricas se muestran.
- Las gráficas se renderizan.
- Se han comprobado estados de error o ausencia de datos si existen.

## Limitaciones actuales

- Utiliza datos dummy.
- No existe conexión real con el backend.
- No existe persistencia.
- Las métricas mostradas todavía no forman parte del flujo final de la plataforma.

## Evolución prevista

### Web demo / Iteración 0

Prototipo visual.

Funcionalidad básica a mostrar como idea principal de lo que se desarrollará en el futuro. No establece la forma de trabajar ni la arquitectura final, es una simple demo. 

### Frontend v0.1.0 / Iteración 4

Conexión con backend y sesiones reales.

Se podrán analizar los distintos archivos generados en las partidas subiéndolos directamente a la web.

### Versiones posteriores

Histórico, comparación y visualizaciones de evolución.

Se generará un histórico de partidas guardando los datos de todas las que se han mandado a analizar. De esta forma se podrán hacer comparativas y estudiar la evolución y el progreso del juador. 