using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TiendaProductosDama.Biblioteca
{
    public class Producto // Clase para guardar datos de productos
    {
        public int Codigo { get; set; } // Guarda el código del producto
        public string Nombre { get; set; } = ""; // Guarda nombre del producto
        public string Categoria { get; set; } = ""; // Guarda categoría
        public decimal Precio { get; set; } // Guarda precio
        public int Stock { get; set; } // Guarda cantidad disponible
    }

    public class Venta // Clase para guardar ventas realizadas
    {
        public int CodigoProducto { get; set; } // Código del producto vendido
        public string NombreProducto { get; set; } = ""; // Nombre del producto
        public int Cantidad { get; set; } // Cantidad vendida
        public decimal Total { get; set; } // Total pagado
        public DateTime Fecha { get; set; } // Fecha de venta
    }

    // Clase "biblioteca": contiene los datos y todas las funciones del sistema
    public class GestorTienda
    {
        // Lista donde se almacenan los productos
        public List<Producto> productos = new List<Producto>();

        // Lista donde se almacenan las ventas
        public List<Venta> ventas = new List<Venta>();

        // Función para registrar productos
        public void RegistrarProducto()
        {
            Producto p = new Producto(); // Crea nuevo producto

            Console.WriteLine("\n--- REGISTRAR PRODUCTO ---");

            // Validación para el código del producto
            int codigo;
            while (true)
            {
                Console.Write("Código del producto: ");
                if (int.TryParse(Console.ReadLine(), out codigo) && codigo > 0)
                {
                    if (productos.Any(prod => prod.Codigo == codigo))
                    {
                        Console.WriteLine("Ya existe un producto con este código. Por favor, ingrese uno diferente.");
                    }
                    else
                    {
                        p.Codigo = codigo;
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Código inválido. Por favor, ingrese un número entero positivo.");
                }
            }

            Console.Write("Nombre del producto: ");
            p.Nombre = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(p.Nombre))
            {
                Console.WriteLine("El nombre del producto no puede estar vacío.");
                Console.Write("Nombre del producto: ");
                p.Nombre = Console.ReadLine();
            }

            Console.Write("Categoría: ");
            p.Categoria = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(p.Categoria))
            {
                Console.WriteLine("La categoría del producto no puede estar vacía.");
                Console.Write("Categoría: ");
                p.Categoria = Console.ReadLine();
            }

            // Validación para el precio del producto
            decimal precio;
            while (true)
            {
                Console.Write("Precio: S/ ");
                if (decimal.TryParse(Console.ReadLine(), NumberStyles.Currency, CultureInfo.CurrentCulture, out precio) && precio >= 0)
                {
                    p.Precio = precio;
                    break;
                }
                else
                {
                    Console.WriteLine("Precio inválido. Por favor, ingrese un valor numérico no negativo.");
                }
            }

            // Validación para el stock inicial
            int stock;
            while (true)
            {
                Console.Write("Stock inicial: ");
                if (int.TryParse(Console.ReadLine(), out stock) && stock >= 0)
                {
                    p.Stock = stock;
                    break;
                }
                else
                {
                    Console.WriteLine("Stock inicial inválido. Por favor, ingrese un número entero no negativo.");
                }
            }

            productos.Add(p); // Guarda producto en la lista

            Console.WriteLine("Producto registrado correctamente.");
        }

        // Función para mostrar productos
        public void ListarProductos()
        {
            Console.WriteLine("\n--- LISTA DE PRODUCTOS ---");

            if (productos.Count == 0)
            {
                Console.WriteLine("No hay productos registrados.");
                return;
            }

            foreach (Producto p in productos)
            {
                Console.WriteLine($"Código: {p.Codigo} | Nombre: {p.Nombre} | Categoría: {p.Categoria} | Precio: S/ {p.Precio:N2} | Stock: {p.Stock}");
            }
        }
        // Función para eliminar producto
        public void EliminarProducto()
        {
            Console.WriteLine("\n--- ELIMINAR PRODUCTO ---");

            if (productos.Count == 0)
            {
                Console.WriteLine("No hay productos registrados.");
                return;
            }

            int codigo;
            while (true)
            {
                Console.Write("Ingrese el código del producto a eliminar: ");

                if (int.TryParse(Console.ReadLine(), out codigo) && codigo > 0)
                {
                    Producto p = productos.FirstOrDefault(x => x.Codigo == codigo);

                    if (p == null)
                    {
                        Console.WriteLine("Producto no encontrado.");
                    }
                    else
                    {
                        Console.WriteLine($"Producto encontrado: {p.Nombre}");
                        Console.Write("¿Está seguro de eliminarlo? (S/N): ");
                        string respuesta = Console.ReadLine().ToUpper();

                        if (respuesta == "S")
                        {
                            productos.Remove(p);
                            Console.WriteLine("Producto eliminado correctamente.");
                        }
                        else
                        {
                            Console.WriteLine("Operación cancelada.");
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Código inválido. Ingrese un número entero positivo.");
                }
            }
        }

        // Función para agregar stock
        public void EntradaStock()
        {
            Console.WriteLine("\n--- ENTRADA DE STOCK ---");

            int codigo;
            Producto p = null;
            while (true)
            {
                Console.Write("Ingrese código del producto: ");
                if (int.TryParse(Console.ReadLine(), out codigo) && codigo > 0)
                {
                    p = productos.FirstOrDefault(x => x.Codigo == codigo);
                    if (p == null)
                    {
                        Console.WriteLine("Producto no encontrado. Por favor, intente con otro código.");
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Código inválido. Por favor, ingrese un número entero positivo.");
                }
            }

            int cantidad;
            while (true)
            {
                Console.Write("Cantidad que ingresa: ");
                if (int.TryParse(Console.ReadLine(), out cantidad) && cantidad > 0)
                {
                    p.Stock += cantidad; // Suma stock
                    Console.WriteLine("Stock actualizado correctamente.");
                    break;
                }
                else
                {
                    Console.WriteLine("Cantidad inválida. Por favor, ingrese un número entero positivo.");
                }
            }
        }

        // Función para disminuir stock
        public void SalidaStock()
        {
            Console.WriteLine("\n--- SALIDA DE STOCK ---");

            int codigo;
            Producto p = null;
            while (true)
            {
                Console.Write("Ingrese código del producto: ");
                if (int.TryParse(Console.ReadLine(), out codigo) && codigo > 0)
                {
                    p = productos.FirstOrDefault(x => x.Codigo == codigo);
                    if (p == null)
                    {
                        Console.WriteLine("Producto no encontrado. Por favor, intente con otro código.");
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Código inválido. Por favor, ingrese un número entero positivo.");
                }
            }

            int cantidad;
            while (true)
            {
                Console.Write("Cantidad que sale: ");
                if (int.TryParse(Console.ReadLine(), out cantidad) && cantidad > 0)
                {
                    if (cantidad > p.Stock)
                    {
                        Console.WriteLine($"No hay suficiente stock. Stock actual: {p.Stock}. Por favor, ingrese una cantidad menor o igual.");
                    }
                    else
                    {
                        p.Stock -= cantidad; // Resta stock
                        Console.WriteLine("Salida registrada correctamente.");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Cantidad inválida. Por favor, ingrese un número entero positivo.");
                }
            }
        }

        // Función para registrar ventas
        public void RegistrarVenta()
        {
            Console.WriteLine("\n--- REGISTRAR VENTA ---");

            int codigo;
            Producto p = null;
            while (true)
            {
                Console.Write("Ingrese código del producto: ");
                if (int.TryParse(Console.ReadLine(), out codigo) && codigo > 0)
                {
                    p = productos.FirstOrDefault(x => x.Codigo == codigo);
                    if (p == null)
                    {
                        Console.WriteLine("Producto no encontrado. Por favor, intente con otro código.");
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Código inválido. Por favor, ingrese un número entero positivo.");
                }
            }

            int cantidad;
            while (true)
            {
                Console.Write("Cantidad vendida: ");
                if (int.TryParse(Console.ReadLine(), out cantidad) && cantidad > 0)
                {
                    if (cantidad > p.Stock)
                    {
                        Console.WriteLine($"No hay stock suficiente. Stock actual: {p.Stock}. Por favor, ingrese una cantidad menor o igual.");
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Cantidad inválida. Por favor, ingrese un número entero positivo.");
                }
            }

            // Calcula total de venta
            decimal total = cantidad * p.Precio;

            p.Stock -= cantidad; // Reduce stock después de vender

            Venta v = new Venta(); // Crea nueva venta

            v.CodigoProducto = p.Codigo;
            v.NombreProducto = p.Nombre;
            v.Cantidad = cantidad;
            v.Total = total;
            v.Fecha = DateTime.Now; // Guarda fecha actual

            ventas.Add(v); // Guarda venta

            Console.WriteLine($"Venta registrada correctamente. Total: S/ {total:N2}");
        }

        // Función para mostrar ventas
        public void ReporteVentas()
        {
            Console.WriteLine("\n--- REPORTE DE VENTAS ---");

            if (ventas.Count == 0)
            {
                Console.WriteLine("No hay ventas registradas.");
                return;
            }

            decimal totalGeneral = 0; // Acumula total vendido

            Console.WriteLine("\n{0,-15} {1,-25} {2,-10} {3,-15} {4,-20}", "Fecha", "Producto", "Cantidad", "Total", "Código Producto");
            Console.WriteLine("{0,-15} {1,-25} {2,-10} {3,-15} {4,-20}", "-----", "--------", "--------", "-----", "---------------");

            foreach (Venta v in ventas.OrderBy(v => v.Fecha))
            {
                Console.WriteLine($"{v.Fecha.ToString("dd/MM/yyyy HH:mm"),-15} {v.NombreProducto,-25} {v.Cantidad,-10} {v.Total.ToString("N2"),-15} {v.CodigoProducto,-20}");
                totalGeneral += v.Total; // Suma ventas
            }

            Console.WriteLine("--------------------------------------------------------------------------------");

            // Muestra total vendido
            Console.WriteLine($"Total vendido: S/ {totalGeneral:N2}");
        }


    }
}