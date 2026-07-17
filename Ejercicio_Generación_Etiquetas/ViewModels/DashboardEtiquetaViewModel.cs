namespace Ejercicio_Generación_Etiquetas.ViewModels
{
    public class DashboardEtiquetaViewModel
    {
        public int TotalEtiquetas { get; set; }
        public int Pendientes { get; set; }
        public int Procesando { get; set; }
        public int Impresas { get; set; }
        public int Errores { get; set; }
        public int Canceladas { get; set; }
        public int ProduccionHoy { get; set; }
        public double PorcentajeGlobal { get; set; }
        public string FechaServidor { get; set; } = string.Empty;
        public List<EtiquetaViewModel> UltimasSolicitudes { get; set; } = new();
    }
}
