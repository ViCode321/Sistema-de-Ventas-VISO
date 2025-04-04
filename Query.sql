--VICODESKTOP\SQLEXPRESS
--DESKTOP-B1Q4OHC
--DESKTOP-O07390I\SQLEXPRESS

USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'BDViso')
BEGIN
    DROP DATABASE BDViso;
END
GO
CREATE DATABASE BDViso;
GO
USE BDViso;
GO
Create Table Rol_Usuario(
	Rol_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Rol varchar(255)
)

Create Table Usuario(
	Usuario_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Nombre varchar(255),
	Contraseña varchar(255),
	Rol_Id int FOREIGN KEY REFERENCES Rol_Usuario(Rol_Id),
	Image varchar(1000),
	Status bit,
)

CREATE TABLE Proveedor(
    Proveedor_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre varchar(255),
    Direccion varchar(255),
	Image varchar(1000),
	Status bit
);

CREATE TABLE Proveedor_Telefono(
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Proveedor_Id int FOREIGN KEY REFERENCES Proveedor(Proveedor_Id),
    Numero varchar(50)
);

CREATE TABLE Categoria(
	Categoria_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Nombre varchar (255),
	Image varchar(1000),
	Status bit
);

CREATE TABLE Marca(
	Marca_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Nombre varchar (255),
	Image varchar(1000),
	Status bit
);

CREATE TABLE Producto(
    Producto_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Proveedor_Id int FOREIGN KEY REFERENCES Proveedor(Proveedor_Id),
	Descripcion varchar (255),
	Categoria_Id int FOREIGN KEY REFERENCES Categoria(Categoria_Id),
	Cantidad int,
	Costo float(2),
	Precio float(2),
	Marca_Id int FOREIGN KEY REFERENCES Marca(Marca_Id),
	Image varchar(1000),
	Status bit
);

CREATE TABLE Compra(
	Compra_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Fecha DATETIME,
	Total_Compra float(2),
);

CREATE TABLE Detalle_Compra(
	DetalleCompra_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Compra_Id int FOREIGN KEY REFERENCES Compra(Compra_Id),
	Usuario_Id int FOREIGN KEY REFERENCES Usuario(Usuario_Id),
	Producto_Id int FOREIGN KEY REFERENCES Producto(Producto_Id),
	Cantidad int,
	Precio_Unitario float(2),
	Total float(2),
	Proveedor_Id INT FOREIGN KEY REFERENCES Proveedor(Proveedor_Id)
);

CREATE TABLE Cliente(
    Cliente_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Nombre varchar(255),
    Apellido varchar(255),
	Status bit
);

CREATE TABLE Tipo_Moneda(
    Moneda_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Tipo varchar(255),
);

CREATE TABLE Factura(
    Factura_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Fecha Datetime,
    Cliente_Id int FOREIGN KEY REFERENCES Cliente(Cliente_Id),
	Monto_final float(2),
	Moneda_Id int FOREIGN KEY REFERENCES Tipo_Moneda(Moneda_Id)
);

CREATE TABLE Detalle_Factura(
    DetalleFactura_Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Factura_Id int FOREIGN KEY REFERENCES Factura(Factura_Id),
	Usuario_Id int FOREIGN KEY REFERENCES Usuario(Usuario_Id),
	Producto_Id int FOREIGN KEY REFERENCES Producto(Producto_Id),
	Cantidad int,
	Precio float(2),
	Total_Ventas float(2),
	Status bit 
);

CREATE TABLE Proveedor_Producto(
	Proveedor_Id int FOREIGN KEY REFERENCES Proveedor(Proveedor_Id),
	Producto_Id int FOREIGN KEY REFERENCES Producto(Producto_Id)
)

GO
CREATE PROCEDURE ActualizarCantidad
	@id  INT,
	@cantidad INT,
	@accion VARCHAR(10)
AS
BEGIN
	IF @accion = 'sumar'
		UPDATE Producto
		SET Cantidad = Cantidad + @cantidad
		WHERE Producto_Id = @id
	ElSE IF @accion = 'restar'
		UPDATE Producto
		SET Cantidad = Cantidad - @cantidad
		WHERE Producto_Id = @id
	ELSE IF @accion = 'restar_factura'
		UPDATE Factura
		SET Monto_final = Monto_final - @cantidad
		WHERE Factura_Id = @id
	ELSE
		UPDATE Compra
		SET Total_Compra = Total_Compra - @cantidad
		WHERE Compra_Id = @id
