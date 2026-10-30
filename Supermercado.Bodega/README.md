# Sistema de Bodegas

Aplicación ASP.NET Core MVC para administrar las existencias de un supermercado. Usa Entity Framework Core, MySQL y el proyecto compartido `Supermercado.Datos`.

## Funciones implementadas

- Listado de bodegas y productos disponibles en cada una (nombre, cantidad y unidad de medida).
- Bodega inicial **Sala de Ventas**, identificada con el código `VENTAS`.
- Creación de bodegas y productos con ID autogenerado.
- Ingreso y retiro de cantidades, sin permitir stock negativo.
- Movimientos de productos entre bodegas mediante una transacción.
- Carga de incrementos de inventario desde JSON; el archivo completo se valida antes de guardar.

## Configuración local

Desde la carpeta de la solución, configure la conexión sin guardar la contraseña en el repositorio:

```powershell
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=supermercado_db;User=USUARIO;Password=CLAVE" --project .\Supermercado.Bodega
dotnet ef database update --project .\Supermercado.Datos --startup-project .\Supermercado.Bodega
dotnet run --project .\Supermercado.Bodega
```

El proyecto sigue un enfoque **Code First**: las entidades y relaciones se mantienen en `Supermercado.Datos`, mientras que las migraciones versionan la creación de la base común usada por los sistemas.

## Formato JSON

Se admite un objeto o una lista de objetos. Los nombres de las propiedades no distinguen mayúsculas de minúsculas.

```json
[
  { "productoId": 1, "bodegaId": 2, "cantidad": 25 },
  { "productoId": 3, "bodegaId": 1, "cantidad": 10 }
]
```

Los productos y bodegas deben existir y las cantidades deben ser positivas. Si alguna fila es inválida, no se aplica ninguna parte de la importación.
