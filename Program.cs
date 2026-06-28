using System;
using TiendaProductosDama.Biblioteca;

namespace Grupo_Flor_Yaneth_Importaciones_SRL
{
    class Program
    {
        static void Main(string[] args)
        {
            // Crea el gestor: contiene todos los datos y funciones del sistema
            GestorTienda gestor = new GestorTienda();

            int opcion;

            do
            {
                Console.Clear();

                Console.WriteLine("==========================================");
                Console.WriteLine("  GRUPO FLOR YANETH IMPORTACIONES SRL");
                Console.WriteLine("           CAJAMARCA - PERÚ");
                Console.WriteLine("==========================================");
                Console.WriteLine("  1. Registrar producto");
                Console.WriteLine("  2. Listar productos");
                Console.WriteLine("  3. Registrar entrada de stock");
                Console.WriteLine("  4. Registrar salida de stock");
                Console.WriteLine("  5. Registrar venta");
                Console.WriteLine("  6. Ver reporte de ventas");
                Console.WriteLine("  7. Modificar producto");
                Console.WriteLine("  8. Eliminar producto");
                Console.WriteLine("  9. Historial de movimientos de stock");
                Console.WriteLine(" 10. Salir");
                Console.WriteLine("==========================================");
                Console.Write("Seleccione una opción: ");

                // Validación de la opción del menú
                while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > 10)
                {
                    Console.WriteLine("Opción inválida. Ingrese un número entre 1 y 10.");
                    Console.Write("Seleccione una opción: ");
                }
                try{
                    // Despacha la opción elegida al gestor
                    switch (opcion)
                    {
                        case 1: gestor.RegistrarProducto(); break;
                        case 2: gestor.ListarProductos(); break;
                        case 3: gestor.EntradaStock(); break;
                        case 4: gestor.SalidaStock(); break;
                        case 5: gestor.RegistrarVenta(); break;
                        case 6: gestor.ReporteVentas(); break;
                        case 7: gestor.ModificarProducto(); break;
                        case 8: gestor.EliminarProducto(); break;
                        case 9: gestor.HistorialMovimientos(); break;
                        case 10:
                            Console.WriteLine("\nGracias por usar el sistema. ¡Hasta pronto!");
                            break;
                    }

                }
                catch (VolverAlMenuException)
                {
                    Console.WriteLine("\nRegresando al menú principal...");
                }


                if (opcion != 10)
                {
                    Console.WriteLine("\nPresione ENTER para continuar...");
                    Console.ReadLine();
                }

            } while (opcion != 10);
        }
    }
}
