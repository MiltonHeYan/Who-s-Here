using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace WhosHereServer.Data
{
    public class UserPos
    {
        [Key]
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public Point? Pos { get; set; }
    }
}