END

--EXEC ActualizarCantidad 1, 4, 'restar'
GO
CREATE PROCEDURE ObtenerCantidadTotal
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Cliente WHERE Status != 0) AS TotalClients,
        (SELECT COUNT(*) FROM Proveedor WHERE Status != 0) AS TotalSuplliers,
        (SELECT COUNT(*) FROM Factura) AS TotalInvoices,
        (SELECT COUNT(*) FROM Compra) AS TotalPurchase;
END;

--exec ObtenerCantidadTotal
GO
CREATE PROCEDURE ObtenerPagosPorFactura
    @NumeroFactura INT
AS
BEGIN
    SELECT
        df.DetalleFactura_Id AS DetalleFactura_Id,
        df.Factura_Id AS Factura_Id,
        df.Producto_Id AS Producto_Id,
        c.Nombre AS Nombre,
        c.Apellido AS Apellido,
        f.Fecha AS Fecha,
        df.Cantidad AS Cantidad,
        df.Precio AS Precio,
        df.Total_ventas AS Total_ventas,
		f.Monto_final AS Monto_final,
        df.Status
    FROM
        Detalle_Factura df
    INNER JOIN
        Factura f ON df.Factura_Id = f.Factura_Id
    INNER JOIN
        Cliente c ON f.Cliente_Id = c.Cliente_Id
    WHERE
        df.Factura_Id = @NumeroFactura AND df.Status != 0;
END;

--exec ObtenerPagosPorFactura 1

GO
CREATE PROCEDURE ObtenerTotalMontoFinalQuincenal
AS
BEGIN
    SELECT ROUND(SUM(CASE WHEN tm.Tipo = 'Dolar' THEN f.Monto_final / 36.00 ELSE f.Monto_final END), 2) AS TotalMontoFinal
    FROM Factura f
    INNER JOIN Tipo_Moneda tm ON f.Moneda_Id = tm.Moneda_Id
    WHERE f.Fecha >= DATEADD(WEEK, -2, GETDATE());
END;


GO
CREATE PROCEDURE ObtenerTotalMontoFinalCompraQuincenal
AS
BEGIN
    SELECT ROUND(SUM(Total_Compra), 2) AS TotalMontoFinalCompra
    FROM Compra
    WHERE Fecha >= DATEADD(WEEK, -2, GETDATE());
END;

--execute ObtenerTotalMontoFinalQuincenal
--execute ObtenerTotalMontoFinalCompraQuincenal

GO
CREATE PROCEDURE ObtenerComprasUltimos7Dias
AS
BEGIN
    SELECT Fecha, ROUND(SUM(Total_Compra),2) AS TotalCompras
    FROM Compra
    WHERE Fecha >= DATEADD(day, -7, GETDATE())
    GROUP BY Fecha
    ORDER BY Fecha;
END;

GO
CREATE PROCEDURE ObtenerVentasUltimos7Dias
AS
BEGIN
    SELECT f.Fecha, ROUND(SUM(CASE WHEN tm.Tipo = 'Dolar' THEN f.Monto_final / 36.00 ELSE f.Monto_final END), 2) AS TotalVentas
    FROM Factura f
    INNER JOIN Tipo_Moneda tm ON f.Moneda_Id = tm.Moneda_Id
    WHERE f.Fecha >= DATEADD(day, -7, GETDATE())
    GROUP BY f.Fecha
    ORDER BY f.Fecha;
END;

--execute ObtenerVentasUltimos7Dias
--execute ObtenerComprasUltimos7Dias

--EXEC ActualizarCantidad 1, 4, 'restar'

