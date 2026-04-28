# Conexión a Base de Datos desde DBeaver

## Pasos para conectar la base de datos SQLite desde DBeaver

### 1. Localizar el archivo de base de datos
- **Ruta de desarrollo local:** `Data/parcial.db`
- **Ruta en Render (producción):** `/var/data/app.db`

### 2. Configurar conexión en DBeaver

#### Opción A: Base de datos local
1. Abre DBeaver
2. Ve a `Database` → `New Database Connection`
3. Selecciona `SQLite` como tipo de base de datos
4. Haz clic en `Next`
5. En la sección de ruta, selecciona el archivo: `[ruta-del-proyecto]\Data\parcial.db`
6. Prueba la conexión haciendo clic en `Test Connection`
7. Finaliza la configuración

#### Opción B: Base de datos en producción (Render)
1. El servidor estará disponible en: `https://parcial.onrender.com` (una vez desplegado)
2. Para acceder a los datos desde DBeaver en producción:
   - Los datos se almacenan en `/var/data/app.db` dentro del contenedor
   - Render no expone acceso directo a SSH, así que la conexión debe ser a través de la API de la aplicación

### 3. Tablas principales

La base de datos incluye las siguientes tablas principales:

- **AspNetUsers** - Usuarios del sistema (Identity)
- **AspNetRoles** - Roles disponibles (Analista, Usuario)
- **AspNetUserRoles** - Relación Usuario-Rol
- **Clientes** - Información de clientes
- **SolicitudCredito** - Solicitudes de crédito
- **Migrations** - Historial de migraciones de EF Core

### 4. Usuarios de prueba (en el seed)

| Email | Contraseña | Rol |
|-------|-----------|-----|
| analista@parcial.com | Parcial2026! | Analista |
| cliente1@parcial.com | Parcial2026! | Usuario |
| cliente2@parcial.com | Parcial2026! | Usuario |

### 5. Crear la base de datos

Si es la primera vez:
1. Ejecuta: `dotnet run`
2. La aplicación crea automáticamente la BD con todas las migraciones
3. Se crearán los usuarios de prueba automáticamente
4. Luego puedes conectarte desde DBeaver

### 6. Troubleshooting

- **"Database file not found"**: Verifica que el archivo exista en `Data/parcial.db`
- **"File is locked"**: La aplicación está usando la BD. Detén la ejecución con `Ctrl+C`
- **"Permission denied"**: Verifica permisos de lectura/escritura en la carpeta `Data/`

---
Para más información, consulta la documentación de [DBeaver SQLite](https://dbeaver.io/)
