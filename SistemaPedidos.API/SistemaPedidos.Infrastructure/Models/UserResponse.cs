namespace SistemaPedidos.Infrastructure.Models
{
    /// <summary>
    /// Modelo de respuesta de JSONPlaceholder API para endpoint GET /users/{id}.
    /// Representa un usuario/cliente del servicio externo de validación.
    /// </summary>
    /// <remarks>
    /// API: https://jsonplaceholder.typicode.com/users/{id}
    /// IDs válidos: 1-10 (usuarios de prueba)
    /// 
    /// Usado en ValidacionExternaService para verificar existencia de clientes.
    /// Solo se utilizan Id, Name y Email para validación y logging.
    /// El resto de propiedades están mapeadas para compatibilidad con la API completa.
    /// </remarks>
    public class UserResponse
    {
        /// <summary>
        /// ID único del usuario. Debe ser > 0 y coincidir con el ID solicitado.
        /// Usado para validar integridad de la respuesta.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// Usado para logging y auditoría.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario (username).
        /// Mapeado para compatibilidad con API, no usado actualmente.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario.
        /// Usado para logging y auditoría.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Dirección completa del usuario (calle, ciudad, coordenadas).
        /// Mapeado para compatibilidad con API, no usado actualmente.
        /// </summary>
        public Address? Address { get; set; }

        /// <summary>
        /// Teléfono del usuario.
        /// Mapeado para compatibilidad con API, no usado actualmente.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Sitio web del usuario.
        /// Mapeado para compatibilidad con API, no usado actualmente.
        /// </summary>
        public string Website { get; set; } = string.Empty;

        /// <summary>
        /// Información de la compañía del usuario.
        /// Mapeado para compatibilidad con API, no usado actualmente.
        /// </summary>
        public Company? Company { get; set; }
    }

    /// <summary>
    /// Dirección física del usuario retornada por JSONPlaceholder API.
    /// Incluye calle, ciudad, código postal y coordenadas geográficas.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Nombre de la calle.
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// Número de suite/departamento.
        /// </summary>
        public string Suite { get; set; } = string.Empty;

        /// <summary>
        /// Ciudad.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Código postal.
        /// </summary>
        public string Zipcode { get; set; } = string.Empty;

        /// <summary>
        /// Coordenadas geográficas (latitud y longitud).
        /// </summary>
        public Geo? Geo { get; set; }
    }

    /// <summary>
    /// Coordenadas geográficas (latitud y longitud) de la dirección.
    /// </summary>
    public class Geo
    {
        /// <summary>
        /// Latitud en formato string.
        /// </summary>
        public string Lat { get; set; } = string.Empty;

        /// <summary>
        /// Longitud en formato string.
        /// </summary>
        public string Lng { get; set; } = string.Empty;
    }

    /// <summary>
    /// Información de la compañía del usuario retornada por JSONPlaceholder API.
    /// </summary>
    public class Company
    {
        /// <summary>
        /// Nombre de la compañía.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Frase comercial (slogan) de la compañía.
        /// </summary>
        public string CatchPhrase { get; set; } = string.Empty;

        /// <summary>
        /// Business strategy (jerga de negocios).
        /// </summary>
        public string Bs { get; set; } = string.Empty;
    }
}