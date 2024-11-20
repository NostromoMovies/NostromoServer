using System.ComponentModel.DataAnnotations;
<<<<<<< HEAD
=======
using NHibernate.Linq.Functions;
using NHibernate.Mapping;
>>>>>>> fea4bb2 (TmdbController/ServerSettings update)

namespace Nostromo.Server.Settings
{
    public class ServerSettings : IServerSettings
    {
        private string _imagesPath;

        public string ImagesPath
        {
            get => _imagesPath;
            set
            {
                _imagesPath = value;
            }
        }

        [Range(1, 65535, ErrorMessage = "Server Port must be between 1 and 65535")]
        public ushort ServerPort { get; set; } = 8112;
<<<<<<< HEAD
=======
        public string TmdbApiKey { get; set; } = "cbd64d95c4c66beed284bd12701769ec";
>>>>>>> fea4bb2 (TmdbController/ServerSettings update)
        public bool FirstRun { get; set; } = true;
    }
}
