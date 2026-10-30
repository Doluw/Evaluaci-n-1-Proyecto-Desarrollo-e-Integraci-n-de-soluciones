CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE TABLE `Bodegas` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Codigo` varchar(255) NOT NULL,
        `Nombre` longtext NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE TABLE `Productos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nombre` longtext NOT NULL,
        `UnidadMedida` longtext NOT NULL,
        `PrecioUnitario` decimal(18,2) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE TABLE `Inventarios` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ProductoId` int NOT NULL,
        `BodegaId` int NOT NULL,
        `Cantidad` int NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Inventarios_Bodegas_BodegaId` FOREIGN KEY (`BodegaId`) REFERENCES `Bodegas` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Inventarios_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    INSERT INTO `Bodegas` (`Id`, `Codigo`, `Nombre`)
    VALUES (1, 'VENTAS', 'Sala de Ventas');
    SELECT ROW_COUNT();

END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE UNIQUE INDEX `IX_Bodegas_Codigo` ON `Bodegas` (`Codigo`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE UNIQUE INDEX `IX_Inventarios_BodegaId_ProductoId` ON `Inventarios` (`BodegaId`, `ProductoId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    CREATE INDEX `IX_Inventarios_ProductoId` ON `Inventarios` (`ProductoId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261027031324_InicialBodega')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261027031324_InicialBodega', '10.0.12');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261028132423_AgregarVentas')
BEGIN
    CREATE TABLE `Ventas` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FechaHora` datetime(6) NOT NULL,
        `Caja` int NOT NULL,
        `RutCliente` varchar(12) NULL,
        `ClienteRegistrado` tinyint(1) NOT NULL,
        `Subtotal` decimal(18,2) NOT NULL,
        `PorcentajeDescuento` decimal(5,2) NOT NULL,
        `Descuento` decimal(18,2) NOT NULL,
        `Total` decimal(18,2) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261028132423_AgregarVentas')
BEGIN
    CREATE TABLE `DetallesVenta` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VentaId` int NOT NULL,
        `ProductoId` int NOT NULL,
        `Cantidad` int NOT NULL,
        `PrecioUnitario` decimal(18,2) NOT NULL,
        PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DetallesVenta_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `Productos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_DetallesVenta_Ventas_VentaId` FOREIGN KEY (`VentaId`) REFERENCES `Ventas` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261028132423_AgregarVentas')
BEGIN
    CREATE INDEX `IX_DetallesVenta_ProductoId` ON `DetallesVenta` (`ProductoId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261028132423_AgregarVentas')
BEGIN
    CREATE INDEX `IX_DetallesVenta_VentaId` ON `DetallesVenta` (`VentaId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261028132423_AgregarVentas')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261028132423_AgregarVentas', '10.0.12');
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261030003527_AgregarClientes')
BEGIN
    CREATE TABLE `Clientes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Rut` varchar(12) NOT NULL,
        `Nombres` varchar(80) NOT NULL,
        `Apellidos` varchar(80) NOT NULL,
        `Email` varchar(120) NULL,
        `Telefono` varchar(20) NULL,
        `FechaRegistro` datetime(6) NOT NULL,
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261030003527_AgregarClientes')
BEGIN
    CREATE UNIQUE INDEX `IX_Clientes_Rut` ON `Clientes` (`Rut`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261030003527_AgregarClientes')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261030003527_AgregarClientes', '10.0.12');
END;

COMMIT;

