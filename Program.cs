using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiendaProductosDama.Biblioteca;

namespace Grupo_Flor_Yaneth_Importaciones_SRL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Objeto de la biblioteca: contiene los datos y todas las funciones
            GestorTienda gestor = new GestorTienda();

            int opcion; // Variable para guardar opción del menú

            do
            {
                Console.Clear(); // Limpia pantalla

                // Título del sistema
                Console.WriteLine("======================================");
                Console.WriteLine("  Grupo_Flor_Yaneth_Importaciones_SRL - CAJAMARCA");
                Console.WriteLine("======================================");

                // Opciones del menú
                Console.WriteLine("1. Registrar producto");
                Console.WriteLine("2. Listar productos");
                Console.WriteLine("3. Registrar entrada de stock");
                Console.WriteLine("4. Registrar salida de stock");
                Console.WriteLine("5. Registrar venta");
                Console.WriteLine("6. Ver reporte de ventas");
                Console.WriteLine("7. Salir");

                Console.Write("Seleccione una opción: ");

                // Lee opción ingresada
                // Se añade validación para la entrada de la opción del menú
                while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < 1 || opcion > 7)
                {
                    Console.WriteLine("Opción inválida. Por favor, ingrese un número entre 1 y 7.");
                    Console.Write("Seleccione una opción: ");
                }

                // Evalúa la opción elegida
                switch (opcion)
                {
                    case 1:
                        gestor.RegistrarProducto(); // Llama función registrar producto
                        break;

                    case 2:
                        gestor.ListarProductos(); // Muestra productos
                        break;

                    case 3:
                        gestor.EntradaStock(); // Aumenta stock
                        break;

                    case 4:
                        gestor.SalidaStock(); // Reduce stock
                        break;

                    case 5:
                        gestor.RegistrarVenta(); // Registra venta
                        break;

                    case 6:
                        gestor.ReporteVentas(); // Muestra reporte
                        break;

                    case 7:
                        Console.WriteLine("Gracias por usar el sistema.");
                        break;

                    default:
                        Console.WriteLine("Opción incorrecta.");
                        break;
                }

                Console.WriteLine("\nPresione ENTER para continuar...");
                Console.ReadLine();

            } while (opcion != 7); // Repite hasta elegir salir
        }
    }
    
}
