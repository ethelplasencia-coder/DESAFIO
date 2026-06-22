using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TiendaProductosDama.Biblioteca
{
    // =============================================================
    //  MODELOS
    // =============================================================

    /// <summary>
    /// Representa un producto del inventario.
    /// El código sigue el formato: 1 letra + 3 dígitos (Ej: J001, A123).
    /// </summary>
    public class Producto
    {
        public string Codigo { get; set; } = "";      // Ej: "J001"
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    /// <summary>
    /// Representa una venta registrada en el sistema.
    /// </summary>
    public class Venta
    {
        public string CodigoProducto { get; set; } = "";
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
    }

    /// <summary>Tipo de movimiento de inventario.</summary>
    public enum TipoMovimiento
    {
        Entrada,
        Salida
    }

    /// <summary>
    /// Representa un movimiento de stock para trazabilidad y auditoría.
    /// Se registra automáticamente cada vez que cambia el stock de un producto.
    /// </summary>
    public class MovimientoStock
    {
        public string CodigoProducto { get; set; } = "";
        public string NombreProducto { get; set; } = "";
        public TipoMovimiento Tipo { get; set; }
        public int Cantidad { get; set; }
        public int StockResultante { get; set; }
        public string Motivo { get; set; } = "";   // "Entrada manual", "Salida manual", "Venta"
        public DateTime Fecha { get; set; }
    }

    // =============================================================
    //  GESTOR PRINCIPAL (BIBLIOTECA)
    // =============================================================

    /// <summary>
    /// Clase biblioteca: contiene los datos y todas las funciones del sistema.
    /// Program.cs crea un objeto de esta clase y le delega toda la lógica.
    /// </summary>
    public class GestorTienda
    {
        // ── Repositorios en memoria ──────────────────────────────
        public List<Producto> productos = new List<Producto>();
        public List<Venta> ventas = new List<Venta>();
        public List<MovimientoStock> movimientos = new List<MovimientoStock>();

        // ── Constantes de configuración ──────────────────────────
        private const string EJEMPLO_CODIGO = "A123";
        private const int UMBRAL_CANTIDAD_SOSPECHOSA = 500;

        // =========================================================
        //  OPCIÓN 1: REGISTRAR PRODUCTO
        // =========================================================

        public void RegistrarProducto()
        {
            Console.WriteLine("\n--- REGISTRAR PRODUCTO ---");

            Producto p = new Producto
            {
                Codigo = LeerCodigoNuevo(),
                Nombre = LeerTextoNoVacio("Nombre del producto: ", "El nombre no puede estar vacío."),
                Categoria = LeerTextoNoVacio("Categoría: ", "La categoría no puede estar vacía."),
                Precio = LeerDecimalNoNegativo("Precio: S/ "),
                Stock = LeerEnteroNoNegativo("Stock inicial: ")
            };

            productos.Add(p);
            Console.WriteLine("\nProducto registrado correctamente.");
        }

        /// <summary>
        /// Pide un código NUEVO con entrada restringida tecla a tecla (1 letra + 3 dígitos).
        /// Valida que no esté ya registrado.
        /// </summary>
        private string LeerCodigoNuevo()
        {
            while (true)
            {
                Console.Write($"Código del producto (ej: {EJEMPLO_CODIGO}): ");
                string entrada = LeerCodigoConTeclado();

                if (productos.Any(prod => prod.Codigo == entrada))
                {
                    Console.WriteLine("Ya existe un producto con ese código. Ingrese uno diferente.");
                    continue;
                }

                return entrada;
            }
        }

        // =========================================================
        //  OPCIÓN 2: LISTAR PRODUCTOS
        // =========================================================

        public void ListarProductos()
        {
            Console.WriteLine("\n--- LISTA DE PRODUCTOS ---");

            if (productos.Count == 0)
            {
                Console.WriteLine("No hay productos registrados.");
                return;
            }

            // Encabezado con columnas alineadas
            Console.WriteLine("\n{0,-8} {1,-25} {2,-15} {3,-12} {4,-8}",
                "Código", "Nombre", "Categoría", "Precio", "Stock");
            Console.WriteLine(new string('-', 72));

            foreach (Producto p in productos.OrderBy(p => p.Codigo))
            {
                // Alerta visual si el stock es bajo (menos de 5 unidades)
                string alertaStock = p.Stock < 5 ? " ⚠ STOCK BAJO" : "";

                Console.WriteLine("{0,-8} {1,-25} {2,-15} {3,-12} {4,-8}{5}",
                    p.Codigo, p.Nombre, p.Categoria,
                    $"S/ {p.Precio:N2}", p.Stock, alertaStock);
            }

            Console.WriteLine(new string('-', 72));
            Console.WriteLine($"Total de productos registrados: {productos.Count}");
        }

        // =========================================================
        //  OPCIÓN 3: ENTRADA DE STOCK
        // =========================================================

        public void EntradaStock()
        {
            Console.WriteLine("\n--- ENTRADA DE STOCK ---");

            if (!HayProductosRegistrados()) return;

            ListarProductos();
            Producto p = SeleccionarProductoExistente();

            int cantidad = LeerCantidadConConfirmacion("Cantidad que ingresa: ");

            p.Stock += cantidad;
            RegistrarMovimiento(p, TipoMovimiento.Entrada, cantidad, "Entrada manual");

            Console.WriteLine($"\nStock actualizado. Nuevo stock de \"{p.Nombre}\": {p.Stock}");
        }

        // =========================================================
        //  OPCIÓN 4: SALIDA DE STOCK
        // =========================================================

        public void SalidaStock()
        {
            Console.WriteLine("\n--- SALIDA DE STOCK ---");

            if (!HayProductosRegistrados()) return;

            ListarProductos();
            Producto p = SeleccionarProductoExistente();

            int cantidad;
            while (true)
            {
                cantidad = LeerCantidadConConfirmacion("Cantidad que sale: ");

                if (cantidad > p.Stock)
                {
                    Console.WriteLine($"Stock insuficiente. Stock actual: {p.Stock}. Ingrese una cantidad menor o igual.");
                    continue;
                }
                break;
            }

            p.Stock -= cantidad;
            RegistrarMovimiento(p, TipoMovimiento.Salida, cantidad, "Salida manual");

            Console.WriteLine($"\nSalida registrada. Nuevo stock de \"{p.Nombre}\": {p.Stock}");
        }

        // =========================================================
        //  OPCIÓN 5: REGISTRAR VENTA
        // =========================================================

        public void RegistrarVenta()
        {
            Console.WriteLine("\n--- REGISTRAR VENTA ---");

            if (!HayProductosRegistrados()) return;

            ListarProductos();
            Producto p = SeleccionarProductoExistente();

            int cantidad;
            while (true)
            {
                cantidad = LeerCantidadConConfirmacion("Cantidad vendida: ");

                if (cantidad > p.Stock)
                {
                    Console.WriteLine($"Stock insuficiente. Stock actual: {p.Stock}. Ingrese una cantidad menor o igual.");
                    continue;
                }
                break;
            }

            decimal total = cantidad * p.Precio;
            p.Stock -= cantidad;
            RegistrarMovimiento(p, TipoMovimiento.Salida, cantidad, "Venta");

            ventas.Add(new Venta
            {
                CodigoProducto = p.Codigo,
                NombreProducto = p.Nombre,
                Cantidad = cantidad,
                Total = total,
                Fecha = DateTime.Now
            });

            Console.WriteLine($"\nVenta registrada correctamente.");
            Console.WriteLine($"Producto : {p.Nombre} ({p.Codigo})");
            Console.WriteLine($"Cantidad : {cantidad} unidades");
            Console.WriteLine($"Total    : S/ {total:N2}");
            Console.WriteLine($"Stock restante: {p.Stock}");
        }

        // =========================================================
        //  OPCIÓN 6: REPORTE DE VENTAS
        // =========================================================

        public void ReporteVentas()
        {
            Console.WriteLine("\n--- REPORTE DE VENTAS ---");

            if (ventas.Count == 0)
            {
                Console.WriteLine("No hay ventas registradas.");
                return;
            }

            Console.WriteLine("\n{0,-17} {1,-25} {2,-10} {3,-12} {4,-8}",
                "Fecha", "Producto", "Cantidad", "Total", "Código");
            Console.WriteLine(new string('-', 80));

            decimal totalGeneral = 0;

            foreach (Venta v in ventas.OrderBy(v => v.Fecha))
            {
                Console.WriteLine("{0,-17} {1,-25} {2,-10} {3,-12} {4,-8}",
                    v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    v.NombreProducto, v.Cantidad,
                    $"S/ {v.Total:N2}", v.CodigoProducto);

                totalGeneral += v.Total;
            }

            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"Total de ventas  : {ventas.Count}");
            Console.WriteLine($"Total facturado  : S/ {totalGeneral:N2}");
        }

        // =========================================================
        //  OPCIÓN 7: MODIFICAR PRODUCTO
        // =========================================================

        public void ModificarProducto()
        {
            Console.WriteLine("\n--- MODIFICAR PRODUCTO ---");

            if (!HayProductosRegistrados()) return;

            ListarProductos();
            Producto p = SeleccionarProductoExistente();

            Console.WriteLine($"\nEditando \"{p.Nombre}\" ({p.Codigo}).");
            Console.WriteLine("Deje en blanco y presione ENTER para mantener el valor actual.\n");

            // Nombre
            Console.Write($"Nombre actual [{p.Nombre}]: ");
            string nuevoNombre = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevoNombre))
                p.Nombre = nuevoNombre.Trim();

            // Categoría
            Console.Write($"Categoría actual [{p.Categoria}]: ");
            string nuevaCategoria = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevaCategoria))
                p.Categoria = nuevaCategoria.Trim();

            // Precio
            Console.Write($"Precio actual [S/ {p.Precio:N2}]: ");
            string entradaPrecio = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(entradaPrecio))
            {
                if (decimal.TryParse(entradaPrecio, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out decimal nuevoPrecio) && nuevoPrecio >= 0)
                    p.Precio = nuevoPrecio;
                else
                    Console.WriteLine("Precio inválido. Se mantuvo el precio anterior.");
            }

            // El stock NO se edita aquí: los cambios de stock deben hacerse
            // por Entrada/Salida (opciones 3 y 4) para mantener trazabilidad.
            Console.WriteLine($"\nStock actual: {p.Stock} (use las opciones 3 o 4 para modificarlo).");
            Console.WriteLine("\nProducto actualizado correctamente.");
        }

        // =========================================================
        //  OPCIÓN 8: ELIMINAR PRODUCTO
        // =========================================================

        public void EliminarProducto()
        {
            Console.WriteLine("\n--- ELIMINAR PRODUCTO ---");

            if (!HayProductosRegistrados()) return;

            ListarProductos();
            Producto p = SeleccionarProductoExistente();

            Console.Write($"\n¿Está seguro de eliminar \"{p.Nombre}\" ({p.Codigo})? Esta acción no se puede deshacer (S/N): ");
            string confirmacion = Console.ReadLine()?.Trim().ToUpper();

            if (confirmacion == "S")
            {
                productos.Remove(p);
                Console.WriteLine("Producto eliminado correctamente.");
            }
            else
            {
                Console.WriteLine("Eliminación cancelada.");
            }
        }

        // =========================================================
        //  OPCIÓN 9: HISTORIAL DE MOVIMIENTOS DE STOCK
        // =========================================================

        public void HistorialMovimientos()
        {
            Console.WriteLine("\n--- HISTORIAL DE MOVIMIENTOS DE STOCK ---");

            if (movimientos.Count == 0)
            {
                Console.WriteLine("No hay movimientos registrados todavía.");
                return;
            }

            Console.WriteLine("\n{0,-17} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                "Fecha", "Código", "Producto", "Tipo", "Cantidad", "Stock final", "Motivo");
            Console.WriteLine(new string('-', 95));

            foreach (MovimientoStock m in movimientos.OrderBy(m => m.Fecha))
            {
                string tipoTexto = m.Tipo == TipoMovimiento.Entrada ? "Entrada" : "Salida";

                Console.WriteLine("{0,-17} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                    m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    m.CodigoProducto, m.NombreProducto,
                    tipoTexto, m.Cantidad, m.StockResultante, m.Motivo);
            }

            Console.WriteLine(new string('-', 95));

            int totalEntradas = movimientos.Where(m => m.Tipo == TipoMovimiento.Entrada).Sum(m => m.Cantidad);
            int totalSalidas = movimientos.Where(m => m.Tipo == TipoMovimiento.Salida).Sum(m => m.Cantidad);

            Console.WriteLine($"Total unidades ingresadas : {totalEntradas}");
            Console.WriteLine($"Total unidades retiradas  : {totalSalidas}");
        }

        // =========================================================
        //  MÉTODOS AUXILIARES PRIVADOS (validación y lectura)
        // =========================================================

        /// <summary>Verifica que existan productos; si no, avisa y retorna false.</summary>
        private bool HayProductosRegistrados()
        {
            if (productos.Count == 0)
            {
                Console.WriteLine("No hay productos registrados. Registre un producto primero (opción 1).");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pide un código de producto EXISTENTE con entrada restringida a 4 teclas.
        /// Se usa en Venta, Entrada, Salida, Modificar y Eliminar.
        /// </summary>
        private Producto SeleccionarProductoExistente()
        {
            while (true)
            {
                Console.Write("Ingrese el código del producto: ");
                string entrada = LeerCodigoConTeclado();

                Producto p = productos.FirstOrDefault(x => x.Codigo == entrada);
                if (p == null)
                {
                    Console.WriteLine("Producto no encontrado. Verifique el código en la lista de arriba.");
                    continue;
                }

                return p;
            }
        }

        /// <summary>
        /// Lee el código de producto tecla por tecla.
        /// Formato obligatorio: 1 letra (A-Z) + 3 dígitos. Total: 4 caracteres.
        /// No permite errores de formato; admite Backspace para corregir.
        /// </summary>
        private string LeerCodigoConTeclado()
        {
            char[] buffer = new char[4];
            int posicion = 0;

            while (true)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

                // Backspace: borra el último carácter ingresado
                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (posicion > 0)
                    {
                        posicion--;
                        Console.Write("\b \b");
                    }
                    continue;
                }

                // ENTER: solo se acepta si ya se completaron los 4 caracteres
                if (tecla.Key == ConsoleKey.Enter)
                {
                    if (posicion == 4) break;
                    continue;
                }

                // Ignora cualquier tecla adicional si ya están los 4 caracteres
                if (posicion >= 4) continue;

                char c = tecla.KeyChar;

                // Posición 0: debe ser una letra
                if (posicion == 0 && char.IsLetter(c))
                {
                    buffer[posicion] = char.ToUpper(c);
                    Console.Write(buffer[posicion]);
                    posicion++;
                }
                // Posiciones 1-3: deben ser dígitos
                else if (posicion >= 1 && char.IsDigit(c))
                {
                    buffer[posicion] = c;
                    Console.Write(c);
                    posicion++;

                    if (posicion == 4) break; // Completo: termina automáticamente
                }
                // Cualquier otra tecla se ignora silenciosamente
            }

            Console.WriteLine();
            return new string(buffer);
        }

        private string LeerTextoNoVacio(string mensaje, string mensajeError)
        {
            string texto;
            do
            {
                Console.Write(mensaje);
                texto = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(texto))
                    Console.WriteLine(mensajeError);
            }
            while (string.IsNullOrWhiteSpace(texto));

            return texto.Trim();
        }

        private decimal LeerDecimalNoNegativo(string mensaje)
        {
            decimal valor;
            while (true)
            {
                Console.Write(mensaje);
                string entrada = Console.ReadLine();

                if (decimal.TryParse(entrada, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out valor) && valor >= 0)
                    return valor;

                Console.WriteLine("Valor inválido. Ingrese un número no negativo (ej: 25.50).");
            }
        }

        private int LeerEnteroNoNegativo(string mensaje)
        {
            int valor;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out valor) && valor >= 0)
                    return valor;

                Console.WriteLine("Valor inválido. Ingrese un número entero no negativo.");
            }
        }

        private int LeerEnteroPositivo(string mensaje)
        {
            int valor;
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out valor) && valor > 0)
                    return valor;

                Console.WriteLine("Valor inválido. Ingrese un número entero mayor que cero.");
            }
        }

        /// <summary>
        /// Lee una cantidad positiva. Si supera el umbral configurado,
        /// pide confirmación explícita para evitar errores de tipeo.
        /// </summary>
        private int LeerCantidadConConfirmacion(string mensaje)
        {
            while (true)
            {
                int cantidad = LeerEnteroPositivo(mensaje);

                if (cantidad > UMBRAL_CANTIDAD_SOSPECHOSA)
                {
                    Console.Write($"La cantidad ingresada ({cantidad}) es inusualmente alta. ¿Confirma? (S/N): ");
                    string confirmacion = Console.ReadLine()?.Trim().ToUpper();

                    if (confirmacion != "S")
                    {
                        Console.WriteLine("Cantidad descartada. Vuelva a ingresarla.");
                        continue;
                    }
                }

                return cantidad;
            }
        }

        /// <summary>
        /// Registra un movimiento de stock en el historial para mantener trazabilidad.
        /// Se llama automáticamente en EntradaStock, SalidaStock y RegistrarVenta.
        /// </summary>
        private void RegistrarMovimiento(Producto p, TipoMovimiento tipo, int cantidad, string motivo)
        {
            movimientos.Add(new MovimientoStock
            {
                CodigoProducto = p.Codigo,
                NombreProducto = p.Nombre,
                Tipo = tipo,
                Cantidad = cantidad,
                StockResultante = p.Stock,
                Motivo = motivo,
                Fecha = DateTime.Now
            });
        }
    }
}
