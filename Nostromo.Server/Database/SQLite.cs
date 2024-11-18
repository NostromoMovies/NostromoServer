using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using Microsoft.Data.Sqlite;
using NHibernate;
using System.Reflection;

namespace Nostromo.Server.Database
{
    public class SQLite: BaseDatabase<SqliteConnection>
    {
        private static string _databasePath;
        public static string DatabasePath
        {
            get
            {
                if (_databasePath != null)
                    return _databasePath;

                var directoryPath = ""; // TODO: provide from settings

                return directoryPath;
            }
        }

        private static string GetDatabaseFilePath()
        {
            throw new NotImplementedException();
        }

        public override string Name => throw new NotImplementedException();

        public override int RequiredVersion => throw new NotImplementedException();

        public override void BackupDatabase(string filename)
        {
            throw new NotImplementedException();
        }

        public override void CreateAndUpdateSchema()
        {
            throw new NotImplementedException();
        }

        public override void CreateDatabase()
        {
            throw new NotImplementedException();
        }

        public override ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard
                .ConnectionString("Data Source=mydatabase.db;"))
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .BuildSessionFactory();
        }

        public override bool DBExists()
        {
            return File.Exists(GetDatabaseFilePath());
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void PopulateInitialData()
        {
            throw new NotImplementedException();
        }

        public override bool TestConnection()
        {
            throw new NotImplementedException();
        }

        protected override void Execute(SqliteConnection connection, string command)
        {
            using var sqCommand = new SqliteCommand(command, connection);
            sqCommand.CommandTimeout = 0;
            sqCommand.ExecuteNonQuery();
        }

        private List<DatabaseCommand> createTables = new()
        {
            new DatabaseCommand(@"
                CREATE TABLE IF NOT EXISTS Movie(
                    PosterPath      TEXT,
                    Adult           BOOLEAN,
                    Overview        TEXT,
                    ReleaseDate     VARCHAR(255),
                    MovieID         INT PRIMARY KEY,
                    OriginalTitle   VARCHAR(255),
                    Title           VARCHAR(255),
                    BackdropPath    TEXT,
                    Popularity      DECIMAL(3, 2),
                    VoteCount       INT,
                    Video           BOOLEAN,
                    VoteAverage     DECIMAL(3, 2),
                    Runtime         INT
                )
            "),

            new DatabaseCommand(@"
                CREATE TABLE IF NOT EXISTS Genre(
                    GenreID         INT PRIMARY KEY,
                    Name            VARCHAR(255)
                )
            "),

            new DatabaseCommand(@"
                CREATE TABLE IF NOT EXISTS MovieGenre(
                    MovieID         INT,
                    GenreID         INT,
                    PRIMARY KEY (MovieID, GenreID),
                    FOREIGN KEY (MovieID) REFERENCES Movie(MovieID),
                    FOREIGN KEY (GenreID) REFERENCES Genre(GenreID)
                )
            "),

            new DatabaseCommand(@"
                CREATE TABLE IF NOT EXISTS userTable(
                    UserID        INT PRIMARY KEY AUTO_INCREMENT,
                    username VARCHAR(50) NOT NULL,
                    password VARCHAR(72) NOT NULL,
                    first_name VARCHAR(72) NOT NULL,
                    last_name VARCHAR(72) NOT NULL,
                    salt VARCHAR(72) NOT NULL

                )
            ")
        };

        //private List<DatabaseCommand> 
    }
}
