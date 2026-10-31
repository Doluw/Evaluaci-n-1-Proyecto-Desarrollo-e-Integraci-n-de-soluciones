# Sistemas del supermercado

La solución contiene tres aplicaciones ASP.NET Core independientes que comparten una base MySQL mediante Entity Framework Core:

- `Supermercado.Bodega`
- `Supermercado.Ventas`
- `Supermercado.Clientes`
- `Supermercado.Datos` (entidades, contexto y migraciones compartidas)

## Preparación en otro computador

La contraseña no se guarda en el repositorio. Cada persona puede utilizar su propio usuario y contraseña MySQL, siempre que ese usuario tenga permisos sobre `supermercado_db`.

Los tres proyectos web comparten el mismo `UserSecretsId`, por lo que la conexión se configura una sola vez:

```powershell
dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3306;Database=supermercado_db;User=SU_USUARIO;Password=SU_CLAVE" --project .\Supermercado.Bodega
```

Después se crea o actualiza la base desde las migraciones:

```powershell
dotnet ef database update --project .\Supermercado.Datos --startup-project .\Supermercado.Bodega
```

Como alternativa para una instalación nueva, se puede ejecutar `supermercado.sql` en MySQL Workbench. Ese script está pensado para una base vacía y no debe ejecutarse nuevamente sobre tablas existentes.

## Ejecución

Abra una terminal para cada sistema:

```powershell
dotnet run --project .\Supermercado.Bodega
dotnet run --project .\Supermercado.Ventas
dotnet run --project .\Supermercado.Clientes
```

La solución usa un enfoque Code First. La base común se versiona mediante las migraciones incluidas en `Supermercado.Datos\Migrations`.
