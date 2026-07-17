using Ejercicio_Generación_Etiquetas.Models;

namespace Ejercicio_Generación_Etiquetas.Repositories
{
    public interface IEtiquetaRepository
    {
        IEnumerable<EtiquetaProduccion> ObtenerTodas();
        EtiquetaProduccion? ObtenerPorId(int id);
        void Agregar(EtiquetaProduccion etiqueta);
        void Actualizar(EtiquetaProduccion etiqueta);
        bool Eliminar(int id);
        IEnumerable<EtiquetaProduccion> Buscar(string termino);
        IEnumerable<EtiquetaProduccion> Filtrar(EstadoEtiqueta? estado, PrioridadEtiqueta? prioridad, string? impresora);
        void GenerarEtiquetas(int id, int cantidad);
        void CambiarEstado(int id, EstadoEtiqueta nuevoEstado);
        void Cancelar(int id);
        double CalcularAvanceGlobal();
    }
}
