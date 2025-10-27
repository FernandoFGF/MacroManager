# 🎮 MacroManager - CRT Screen Effect Guide

## 📺 Pantalla Curvada CRT Implementada

Se ha añadido un efecto visual de **pantalla curvada** que simula los monitores CRT antiguos, junto con un efecto de **viñetado** (oscurecimiento en los bordes) que es característico de los tubos de rayos catódicos vintage.

### ✨ Características del Efecto

**Distorsión de Barril (Barrel Distortion)**
- Simula la curvatura característica de las pantallas CRT antiguas
- El contenido se ve ligeramente "abombado" hacia afuera, como en un tubo antiguo
- Intensidad: **Moderada (8%)**

**Viñetado (Vignetting)**
- Oscurece progresivamente los bordes de la pantalla
- Crea una sensación de profundidad y autenticidad vintage
- Los bordes tienen un aspecto más oscuro, como si mirases a través de un tubo
- Intensidad: **Moderada (35%)**

### 🎯 Cómo Funciona

El efecto se implementa mediante la clase `CRTScreenOverlay`:

1. **Lightweight**: No interfiere con los controles de la aplicación
2. **Transparent**: Usa una capa transparente para no bloquear la interacción
3. **Efficient**: Cachea el bitmap del viñetado para optimizar rendimiento
4. **Configurable**: Las intensidades pueden ajustarse en código

### 🔧 Personalización del Efecto

Si deseas cambiar la intensidad del efecto, edita el archivo `MainForm.cs` en el método `SetupUI()`:

```csharp
// Busca esta sección cerca del final (línea ~240):
CRTScreenOverlay crtOverlay = new CRTScreenOverlay
{
    DistortionStrength = 0.08f,   // ← Curvatura (0.0 a 1.0)
    VignetteStrength = 0.35f      // ← Viñetado (0.0 a 1.0)
};
```

**Valores posibles:**
- `0.0` = Sin efecto
- `0.05` = Muy sutil
- `0.08` = Moderado (actual)
- `0.15` = Pronunciado
- `0.20+` = Extremo

**Ejemplo - Efecto más pronunciado:**
```csharp
CRTScreenOverlay crtOverlay = new CRTScreenOverlay
{
    DistortionStrength = 0.15f,   // Más curvatura
    VignetteStrength = 0.50f      // Más viñetado
};
```

**Ejemplo - Efecto muy sutil:**
```csharp
CRTScreenOverlay crtOverlay = new CRTScreenOverlay
{
    DistortionStrength = 0.04f,   // Casi invisible
    VignetteStrength = 0.15f      // Muy tenue
};
```

### 📊 Combinación Retro Completa

El efecto CRT se combina con los siguientes elementos retro ya implementados:

✅ **Paleta de colores verde fosforescente**
- Fondo: Verde/negro profundo (RGB 10, 30, 15)
- Texto: Verde brillante (RGB 50, 220, 50)
- Acentos: Ámbar/Amarillo (RGB 220, 150, 30)

✅ **Tipografía Courier New**
- Simula terminales y monitores antiguos
- Monoespaciada, característica de displays digitales

✅ **Bordes verdes en controles**
- Botones con bordes retro
- TreeView y controles con estilos vintage

✅ **Efecto CRT curvo con viñetado**
- Distorsión de barril (NEW)
- Oscurecimiento en bordes (NEW)

### 🎬 Resultado Visual

Al abrir la aplicación, notarás:

1. **Bordes oscuros suave**: Los bordes se desvanecen gradualmente a negro
2. **Curvatura sutil**: La pantalla tiene una leve convexidad, como un monitor antiguo
3. **Sensación de profundidad**: Da la impresión de mirar a través de un tubo CRT

### 🔌 Desactivación del Efecto (si es necesario)

Si deseas desactivar completamente el efecto, comenta esta línea en `MainForm.cs`:

```csharp
// Comenta o elimina estas líneas:
// CRTScreenOverlay crtOverlay = new CRTScreenOverlay { ... };
// this.Controls.Add(crtOverlay);
// crtOverlay.BringToFront();
```

### 📝 Notas Técnicas

- **Rendimiento**: El overlay tiene un impacto mínimo en rendimiento gracias a:
  - Caché de bitmaps
  - Renderizado eficiente
  - No interfiere con controles

- **Compatibilidad**: Funciona en todas las resoluciones de pantalla
- **Sistema**: Windows 10/11, .NET 8.0+

### 🎨 Comparación: Antes vs Después

**Antes:**
- UI moderna plana
- Colores azules estándar
- Bordes rectos
- Apariencia contemporánea

**Después:**
- UI retro inmersiva
- Colores verde fosforescente vintage
- Pantalla curvada con viñetado
- Apariencia Fallout/CRT años 80-90

---

**Tu MacroManager ahora es totalmente retro!** 🎮🟢🖥️

*Disfruta la nostalgia de los monitores CRT clásicos mientras grabas y ejecutas tus macros.*