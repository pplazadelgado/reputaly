# Design System — Corporate Trust

Sistema de diseño de Reputaly. Navy `#0B2545` como chrome y acción primaria · azul `#2563EB` para CTAs secundarios · neutros slate · tipografía Manrope · cards planas con sombra mínima.

---

## Tokens (`src/styles/tokens.css`)

Importado en `main.tsx` antes que `index.css`. Todos los valores de color, tipografía, espaciado y radios son custom properties CSS reutilizables.

| Grupo | Variables clave |
|---|---|
| Navy (marca) | `--navy-900` `--navy-800` `--navy-700` `--navy-accent` |
| Azul de acción | `--blue-500` `--blue-50` |
| Texto / neutros | `--slate-900` `--slate-700` `--slate-500` `--slate-400` |
| Bordes / fondos | `--slate-200` `--slate-100` `--slate-50` `--white` |
| Semánticos | `--green-500/50` `--amber-500/50` `--red-500/50` |
| Tipografía | `--font-ui` `--fw-regular/medium/semibold/bold/extrabold` |
| Radios | `--radius-sm` `--radius` `--radius-lg` `--radius-pill` |
| Sombras | `--shadow-sm` `--shadow-lg` |
| Espaciado | `--space-1` … `--space-10` (escala 4px) |

**Regla crítica:** `--navy-accent` solo sobre fondos navy. Nunca sobre blanco (contraste insuficiente).

---

## Tipografía

**Manrope** instalada con `@fontsource/manrope` (sin dependencia de red en build). Pesos: 400/500/600/700/800. Importado en `main.tsx`.

---

## Iconos

`lucide-react`. Convención: `size={18}`, `strokeWidth={1.6}`, `color="currentColor"`.

---

## Componentes UI (`src/components/ui/`)

Todos accesibles desde el barrel `src/components/ui/index.ts`.

### Button

```tsx
<Button variant="primary" size="md" loading={false} icon={<Plus size={16} />}>
  Añadir reseña
</Button>
```

**Variantes:** `primary` `secondary` `ghost` `blue` `destructive`  
**Tamaños:** `sm` `md` `lg`  
**Props extra:** `loading` (spinner + no interacción), `icon` (izquierda)

### Card

```tsx
<Card padding="var(--space-6)">
  Contenido
</Card>
```

Fondo blanco, borde `--slate-200`, `--radius-lg`, `--shadow-sm`.

### Field + Input + Textarea

```tsx
<Field label="Nombre del negocio" error="Campo obligatorio">
  <Input value={val} onChange={...} iconLeft={<Search size={16} />} />
</Field>

<Field label="Descripción">
  <Textarea value={val} onChange={...} maxLength={500} />
</Field>
```

Altura de input: 42px. Foco: anillo azul `--blue-500`. Error: borde rojo.

### StatusBadge

```tsx
<StatusBadge status="auto" />
// Estados: pending | auto | escalated | replied | error
```

### Stars

```tsx
<Stars value={4.5} size={16} />
```

Admite decimales (media estrella con relleno parcial SVG).

### Avatar

```tsx
<Avatar name="María García López" size={40} />
```

Genera iniciales como fallback. Acepta prop `src` para imagen.

### KpiCard

```tsx
<KpiCard label="Reseñas este mes" value="247" trend={12.4} />
// trend positivo → verde ▲, negativo → rojo ▼
```

### Toggle / Checkbox / Radio / Select

```tsx
<Toggle checked={active} onChange={setActive} label="Auto-respuesta" />
<Checkbox checked={val} onChange={setVal} label="Recibir alertas" />
<Radio checked={tone==='formal'} onChange={setTone} value="formal" label="Formal" />
<Select options={[{value:'es', label:'Español'}]} value={lang} onChange={setLang} />
```

### ProgressBar

```tsx
<ProgressBar value={189} max={500} color="navy" />
// colores: navy | blue | green
```

### Toast

```tsx
// En la raíz de la app (main.tsx), envolver con:
<ToastProvider>...</ToastProvider>

// En cualquier componente:
const { addToast } = useToast();
addToast('Guardado correctamente.', 'success');
// variantes: success | error | warning | info
```

### Modal / ConfirmDialog

```tsx
<Modal open={open} onClose={close} title="Editar reseña" footer={<Button onClick={close}>Cerrar</Button>}>
  Contenido
</Modal>

<ConfirmDialog
  open={open}
  onClose={close}
  onConfirm={handleDelete}
  title="Eliminar reseña"
  message="Esta acción no se puede deshacer."
  destructive
/>
```

### Drawer

```tsx
<Drawer open={open} onClose={close} title="Detalle de reseña">
  Contenido del panel
</Drawer>
```

Panel lateral derecho (460px), entra desde la derecha.

### ChipInput

```tsx
<ChipInput
  values={keywords}
  onChange={setKeywords}
  placeholder="Añade palabras clave…"
/>
```

### Gráficas (SVG, sin dependencias)

```tsx
<LineChart data={[{label:'1 may', value:5}, ...]} title="Reseñas recibidas" height={180} />

<StarBars distribution={[142, 68, 21, 9, 7]} />
// distribución: [5★, 4★, 3★, 2★, 1★]

<Donut segments={[
  { label: 'Auto', value: 189, color: '#16A34A' },
  { label: 'Manual', value: 34, color: '#2563EB' },
]} size={120} thickness={20} />
```

---

## Chrome de la app

**Decisión de arquitectura:** el `<TopBar>` vive dentro de cada página (no en AppLayout). Esto permite que cada página declare su propio título y subtítulo sin necesitar un contexto global. El `AppLayout` solo provee el shell (sidebar + columna principal).

```
AppLayout
├── Sidebar (240px, fijo, blanco)
└── main (flex-col, fondo --slate-50)
    └── <Outlet>  → cada página renderiza:
        ├── <TopBar title="..." subtitle="..." />
        └── <div className={styles.content}>...</div>
```

---

## Crear una página nueva

```tsx
// src/Pages/MiPagina.tsx
import TopBar from '../components/layout/TopBar';
import { Card, Button } from '../components/ui';
import styles from './MiPagina.module.css';

export default function MiPagina() {
  return (
    <>
      <TopBar title="Mi página" subtitle="Descripción breve." />
      <div className={styles.content}>
        <Card>
          {/* contenido */}
        </Card>
      </div>
    </>
  );
}
```

```css
/* src/Pages/MiPagina.module.css */
.content {
  padding: var(--space-8);
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}
```

Añade la ruta en `src/router/AppRouter.tsx`.

---

## Fases futuras

### Reseñas (fase 2)
- Listado paginado con `StatusBadge`, `Stars`, `Avatar`
- Drawer de detalle con la reseña completa y el área de respuesta
- Botón "Responder con IA" (`Sparkles` icon, variante `blue`)
- Filtros con `Select` y `SlidersHorizontal`

### Facturación (fase 3)
- Planes: Free / Starter 29 €/mes / Pro 79 €/mes
- Cards de plan con `ProgressBar` de respuestas IA
- `ConfirmDialog` destructivo para cancelar suscripción

### Equipo (fase 4)
- Tabla de miembros con `Avatar`, nombre, rol
- `Modal` de invitación con `Input` de email y `Select` de rol
- Badges de estado (activo / pendiente)

### Onboarding (fase 5)
- Flujo de 3 pasos: conectar Google → configurar IA → primera reseña
- `ProgressBar` de progreso en la parte superior
- Cada paso es una `Card` con validación antes de avanzar
