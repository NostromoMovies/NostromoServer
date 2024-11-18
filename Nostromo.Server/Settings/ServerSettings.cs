using System.ComponentModel.DataAnnotations;

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
        public bool FirstRun { get; set; } = true;
    }
}