GO
CREATE PROCEDURE ObtenerGananciaPerdidaQuincenal
AS
BEGIN
    SELECT 
		(SELECT ROUND(SUM(dc.Total), 2) AS Total_Perdidas 
		FROM Compra c 
		JOIN Detalle_Compra dc ON c.Compra_Id = dc.Compra_Id
		WHERE c.Fecha >= DATEADD(WEEK, -2, GETDATE())) AS Perdidas,
		(SELECT ROUND(SUM(CASE WHEN tm.Tipo = 'Dolar' THEN df.Total_Ventas / 36.00 ELSE df.Total_Ventas END), 2) 
		FROM Factura f 
		JOIN Detalle_Factura df ON f.Factura_Id = df.Factura_Id
		INNER JOIN Tipo_Moneda tm ON f.Moneda_Id = tm.Moneda_Id
		WHERE df.Status = 1 AND f.Fecha >= DATEADD(WEEK, -2, GETDATE())) AS Ganancias
END;
GO

--execute ObtenerGananciaPerdidaQuincenal

GO
CREATE PROCEDURE ObtenerVentasMensuales
AS
BEGIN
    SELECT DATEPART(wk, f.Fecha) AS Semana, ROUND(SUM(CASE WHEN tm.Tipo = 'Dolar' THEN f.Monto_final / 36.00 ELSE f.Monto_final END), 2) AS TotalVentas
    FROM Factura f
    INNER JOIN Tipo_Moneda tm ON f.Moneda_Id = tm.Moneda_Id
    WHERE f.Fecha >= DATEADD(MONTH, -1, GETDATE()) AND f.Fecha < DATEADD(MONTH, 0, GETDATE())
    GROUP BY DATEPART(wk, f.Fecha)
    ORDER BY Semana;
END;

--execute ObtenerVentasMensuales

GO
CREATE PROCEDURE ObtenerTop5ProductosVendidosMensual
AS
BEGIN
    SELECT TOP 5 
        p.Descripcion AS Producto,
        SUM(df.Cantidad) AS CantidadVendida
    FROM Detalle_Factura df
    INNER JOIN Producto p ON df.Producto_Id = p.Producto_Id
    INNER JOIN Factura f ON df.Factura_Id = f.Factura_Id
    WHERE f.Fecha >= DATEADD(MONTH, -1, GETDATE())
    GROUP BY p.Descripcion
    ORDER BY CantidadVendida DESC;
END;

--exec ObtenerTop5ProductosVendidosMensual

GO
CREATE PROCEDURE ObtenerComprasMensuales
AS
BEGIN
    SELECT DATEPART(wk, c.Fecha) AS Semana, ROUND(SUM(c.Total_Compra), 2) AS TotalCompras
    FROM Compra c
    WHERE c.Fecha >= DATEADD(MONTH, -1, GETDATE()) AND c.Fecha < DATEADD(MONTH, 0, GETDATE())
    GROUP BY DATEPART(wk, c.Fecha)
    ORDER BY Semana;
END;

--exec ObtenerComprasMensuales

GO
CREATE PROCEDURE ObtenerDatosDashboard
AS
BEGIN
    SELECT 
        ROUND(SUM(CASE WHEN tm.Tipo = 'Dolar' THEN f.Monto_final / 36.00 ELSE f.Monto_final END), 2) AS TotalVentas,

        (SELECT COUNT(*)
         FROM Detalle_Factura df
         INNER JOIN Factura f ON df.Factura_Id = f.Factura_Id
         WHERE CONVERT(DATE, f.Fecha) = CONVERT(DATE, GETDATE())) AS ProductosVendidos,
        
        (SELECT COUNT(*)
         FROM Producto
         WHERE Cantidad < 10 AND Status = 1) AS ProductosStockBajo, -- Asumiendo que 10 es el umbral para "bajo stock"
        
        (SELECT COUNT(DISTINCT f.Cliente_Id)
         FROM Factura f
         WHERE CONVERT(DATE, f.Fecha) = CONVERT(DATE, GETDATE())) AS ClientesAtendidos
    FROM Factura f
    INNER JOIN Tipo_Moneda tm ON f.Moneda_Id = tm.Moneda_Id
    WHERE CONVERT(DATE, f.Fecha) = CONVERT(DATE, GETDATE());
END;
GO

--exec ObtenerDatosDashboard

GO
CREATE PROCEDURE ObtenerTop5ProductosVendidosHoy
AS
BEGIN
	SELECT TOP 5 
        p.Descripcion AS Producto,
        SUM(df.Cantidad) AS CantidadVendida
    FROM Detalle_Factura df
    INNER JOIN Producto p ON df.Producto_Id = p.Producto_Id
	INNER JOIN Factura f ON df.Factura_Id = f.Factura_Id
    WHERE CONVERT(DATE, f.Fecha) = CONVERT(DATE, GETDATE())
    GROUP BY p.Descripcion
    ORDER BY CantidadVendida DESC;
