# 📐 CONFIGURACIÓN DE INTERFAZ - GUÍA RÁPIDA

## ¿Qué es uiconfig.json?

Es un archivo de configuración que controla el tamaño y la distribución de la ventana principal de MacroManager. Puedes editarlo sin necesidad de recompilar el código.

---

## 📍 UBICACIÓN DEL ARCHIVO

```
MacroManager/uiconfig.json
```

Abra con cualquier editor de texto (Notepad, VS Code, etc.)

---

## ⚙️ PARÁMETROS CONFIGURABLES

### 1. **TAMAÑO DE VENTANA** (`window`)

```json
"window": {
  "minWidth": 1000,        // Ancho mínimo en píxeles
  "minHeight": 700,        // Alto mínimo en píxeles
  "defaultWidth": 1200,    // Ancho inicial al abrir (píxeles)
  "defaultHeight": 800     // Alto inicial al abrir (píxeles)
}
```

**Ejemplos:**
- Pantalla pequeña: `1000x700`
- Pantalla mediana: `1200x800`
- Pantalla grande: `1600x900`
- Full HD: `1920x1080`

---

### 2. **DISTRIBUCIÓN DE PANELES** (`layout`)

```json
"layout": {
  "treeViewPercentage": 25,      // % ancho panel izquierdo (árbol de macros)
  "editorPercentage": 66.66,     // % ancho panel central (editor)
  "playbackPanelHeight": 80      // Alto panel inferior (controles) en píxeles
}
```

**Explicación:**
- **treeViewPercentage**: Porcentaje del ancho que ocupa la lista de macros (izquierda)
- **editorPercentage**: Porcentaje del ancho que ocupa el editor de acciones (centro)
- **playbackPanelHeight**: Alto del panel con botones de reproducción (abajo)

**Ejemplos de distribución:**

| Config | Efecto |
|--------|--------|
| `Tree: 20, Editor: 70` | Panel árbol más pequeño |
| `Tree: 30, Editor: 60` | Panel árbol más grande |
| `Tree: 15, Editor: 80` | Editor muy expandido |

---

### 3. **TAMAÑOS MÍNIMOS** (`sizes`)

```json
"sizes": {
  "minimumTreeViewWidth": 200,   // Ancho mínimo del árbol (píxeles)
  "minimumEditorWidth": 400      // Ancho mínimo del editor (píxeles)
}
```

Evita que los paneles se hagan demasiado pequeños al redimensionar la ventana.

---

## 🎯 EJEMPLOS PRÁCTICOS

### Ejemplo 1: Pantalla pequeña (1000x700)

```json
{
  "window": {
    "minWidth": 800,
    "minHeight": 600,
    "defaultWidth": 1000,
    "defaultHeight": 700
  },
  "layout": {
    "treeViewPercentage": 20,
    "editorPercentage": 70,
    "playbackPanelHeight": 60
  },
  "sizes": {
    "minimumTreeViewWidth": 150,
    "minimumEditorWidth": 300
  }
}
```

### Ejemplo 2: Pantalla grande (1920x1080)

```json
{
  "window": {
    "minWidth": 1400,
    "minHeight": 800,
    "defaultWidth": 1920,
    "defaultHeight": 1080
  },
  "layout": {
    "treeViewPercentage": 25,
    "editorPercentage": 66.66,
    "playbackPanelHeight": 100
  },
  "sizes": {
    "minimumTreeViewWidth": 250,
    "minimumEditorWidth": 500
  }
}
```

### Ejemplo 3: Enfoque en editor (minimizar panel de macros)

```json
{
  "window": {
    "defaultWidth": 1200,
    "defaultHeight": 800
  },
  "layout": {
    "treeViewPercentage": 15,      // Árbol muy pequeño
    "editorPercentage": 80,         // Editor muy grande
    "playbackPanelHeight": 80
  }
}
```

---

## 🚀 CÓMO APLICAR LOS CAMBIOS

1. **Edita el archivo** `uiconfig.json` con tu editor favorito
2. **Guarda los cambios**
3. **Cierra** completamente MacroManager
4. **Abre** MacroManager nuevamente
5. ✅ Los cambios se aplican automáticamente

> **NOTA:** No necesitas recompilar ni reinstalar. Los cambios se cargan al iniciar la aplicación.

---

## ⚠️ NOTAS IMPORTANTES

### Validación JSON
- El archivo debe ser **JSON válido**
- Cuidado con las **comas** y **comillas**
- Si hay un error, aparecerá un mensaje y se usarán valores por defecto

### Porcentajes
- Los porcentajes son para **distribuir el ancho total**
- No necesitan sumar exactamente 100% (es solo de referencia)
- Ejemplo: Tree 25% + Editor 67% = visualmente correcto

### Píxeles
- Todos los valores de píxeles deben ser **números enteros**
- No usar decimales en píxeles (Ej: `"minWidth": 1000.5` ❌)
- Los porcentajes SÍ pueden tener decimales (Ej: `66.66` ✅)

---

## 🔧 RESOLUCIÓN DE PROBLEMAS

### "Error loading UI configuration"
Significa que hay un error en el JSON. Verifica:
- ¿Hay comillas faltantes?
- ¿Hay comas mal colocadas?
- ¿Están cerradas todas las llaves `{}`?

**Solución:** Usa un validador JSON online: https://jsonlint.com/

### Ventana aparece muy pequeña/grande
Aumenta/disminuye `defaultWidth` y `defaultHeight`

### Paneles desproporcionados
Ajusta `treeViewPercentage` y `editorPercentage`

### Botones se solapan
Aumenta `playbackPanelHeight`

---

## 📋 ARCHIVO COMPLETO POR DEFECTO

```json
{
  "window": {
    "minWidth": 1000,
    "minHeight": 700,
    "defaultWidth": 1200,
    "defaultHeight": 800
  },
  "layout": {
    "treeViewPercentage": 25,
    "editorPercentage": 66.66,
    "playbackPanelHeight": 80
  },
  "sizes": {
    "minimumTreeViewWidth": 200,
    "minimumEditorWidth": 400
  }
}
```

---

## 💡 CONSEJOS

✅ **Haz cambios pequeños** y prueba
✅ **Documenta tus cambios** con comentarios
✅ **Haz copias de seguridad** si lo personalizas mucho
✅ **Experimenta** hasta encontrar tu configuración ideal

---

**¡Listo!** Ahora puedes personalizar la interfaz sin tocar código. 🎮