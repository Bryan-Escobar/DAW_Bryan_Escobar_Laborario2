using Ejercicio_Generación_Etiquetas.Models;

namespace Ejercicio_Generación_Etiquetas.Repositories
{
    public class EtiquetaRepository : IEtiquetaRepository
    {
        private readonly List<EtiquetaProduccion> _etiquetas;
        private int _nextId = 1;

        private static readonly string[] Modelos = ["Camisa", "Pantalón", "Chaqueta", "Vestido", "Sudadera"];
        private static readonly string[] Colores = ["Negro", "Blanco", "Azul", "Rojo", "Verde"];
        private static readonly string[] Tallas = ["S", "M", "L", "XL"];
        private static readonly string[] Impresoras = ["Impresora-01", "Impresora-02", "Impresora-03"];
        private static readonly string[] Usuarios = ["jperez", "mgarcia", "lrojas", "acastro", "svega"];

        public EtiquetaRepository()
        {
            _etiquetas = GenerarSemilla();
            _nextId = _etiquetas.Count + 1;
        }

        public IEnumerable<EtiquetaProduccion> ObtenerTodas() =>
            _etiquetas.OrderByDescending(e => e.FechaSolicitud);

        public EtiquetaProduccion? ObtenerPorId(int id) =>
            _etiquetas.FirstOrDefault(e => e.Id == id);

        public void Agregar(EtiquetaProduccion etiqueta)
        {
            ValidarNumeroEtiquetaUnico(etiqueta.NumeroEtiqueta);
            ValidarCantidades(etiqueta.CantidadSolicitada, etiqueta.CantidadGenerada);

            etiqueta.Id = _nextId++;
            etiqueta.FechaSolicitud = DateTime.Now;
            AplicarEstadoPorCantidad(etiqueta);

            _etiquetas.Add(etiqueta);
        }

        public void Actualizar(EtiquetaProduccion etiqueta)
        {
            var existente = ObtenerPorId(etiqueta.Id)
                ?? throw new InvalidOperationException("La etiqueta no existe.");

            ValidarNumeroEtiquetaUnico(etiqueta.NumeroEtiqueta, etiqueta.Id);
            ValidarCantidades(etiqueta.CantidadSolicitada, etiqueta.CantidadGenerada);

            if (existente.Estado == EstadoEtiqueta.Cancelada &&
                etiqueta.Estado == EstadoEtiqueta.Procesando)
            {
                throw new InvalidOperationException("No se puede pasar una etiqueta cancelada a Procesando.");
            }

            existente.NumeroEtiqueta = etiqueta.NumeroEtiqueta;
            existente.Orden = etiqueta.Orden;
            existente.Modelo = etiqueta.Modelo;
            existente.Color = etiqueta.Color;
            existente.Talla = etiqueta.Talla;
            existente.Usuario = etiqueta.Usuario;
            existente.Impresora = etiqueta.Impresora;
            existente.CantidadSolicitada = etiqueta.CantidadSolicitada;
            existente.CantidadGenerada = etiqueta.CantidadGenerada;
            existente.Prioridad = etiqueta.Prioridad;
            existente.Estado = etiqueta.Estado;

            AplicarEstadoPorCantidad(existente);
        }

        public bool Eliminar(int id)
        {
            var etiqueta = ObtenerPorId(id);
            if (etiqueta is null)
            {
                return false;
            }

            if (etiqueta.Estado == EstadoEtiqueta.Impresa)
            {
                return false;
            }

            _etiquetas.Remove(etiqueta);
            return true;
        }

        public IEnumerable<EtiquetaProduccion> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return ObtenerTodas();
            }

            termino = termino.Trim();

            return _etiquetas
                .Where(e =>
                    e.Orden.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    e.Modelo.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    e.Usuario.Contains(termino, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.FechaSolicitud);
        }