END;

--exec ObtenerTop5ProductosVendidosHoy

GO
--Introduccion Datos
--Tabla Rol
Insert into Rol_Usuario(Rol) values ('Administrador')
Insert into Rol_Usuario(Rol) values ('Usuario')
--Tabla Usuario
Insert into Usuario(Nombre,Contraseña,Rol_Id, Image, Status) values ('Sofia', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3',1, '/img/customer/customer1.jpg',1)
Insert into Usuario(Nombre,Contraseña,Rol_Id, Image, Status) values ('Victor', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3',2, '/img/customer/customer5.jpg',1)
--Tabla Proveedor
Insert into Proveedor(Nombre, Direccion, Image, Status) values ('Gonper', 'De la Policia, 1/2 al norte', '/img/images/default/imagen.png', 1)
Insert into Proveedor(Nombre, Direccion, Image, Status) values ('Hispamer', '4PHJ+7X2, Managua 14003', '/img/images/default/imagen.png',1)
--Select * From Proveedor
--Tabla Telefono
Insert into Proveedor_Telefono(Proveedor_Id,Numero) values (1, '+505 25222059')
Insert into Proveedor_Telefono(Proveedor_Id,Numero) values (2, '+505 22781210')
--Select * From Proveedor_Telefono
--Tabla Categoria
Insert into Categoria(Nombre, Image, Status) values ('Boligrafos', '/img/images/images_category/Boligrafo.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Cuadernos', '/img/images/images_category/Cuaderno.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Lapiz', '/img/images/images_category/Lapiz.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Libretas', '/img/images/images_category/Libreta.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Manualidades', '/img/images/images_category/Manualidades.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Material De Arte', '/img/images/images_category/MaterialArte.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Material Escolar', '/img/images/images_category/MaterialEscolar.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Material Escritura', '/img/images/images_category/MaterialEscritura.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Material Oficina', '/img/images/images_category/MaterialOficina.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Material Regalos', '/img/images/images_category/Regalos.png', 1)
Insert into Categoria(Nombre, Image, Status) values ('Papel', '/img/images/images_category/Papel.png', 1)
--Select * From Categoria
--Tabla Marca
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Artline', '/img/images/images_brand/artline.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('BIC', '/img/images/images_brand/bic.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Kadio', '/img/images/images_brand/kadio.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Lider', '/img/images/images_brand/lider.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Loro', '/img/images/images_brand/loro.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Merletto', '/img/images/images_brand/merletto.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('MEMO', '/img/images/images_brand/memo.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('PaperMate', '/img/images/images_brand/papermate.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Pentel', '/img/images/images_brand/pentel.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Pontier', '/img/images/images_brand/pointer.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Tukan', '/img/images/images_brand/tucan.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Zebra', '/img/images/images_brand/zebra.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Faber Castle', '/img/images/images_brand/faber.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Stabilo', '/img/images/images_brand/stabilo.png', 1)
INSERT INTO Marca(Nombre, Image, Status) VALUES ('Otro', '/img/images/images_brand/otro.png', 1)
--Select * From Marca
--Tabla Producto
--Boligrafos
INSERT INTO Producto(Proveedor_Id, Descripcion, Cantidad, Categoria_Id, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapicero azul', 69, 1, 3.16,  5, 2, '/img/images/images_product/lapicero_azul_biC.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Cantidad, Categoria_Id, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapicero negro', 35, 1, 3.16,  5, 2, '/img/images/images_product/lapicero_negro_bic.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Cantidad, Categoria_Id, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapicero rojo', 22, 1, 3.16,  5, 2, '/img/images/images_product/lapicero_rojo_bic.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Cantidad, Categoria_Id, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapicero Azul 0.7', 6, 1, 10.88,  14, 8, '/img/images/images_product/lapicero_azul_papermate.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Cantidad, Categoria_Id, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapicero negro 0.5 Gel', 5, 1, 8.68,  12, 8, '/img/images/images_product/lapicero_gel05_papermate.png', 1);
--select * from Producto where Categoria_Id = 1

-----Cuadernos
--select * from Producto
--Cuadernos
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Cuaderno Cosido Cuadriculado', 2, 6, 35.00,  40, 4, '/img/images/images_product/cuaderno_cosido_lider.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Cuaderno Cosido Cuadriculado Normal', 2, 10, 2.38, 30, 4, '/img/images/images_product/cuaderno_cosido_lider2.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Cuaderno Cosido Doble Raya', 2, 4, 23.75, 30, 5, '/img/images/images_product/cuaderno_dobleraya_loro.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Cuaderno Cosido Rayado', 2, 22, 24.77, 30, 5, '/img/images/images_product/cuaderno_rayado_loro.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Cuaderno Espiral Rayado', 2, 16, 12.50, 16, 5, '/img/images/images_product/cuaderno_espiral_loro.png', 1);
--select * from Producto where Categoria_Id = 2
--Lápices
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lápiz Con Puntas', 3, 24, 3.17, 5, 10, '/img/images/images_product/Lapiz_pointer.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lápiz Grafito Jumbo', 3, 85, 2.02, 4, 9, '/img/images/images_product/Lapiz_jumbo.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (2, 'Lápiz Grafito Neon', 3, 12, 2.08, 4, 14, '/img/images/images_product/Lapiz_neon.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lápiz Grafito Mecánico', 3, 24, 5.83, 8, 14, '/img/images/images_product/Lapiz_mecanico_stabilo.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lápiz Metálico', 3, 135, 1.75, 3, 10, '/img/images/images_product/Lapiz_metalico_pointer.png', 1);
--select * from Producto where Categoria_Id = 3

--Blocks
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Block Con Raya Cocido', 4, 16, 10.38, 13, 7, '/img/images/images_product/Block_raya_memo.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Block Con Raya Doble Cara Cocido', 4, 6, 10.50, 13, 7, '/img/images/images_product/Block_raya_memo2.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Block Espiral Con Raya', 4, 6, 18, 22, 4, '/img/images/images_product/Block_Lider.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Block Espiral Sin Raya', 4, 8, 10, 12, 4, '/img/images/images_product/Block_sinraya_Lider.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Libreta de Apuntes', 4, 10, 9, 11, 4, '/img/images/images_product/Libreta_apuntes.png', 1);
Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Libreta De Apuntes', 4, 9, 14.72, 18, 7, '/img/images/images_product/Libreta_apuntes.png', 1);
--select * from Producto where Categoria_Id = 4

--Manualidades
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Cartulina Corriente', 5, 45, 3.40, 5, 15, '/img/images/images_product/Cartulina.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Cartulina Satinada', 5, 21, 8.33, 12, 15, '/img/images/images_product/Cartulina_Satinada.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Escarche Plateado Bolsitas', 5, 62, 2.50, 5, 6, '/img/images/images_product/Escarche_bolsita.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Foamy Moldeable', 5, 2, 45, 55, 15, '/img/images/images_product/Foamy_Moldeable.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Foamy Azul', 5, 18, 2.50, 5, 15, '/img/images/images_product/Foamy_azul.png', 1);

-- Materiales De Arte
Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Acuarelas', 6, 3, 28, 30, 11, '/img/images/default/imagen.png', 1);

Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Crayolas Pequeñas Cajas', 6, 20, 8.33, 12, 12, '/img/images/default/imagen.png', 1);

Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status)  
VALUES (2, 'Plastilina', 6, 7, 11.25, 14, 1, '/img/images/default/imagen.png', 1);

Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status) 
VALUES (2, 'Tempera Tukan', 6, 5, 26, 32, 11, '/img/images/default/imagen.png', 1);

Insert into Producto( Proveedor_Id, Descripcion, Categoria_Id, Cantidad,  Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Lapices De Colores Cajas', 6, 4, 45, 52, 13, '/img/images/default/imagen.png', 1);
--select * from Producto where Categoria_Id = 6

-- Material Escolar
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Calculadora KD 815', 7, 4, 35, 40, 3, '/img/images/images_product/Calculadora.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Estuche Geométrico Metálico', 7, 9, 12.67, 18, 15, '/img/images/images_product/Estuche.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Fichas Bibliográficas 3x5cm Pequeñas', 7, 6, 0, 1, 15, '/img/images/images_product/Fichas_bi.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Forros De Boletines', 7, 18, 15, 18, 15, '/img/images/images_product/Forro.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Mapas', 7, 12, 1.67, 3, 15, '/img/images/images_product/Mapas.png', 1);
--select * from Producto where Categoria_Id = 7


-- Material Escritura
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Borradores De Cabeza De Lápiz', 8, 24, 1.07, 2, 10, '/img/images/images_product/Borrador_Punta_Lapiz.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Borradores De Grafito Grande', 8, 29, 2.25, 4, 14, '/img/images/images_product/Borrador.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Borradores Lápiz', 8, 62, 4.50, 6, 15, '/img/images/images_product/Borrador_otro.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Corrector Lápiz', 8, 18, 4.50, 6, 10, '/img/images/images_product/Corrector.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Marcador Acrílico AL', 8, 7, 0, 20, 9, '/img/images/images_product/Marcador.png', 1);
--select * from Producto where Categoria_Id = 8

-- Material Oficina
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Barra De Silicon', 9, 22, 3.18, 4, 10, '/img/images/images_product/Barra_De_Silicon.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Carpeta', 9, 12, 7, 15, 15, '/img/images/images_product/Carpeta.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status)  
VALUES (1, 'Chinche Cabeza Plástico', 9, 6, 17, 20, 10, '/img/images/images_product/tachuelas.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Chinche Metálico Caja', 9, 35, 3.50, 5, 12, '/img/images/images_product/chinche.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES (1, 'Engrapadora Pequeña C/Grp', 9, 5, 32, 40, 15, '/img/images/images_product/engrapadora.png', 1);

INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES
    (1, 'Sellador 30yd', 9, 10, 8.00, 10.00, 10, '/img/images/images_product/cinta.png', 1),
    (2, 'Tape de regalo', 9, 34, 1.67, 3.00, 6, '/img/images/images_product/tape.png', 1),
    (2, 'Tape Escarchado', 9, 12, 12.00, 15.00, 6, '/img/images/images_product/tape_es.png', 1),
    (1, 'Tijeras pequeña escolar S/P', 9, 17, 5.00, 7.00, 15, '/img/images/images_product/tijeras.png', 1),
    (1, 'Tijeras Stanless Steel', 9, 2, 10.00, 15.00, 15, '/img/images/images_product/tijeras_s.png', 1);

--select * from Producto where Categoria_Id = 9


-- Material Regalos
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES
    (2, 'Banderas Plasticas', 10, 14, 1.50, 4.00, 15, '/img/images/images_product/bandera.png', 1),
    (2, 'Bolsa de Regalo Grande', 10, 12, 9.58, 15.00, 15, '/img/images/images_product/bolsa_grande.png', 1),
    (2, 'Bolsa de regalo mediana', 10, 11, 7.50, 10.00, 15, '/img/images/images_product/bolsa_mediana.png', 1),
    (2, 'Bolsa de regalo pequeña/panam', 10, 10, 6.00, 8.00, 15, '/img/images/images_product/bolsa_pequeña.png', 1);

--select * from Producto where Categoria_Id = 10

-- Papel
INSERT INTO Producto(Proveedor_Id, Descripcion, Categoria_Id, Cantidad, Costo, Precio, Marca_Id, Image, Status) 
VALUES
    (1, 'Hojas de Colores', 11, 131, 0.60, 1.00, 15, '/img/images/images_product/hojas_c.png', 1),
    (1, 'Pliegos de Papel Craff', 11, 44, 3.20, 5.00, 15, '/img/images/images_product/papel_craft.png', 1),
    (1, 'Pliegos de papel de regalo', 11, 48, 3.20, 5.00, 15, '/img/images/images_product/papel_regalo.png', 1),
    (1, 'Pliegos de papelón Bond', 11, 82, 1.80, 4.00, 15, '/img/images/images_product/papel_bond.png', 1),
    (1, 'Hojas blancas Resma de papel', 11, 467, 0.20, 0.50, 7, '/img/images/images_product/papel.png', 1);

--select * from Producto where Categoria_Id = 11

--Tabla Proveedor_Producto
Insert into Proveedor_Producto( Proveedor_Id, Producto_Id) 
values 
	(1, 1),
	(1, 2),
	(1, 3),
	(1, 4),
	(1, 5),
	(1, 6),
	(1, 7),
	(1, 8),
	(1, 9),
	(2, 10),
	(1, 11),
	(1, 12),
	(2, 13),
	(1, 14),
	(1, 15),
	(1, 16),
	(1, 17),
	(1, 18),
	(2, 19),
	(1, 20),
	(1, 21),
	(1, 22),
	(2, 23),
	(1, 24),
	(1, 25),
	(1, 26),
	(1, 27),
	(2, 28),
	(2, 29),
	(1, 30),
	(1, 31),
	(1, 32),
	(1, 33),
	(1, 34),
	(1, 35),
	(1, 36),
	(1, 37),
	(1, 38),
	(1, 39),
	(1, 40),
	(1, 41),
	(1, 42),
	(1, 43),
	(1, 44),
	(1, 45),
	(1, 46),
	(2, 47),
	(2, 48),
	(1, 49),
	(1, 50),
	(2, 51),
	(2, 52),
	(2, 53),
	(2, 54),
	(2, 55),
	(1, 56),
	(1, 57),
	(1, 58),
	(1, 59),
	(1, 60);
--Select * From Proveedor_Producto

--Select * From Compra

Insert into Compra(Fecha,Total_Compra) 
values 
	(GETDATE(), 218.04),
	(GETDATE(), 110.60),
	(GETDATE(), 69.52),
	(GETDATE(), 65.28),
	(GETDATE(), 43.40),
	(GETDATE(), 210.00),
	(GETDATE(), 23.80),
	(GETDATE(), 95.00),
	(GETDATE(), 544.94),
	(GETDATE(), 200.00)

--Select * From Detalle_Compra

Insert into Detalle_Compra(Compra_Id,Usuario_Id,Producto_Id,Cantidad,Precio_Unitario,Total, Proveedor_Id)
values 
	(1,1,1,69,3.16,218.04, 1),
	(2,1,2,35,3.16,110.60, 1),
	(3,1,3,22,3.16,69.52, 1),
	(4,1,4,6,10.88,65.28, 1),
	(5,1,5,5,8.68,43.40, 1),
	(6,1,6,6,35,210.00, 1),
	(7,1,7,10,2.38,23.80, 1),
	(8,1,8,4,23.75,95.00, 1),
	(9,1,9,22,24.77,544.94, 1),
	(10,1,10,16,12.50,200.00, 2)

--Tabla Clientes
Insert into Cliente (Nombre, Apellido,Status) Values ('Variado', '',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Maria', 'Urtecho',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Victor', 'Guevara',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Ana', 'Siu',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Leonel', 'Chavez',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Andrea', 'Lopez',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Veronica', 'Martinez',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Carolina', 'Cortez',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Cristian', 'Tinoco',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Josue', 'Herrera',1)
Insert into Cliente (Nombre, Apellido,Status) Values ('Camila', 'Sanchez',1)
--SELECT * FROM Cliente

--Tabla Tipo Moneda
Insert Into Tipo_Moneda(Tipo) values ('Cordoba')
Insert Into	Tipo_Moneda(Tipo) values ('Dolar')
--Select * From Tipo_Moneda

--Tabla Factura
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 1, 62, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 2, 300, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 3, 36, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 4, 40, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 5, 80, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 6, 180, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 7, 66, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 8, 56, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 9, 24, 1)
Insert into Factura (Fecha,Cliente_Id,Monto_Final,Moneda_Id) Values (GETDATE(), 10, 1, 2)

--Select * From Factura

--Detalle Factura
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (1, 2, 2,5, 10,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (1, 30, 1,52, 52,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (2, 7, 10,30, 300,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (3, 5, 3,12, 36,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (4, 6, 1,40, 40,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (5, 10, 5,16, 80,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (6, 20, 10,18, 180,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (7, 18, 3,22, 66,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (8, 28, 4,14, 56,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (9, 55, 3,8, 24,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (10, 32, 2,18, 1,1, 1)
Insert into Detalle_Factura (Factura_Id,Producto_Id,Cantidad,Precio,Total_ventas,Usuario_Id, Status) Values (10, 32, 2,18, 1,1, 1)
--Select * From Detalle_Factura

/**/