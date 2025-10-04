# PortalAcademicoApp

**PortalAcademicoApp** es una aplicación web desarrollada con **ASP.NET Core 8**, que permite la gestión de cursos, matrículas y usuarios con roles, diseñada para instituciones educativas.

La aplicación está desplegada como **web service** y es accesible desde cualquier navegador.

---

## **Funcionalidades principales**

### **Usuarios y Roles**
- Sistema de autenticación con **Identity**.
- Roles definidos:
  - **Coordinador**: Gestiona cursos y visualiza matrículas.
  - **Usuario estándar**: Puede inscribirse en cursos activos.
- Seed inicial:
  - Usuario coordinador: `coordinador@uni.edu` / `P@ssw0rd123!`
  - Rol coordinador creado automáticamente si no existe.

### **Gestión de Cursos**
- Listar todos los cursos activos (`/Cursos/Index`).
- Filtrar cursos por nombre y créditos.
- Crear, editar y desactivar cursos (solo coordinador, `/Coordinador/Cursos`).
- Panel de coordinador:
  - Visualiza cursos y su estado.
  - Gestiona matrículas de los estudiantes por curso.

### **Matrículas**
- Los usuarios pueden inscribirse en cursos activos.
- Validaciones automáticas:
  - Evita duplicados.
  - Control de cupo máximo.
  - Evita solapamiento de horarios.
- Estado de matrícula:
  - Pendiente, Confirmada o Cancelada.
- Coordinador puede confirmar o cancelar matrículas de los estudiantes.

### **Cache y Sesión**
- Uso de **Redis** para cacheo de cursos activos (mejora rendimiento).
- Sesiones activas por 30 minutos.

### **Redirección según rol**
- Al iniciar sesión:
  - **Coordinador** → `/Coordinador/Cursos`
  - Otros usuarios → `/Cursos/Index`

---

## **Cómo navegar por el proyecto**

### **1. Página de inicio**
- URL base: `https://portalacademic.onrender.com/`
- Si no estás logueado, se puede navegar al catálogo de cursos para ver los cursos activos.

### **2. Inicio de sesión**
- Haz clic en "Iniciar sesión".
- Ingresa con un usuario registrado.
- Si eres coordinador, serás redirigido automáticamente al **panel de coordinador**.

### **3. Coordinador**
- URL: `/Coordinador/Cursos`
- Funcionalidades:
  - Crear, editar y desactivar cursos.
  - Ver matrículas de cada curso.
  - Confirmar o cancelar matrículas.

### **4. Usuario estándar**
- URL: `/Cursos/Index`
- Funcionalidades:
  - Visualizar el catálogo de cursos.
  - Filtrar por nombre y créditos.
  - Inscribirse en cursos activos.

---

## **Notas adicionales**
- La aplicación está lista para producción con **HTTPS**.
- Redis se usa para cache de cursos; si no está disponible, se usa memoria local como fallback.
- Todos los cursos y usuarios se inicializan automáticamente mediante el SeedData si la base de datos está vacía.

---

## **Credenciales iniciales (Seed)**
| Rol          | Email                  | Contraseña       |
|--------------|----------------------|----------------|
| Coordinador  | coordinador@uni.edu    | P@ssw0rd123!   |

---

## **Tecnologías**
- ASP.NET Core 8
- Entity Framework Core (SQLite)
- Identity para autenticación y roles
- Redis para cacheo y sesiones
- Render como hosting del web service