        public IEnumerable<EtiquetaProduccion> Filtrar(
            EstadoEtiqueta? estado,
            PrioridadEtiqueta? prioridad,
            string? impresora)
        {
            var query = _etiquetas.AsEnumerable();

            if (estado.HasValue)
            {
                query = query.Where(e => e.Estado == estado.Value);
            }

            if (prioridad.HasValue)
            {
                query = query.Where(e => e.Prioridad == prioridad.Value);
            }

            if (!string.IsNullOrWhiteSpace(impresora))
            {
                query = query.Where(e =>
                    e.Impresora.Equals(impresora, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(e => e.FechaSolicitud);
        }

        public void GenerarEtiquetas(int id, int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new InvalidOperationException("La cantidad a generar debe ser mayor a cero.");
            }

            var etiqueta = ObtenerPorId(id)
                ?? throw new InvalidOperationException("La etiqueta no existe.");

            if (etiqueta.Estado == EstadoEtiqueta.Cancelada)
            {
                throw new InvalidOperationException("No se puede reimprimir una etiqueta cancelada.");
            }

            if (etiqueta.Estado == EstadoEtiqueta.Impresa)
            {
                throw new InvalidOperationException("La etiqueta ya fue impresa completamente.");
            }

            if (etiqueta.CantidadGenerada + cantidad > etiqueta.CantidadSolicitada)
            {
                throw new InvalidOperationException("La cantidad generada no puede superar la solicitada.");
            }

            etiqueta.CantidadGenerada += cantidad;

            if (etiqueta.CantidadGenerada < etiqueta.CantidadSolicitada)
            {
                etiqueta.Estado = EstadoEtiqueta.Procesando;
            }

            AplicarEstadoPorCantidad(etiqueta);
        }

        public void CambiarEstado(int id, EstadoEtiqueta nuevoEstado)
        {
            var etiqueta = ObtenerPorId(id)
                ?? throw new InvalidOperationException("La etiqueta no existe.");

            if (etiqueta.Estado == EstadoEtiqueta.Cancelada &&
                nuevoEstado == EstadoEtiqueta.Procesando)
            {
                throw new InvalidOperationException("No se puede pasar una etiqueta cancelada a Procesando.");
            }

            if (etiqueta.Estado == EstadoEtiqueta.Cancelada &&
                nuevoEstado == EstadoEtiqueta.Impresa)
            {
                throw new InvalidOperationException("No se puede reimprimir una etiqueta cancelada.");
            }

            etiqueta.Estado = nuevoEstado;
            AplicarEstadoPorCantidad(etiqueta);
        }

        public void Cancelar(int id)
        {
            var etiqueta = ObtenerPorId(id)
                ?? throw new InvalidOperationException("La etiqueta no existe.");

            if (etiqueta.Estado == EstadoEtiqueta.Impresa)
            {
                throw new InvalidOperationException("No se puede cancelar una etiqueta ya impresa.");
            }

            etiqueta.Estado = EstadoEtiqueta.Cancelada;
        }

        public double CalcularAvanceGlobal()
        {
            if (_etiquetas.Count == 0)
            {
                return 0;
            }

            return _etiquetas.Average(e => e.PorcentajeAvance);
        }

        private void ValidarNumeroEtiquetaUnico(string numeroEtiqueta, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(numeroEtiqueta))
            {
                throw new InvalidOperationException("El número de etiqueta es obligatorio.");
            }

            if (_etiquetas.Any(e =>
                    e.Id != idExcluir &&
                    e.NumeroEtiqueta.Equals(numeroEtiqueta, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Ya existe una etiqueta con ese número.");
            }
        }

        private static void ValidarCantidades(int cantidadSolicitada, int cantidadGenerada)
        {
            if (cantidadSolicitada <= 0)
            {
                throw new InvalidOperationException("La cantidad solicitada debe ser mayor a cero.");
            }

            if (cantidadGenerada > cantidadSolicitada)
            {
                throw new InvalidOperationException("La cantidad generada no puede superar la solicitada.");
            }
        }

        private static void AplicarEstadoPorCantidad(EtiquetaProduccion etiqueta)
        {
            if (etiqueta.Estado == EstadoEtiqueta.Cancelada ||
                etiqueta.Estado == EstadoEtiqueta.Error)
            {
                return;
            }

            if (etiqueta.CantidadGenerada == etiqueta.CantidadSolicitada)
            {
                etiqueta.Estado = EstadoEtiqueta.Impresa;
            }
        }

        private static List<EtiquetaProduccion> GenerarSemilla()
        {
            var random = new Random(42);
            var etiquetas = new List<EtiquetaProduccion>();
            var distribucionEstados = new List<EstadoEtiqueta>();

            distribucionEstados.AddRange(Enumerable.Repeat(EstadoEtiqueta.Impresa, 30));
            distribucionEstados.AddRange(Enumerable.Repeat(EstadoEtiqueta.Procesando, 10));
            distribucionEstados.AddRange(Enumerable.Repeat(EstadoEtiqueta.Pendiente, 5));
            distribucionEstados.AddRange(Enumerable.Repeat(EstadoEtiqueta.Error, 3));
            distribucionEstados.AddRange(Enumerable.Repeat(EstadoEtiqueta.Cancelada, 2));

            for (var i = 1; i <= 50; i++)
            {
                var estado = distribucionEstados[i - 1];
                var cantidadSolicitada = random.Next(10, 101);
                var cantidadGenerada = estado switch
                {
                    EstadoEtiqueta.Impresa => cantidadSolicitada,
                    EstadoEtiqueta.Procesando => random.Next(1, cantidadSolicitada),
                    EstadoEtiqueta.Pendiente => 0,
                    EstadoEtiqueta.Error => random.Next(0, cantidadSolicitada),
                    EstadoEtiqueta.Cancelada => random.Next(0, cantidadSolicitada),
                    _ => 0
                };

                etiquetas.Add(new EtiquetaProduccion
                {
                    Id = i,
                    NumeroEtiqueta = $"ETQ-{i:D4}",
                    Orden = $"ORD-{random.Next(1000, 9999)}",
                    Modelo = Modelos[random.Next(Modelos.Length)],
                    Color = Colores[random.Next(Colores.Length)],
                    Talla = Tallas[random.Next(Tallas.Length)],
                    Usuario = Usuarios[random.Next(Usuarios.Length)],
                    Impresora = Impresoras[random.Next(Impresoras.Length)],
                    CantidadSolicitada = cantidadSolicitada,
                    CantidadGenerada = cantidadGenerada,
                    Estado = estado,
                    Prioridad = (PrioridadEtiqueta)random.Next(3),
                    FechaSolicitud = DateTime.Now.AddHours(-random.Next(1, 72))
                });
            }

            return etiquetas;
        }
    }
}
