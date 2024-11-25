using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nostromo.Server.API.Models;
using Nostromo.Server.Settings;
using System.Net.Http.Json;  // Add this for GetFromJsonAsync
using Nostromo.Server.Utilities;



//namespace Nostromo.Server.Scheduling.Jobs
//{
//    class MetadataJob : BaseJob
//    {
//        private readonly HttpClient _httpClient;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly ISettingsProvider _settingsProvider;
//        public const string MOVIE_ID_KEY = "MovieId";
     
//        public override string RemoteURL => "https://image.tmdb.org/t/p/original";

//        public MetadataJob(
//           ILogger<MetadataJob> logger,
//           IHttpClientFactory httpClientFactory,
//           ISettingsProvider settingsProvider,
//           IHttpContextAccessor httpContextAccessor)
//           : base(logger)
//        {
//            _httpClient = httpClientFactory.CreateClient();
//            _settingsProvider = settingsProvider;
//            _httpContextAccessor = httpContextAccessor;
//        }

//        public override string Name => "Metadata job";
//        public override string Type => "FileProcessing";

//        public override async Task ProcessJob()
//        {
//            try
//            {
                
//            }
//        }


//}
