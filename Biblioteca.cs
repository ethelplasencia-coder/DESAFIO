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
        public string Motivo { get; set; } = "";   // "Entrada manual", "Dañado", "Vencido", "Devolución", "Venta", etc.
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

        // ── Motivos predefinidos para salida de stock ────────────
        private static readonly string[] MOTIVOS_SALIDA = { "Dañado", "Vencido", "Devolución", "Otro" };

        // =========================================================
        //  OPCIÓN 1: REGISTRAR PRODUCTO
        // =========================================================

        public void RegistrarProducto()
        {
            Console.WriteLine("\n--- REGISTRAR PRODUCTO ---");

            Producto p = new Producto
            {
                Codigo = LeerCodigoNuevo(),
                Nombre = LeerTextoSoloLetras("Nombre del producto: ", "El nombre no puede estar vacío y solo puede contener letras."),
                Categoria = LeerTextoSoloLetras("Categoría: ", "La categoría no puede estar vacía y solo puede contener letras."),
                Precio = LeerDecimalPositivo("Precio: S/ "),
                Stock = LeerEnteroPositivo("Stock inicial: ")
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
            if (p == null)
            {
                Console.WriteLine("\nOperación cancelada.");
                return;
            }

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
            if (p == null)
            {
                Console.WriteLine("\nOperación cancelada.");
                return;
            }

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

            string motivo = LeerMotivoSalida();

            p.Stock -= cantidad;
            RegistrarMovimiento(p, TipoMovimiento.Salida, cantidad, motivo);

            Console.WriteLine($"\nSalida registrada. Motivo: {motivo}. Nuevo stock de \"{p.Nombre}\": {p.Stock}");
        }

        /// <summary>
        /// Muestra un menú de motivos predefinidos para la salida de stock.
        /// Si el usuario elige "Otro", solicita un texto descriptivo no vacío.
        /// </summary>
        private string LeerMotivoSalida()
        {
            Console.WriteLine("\nSeleccione el motivo de la salida:");
            for (int i = 0; i < MOTIVOS_SALIDA.Length; i++)
                Console.WriteLine($"  {i + 1}. {MOTIVOS_SALIDA[i]}");

            int opcion;
            while (true)
            {
                Console.Write($"Opción (1-{MOTIVOS_SALIDA.Length}): ");
                string entrada = Console.ReadLine();

                if (int.TryParse(entrada, out opcion) && opcion >= 1 && opcion <= MOTIVOS_SALIDA.Length)
                    break;

                Console.WriteLine("Opción inválida. Intente nuevamente.");
            }

            string motivoElegido = MOTIVOS_SALIDA[opcion - 1];

            if (motivoElegido == "Otro")
            {
                string detalle = LeerTextoNoVacio("Especifique el motivo: ", "El motivo no puede estar vacío.");
                return $"Otro: {detalle}";
            }

            return motivoElegido;
        }

        // =========================================================
        //  OPCIÓN 5: REGISTRAR VENTA
        // =========================================================

        public void RegistrarVenta()
        {
            Console.WriteLine("\n--- REGISTRAR VENTA ---");
            Console.WriteLine("(En cualquier momento puede escribir \"C\" para cancelar la venta)");

            if (!HayProductosRegistrados()) return;

            ListarProductos();

            // Selección de producto con posibilidad de cancelar
            Producto p = SeleccionarProductoExistenteOCancelar();
            if (p == null)
            {
                Console.WriteLine("\nVenta cancelada. No se realizó ningún cambio.");
                return;
            }

            // Cantidad con posibilidad de cancelar
            int? cantidadNullable = LeerCantidadVentaOCancelar(p);
            if (cantidadNullable == null)
            {
                Console.WriteLine("\nVenta cancelada. No se realizó ningún cambio.");
                return;
            }
            int cantidad = cantidadNullable.Value;

            decimal total = cantidad * p.Precio;

            // Confirmación final con posibilidad de cancelar
            Console.WriteLine($"\nResumen de la venta:");
            Console.WriteLine($"Producto : {p.Nombre} ({p.Codigo})");
            Console.WriteLine($"Cantidad : {cantidad} unidades");
            Console.WriteLine($"Total    : S/ {total:N2}");
            Console.Write("¿Confirma la venta? (S = Sí / C = Cancelar): ");

            string confirmacion;
            while (true)
            {
                confirmacion = Console.ReadLine()?.Trim().ToUpper();

                if (confirmacion == "S" || confirmacion == "C")
                    break;

                Console.Write("Opción inválida. Ingrese S para confirmar o C para cancelar: ");
            }

            if (confirmacion == "C")
            {
                Console.WriteLine("\nVenta cancelada. No se realizó ningún cambio.");
                return;
            }

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

            Console.WriteLine("\n{0,-20} {1,-25} {2,-10} {3,-12} {4,-8}",
                "Fecha", "Producto", "Cantidad", "Total", "Código");
            Console.WriteLine(new string('-', 85));

            decimal totalGeneral = 0;

            foreach (Venta v in ventas.OrderBy(v => v.Fecha))
            {
                Console.WriteLine("{0,-20} {1,-25} {2,-10} {3,-12} {4,-8}",
                    v.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    v.NombreProducto, v.Cantidad,
                    $"S/ {v.Total:N2}", v.CodigoProducto);

                totalGeneral += v.Total;
            }

            Console.WriteLine(new string('-', 85));
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
            if (p == null)
            {
                Console.WriteLine("\nOperación cancelada.");
                return;
            }

            Console.WriteLine($"\nEditando \"{p.Nombre}\" ({p.Codigo}).");
            Console.WriteLine("Deje en blanco y presione ENTER para mantener el valor actual.\n");

            // Nombre
            Console.Write($"Nombre actual [{p.Nombre}]: ");
            string nuevoNombre = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevoNombre))
            {
                nuevoNombre = nuevoNombre.Trim();
                if (EsSoloLetras(nuevoNombre))
                    p.Nombre = nuevoNombre;
                else
                    Console.WriteLine("Nombre inválido (solo letras). Se mantuvo el nombre anterior.");
            }

            // Categoría
            Console.Write($"Categoría actual [{p.Categoria}]: ");
            string nuevaCategoria = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(nuevaCategoria))
            {
                nuevaCategoria = nuevaCategoria.Trim();
                if (EsSoloLetras(nuevaCategoria))
                    p.Categoria = nuevaCategoria;
                else
                    Console.WriteLine("Categoría inválida (solo letras). Se mantuvo la categoría anterior.");
            }

            // Precio
            Console.Write($"Precio actual [S/ {p.Precio:N2}]: ");
            string entradaPrecio = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(entradaPrecio))
            {
                if (decimal.TryParse(entradaPrecio, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out decimal nuevoPrecio) && nuevoPrecio > 0)
                    p.Precio = nuevoPrecio;
                else
                    Console.WriteLine("Precio inválido (debe ser mayor a 0). Se mantuvo el precio anterior.");
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
            if (p == null)
            {
                Console.WriteLine("\nOperación cancelada.");
                return;
            }

            Console.Write($"\n¿Está seguro de eliminar \"{p.Nombre}\" ({p.Codigo})? Esta acción no se puede deshacer (S/N): ");
            string confirmacion = LeerSiNo();

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

            Console.WriteLine("\n{0,-20} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                "Fecha", "Código", "Producto", "Tipo", "Cantidad", "Stock final", "Motivo");
            Console.WriteLine(new string('-', 100));

            foreach (MovimientoStock m in movimientos.OrderBy(m => m.Fecha))
            {
                string tipoTexto = m.Tipo == TipoMovimiento.Entrada ? "Entrada" : "Salida";

                Console.WriteLine("{0,-20} {1,-8} {2,-22} {3,-9} {4,-9} {5,-12} {6}",
                    m.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    m.CodigoProducto, m.NombreProducto,
                    tipoTexto, m.Cantidad, m.StockResultante, m.Motivo);
            }

            Console.WriteLine(new string('-', 100));

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
        /// Se usa en Entrada, Salida, Modificar y Eliminar.
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
        /// Igual que SeleccionarProductoExistente, pero permite cancelar
        /// escribiendo "C" en cualquier momento (usado en RegistrarVenta).
        /// Retorna null si el usuario cancela.
        /// </summary>
        private Producto SeleccionarProductoExistenteOCancelar()
        {
            while (true)
            {
                Console.Write("Ingrese el código del producto (o presione C para cancelar): ");

                ConsoleKeyInfo primeraTecla = Console.ReadKey(intercept: true);

                if (char.ToUpper(primeraTecla.KeyChar) == 'C')
                {
                    Console.WriteLine("C");
                    return null;
                }

                string entrada = LeerCodigoConTeclado(primeraTecla);

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
        /// Lee la cantidad a vender, validando contra el stock disponible.
        /// Permite cancelar escribiendo "C". Retorna null si se cancela.
        /// </summary>
        private int? LeerCantidadVentaOCancelar(Producto p)
        {
            while (true)
            {
                Console.Write($"Cantidad vendida (stock disponible: {p.Stock}, o escriba C para cancelar): ");
                string entrada = Console.ReadLine()?.Trim();

                if (string.Equals(entrada, "C", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (!int.TryParse(entrada, out int cantidad) || cantidad <= 0)
                {
                    Console.WriteLine("Valor inválido. Ingrese un número entero mayor que cero, o C para cancelar.");
                    continue;
                }

                if (cantidad > p.Stock)
                {
                    Console.WriteLine($"Stock insuficiente. Stock actual: {p.Stock}. Ingrese una cantidad menor o igual, o C para cancelar.");
                    continue;
                }

                if (cantidad > UMBRAL_CANTIDAD_SOSPECHOSA)
                {
                    Console.Write($"La cantidad ingresada ({cantidad}) es inusualmente alta. ¿Confirma? (S/N): ");
                    string confirmacion = LeerSiNo();

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
        /// Lee el código de producto tecla por tecla.
        /// Formato obligatorio: 1 letra (A-Z) + 3 dígitos. Total: 4 caracteres.
        /// No permite errores de formato; admite Backspace para corregir.
        /// </summary>
        private string LeerCodigoConTeclado()
        {
            return LeerCodigoConTeclado(null);
        }

        /// <summary>
        /// Sobrecarga que permite pasar la primera tecla ya leída
        /// (usada cuando antes se comprobó si era "C" de cancelar).
        /// </summary>
        private string LeerCodigoConTeclado(ConsoleKeyInfo? primeraTeclaYaLeida)
        {
            char[] buffer = new char[4];
            int posicion = 0;
            bool esPrimeraIteracion = true;

            while (true)
            {
                ConsoleKeyInfo tecla;

                if (esPrimeraIteracion && primeraTeclaYaLeida.HasValue)
                {
                    tecla = primeraTeclaYaLeida.Value;
                }
                else
                {
                    tecla = Console.ReadKey(intercept: true);
                }
                esPrimeraIteracion = false;

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

        /// <summary>
        /// Verifica que el texto contenga SOLO letras (incluye tildes, ñ/Ñ)
        /// y espacios simples entre palabras. No permite números ni símbolos.
        /// </summary>
        private bool EsSoloLetras(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;

            // No se permite espacio al inicio/final ni espacios dobles
            if (texto.StartsWith(" ") || texto.EndsWith(" ") || texto.Contains("  "))
                return false;

            foreach (char c in texto)
            {
                bool esLetraValida = char.IsLetter(c) || c == ' ';
                if (!esLetraValida) return false;
            }
            // No permitir un mismo carácter repetido (AAAAA, ÑÑÑÑÑ, etc.)
            if (texto.Replace(" ", "").Distinct().Count() == 1)
                return false;
            return true;
        }

        /// <summary>
        /// Pide un texto que solo puede contener letras y espacios (sin números ni símbolos).
        /// Rechaza cadenas vacías, solo espacios, o con espacios al inicio/final/dobles.
        /// Usado para Nombre y Categoría.
        /// </summary>
        private string LeerTextoSoloLetras(string mensaje, string mensajeError)
        {
            while (true)
            {
                Console.Write(mensaje);
                string texto = Console.ReadLine();

                if (texto != null) texto = texto.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                {
                    Console.WriteLine(mensajeError);
                    continue;
                }

                if (!EsSoloLetras(texto))
                {
                    Console.WriteLine($"{mensajeError} Solo se permiten letras y espacios entre palabras (sin números ni símbolos).");
                    continue;
                }

                return texto;
            }
        }

        /// <summary>
        /// Pide texto libre obligatorio (no vacío, sin espacios en blanco únicamente).
        /// Usado para el detalle de "Otro" motivo de salida.
        /// </summary>
        private string LeerTextoNoVacio(string mensaje, string mensajeError)
        {
            string texto;
            do
            {
                Console.Write(mensaje);
                texto = Console.ReadLine();
                if (texto != null) texto = texto.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    Console.WriteLine(mensajeError);
            }
            while (string.IsNullOrWhiteSpace(texto));

            return texto;
        }

        /// <summary>Lee un decimal que debe ser ESTRICTAMENTE mayor a 0 (no se acepta 0 ni negativos).</summary>
        private decimal LeerDecimalPositivo(string mensaje)
        {
            decimal valor;
            while (true)
            {
                Console.Write(mensaje);
                string entrada = Console.ReadLine();

                if (decimal.TryParse(entrada, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out valor) && valor > 0)
                    return valor;
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
        /// Usado en Entrada y Salida de stock.
        /// </summary>
        private int LeerCantidadConConfirmacion(string mensaje)
        {
            while (true)
            {
                int cantidad = LeerEnteroPositivo(mensaje);

                if (cantidad > UMBRAL_CANTIDAD_SOSPECHOSA)
                {
                    Console.Write($"La cantidad ingresada ({cantidad}) es inusualmente alta. ¿Confirma? (S/N): ");
                    string confirmacion = LeerSiNo();

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
        /// Lee una respuesta S/N de forma estricta, sin aceptar vacíos
        /// ni valores distintos a S o N. Repite la pregunta hasta obtener una válida.
        /// </summary>
        private string LeerSiNo()
        {
            while (true)
            {
                string entrada = Console.ReadLine()?.Trim().ToUpper();
                if (entrada == "S" || entrada == "N")
                    return entrada;
                Console.Write("Respuesta inválida. Ingrese S (Sí) o N (No): ");
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