using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.Net;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite; // used for Sqlite
using Nostromo.Models;


public class DatabaseService
{
    private readonly string _connectionString = "Data Source=nostromo.db";

    public DatabaseService()
    {
        using (var conn = new SqliteConnection(_connectionString))
        {
            conn.Open();

            string createMovieTableQuery = @"
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
            ";

            string createGenreTableQuery = @"
                CREATE TABLE IF NOT EXISTS Genre(
                    GenreID         INT PRIMARY KEY,
                    Name            VARCHAR(255)
                )
            ";

            string createMovieGenreTableQuery = @"
                CREATE TABLE IF NOT EXISTS MovieGenre(
                    MovieID         INT,
                    GenreID         INT,
                    PRIMARY KEY (MovieID, GenreID),
                    FOREIGN KEY (MovieID) REFERENCES Movie(MovieID),
                    FOREIGN KEY (GenreID) REFERENCES Genre(GenreID)
                )
            ";
            string createUserTable = @"
                CREATE TABLE IF NOT EXISTS userTable(
                    UserID        INT PRIMARY KEY AUTO_INCREMENT,
                    username VARCHAR(50) NOT NULL,
                    password VARCHAR(72) NOT NULL,
                    first_name VARCHAR(72) NOT NULL,
                    last_name VARCHAR(72) NOT NULL,
                    salt VARCHAR(72) NOT NULL

                )
            ";

            using (var command = new SqliteCommand(createMovieTableQuery, conn))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SqliteCommand(createGenreTableQuery, conn))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SqliteCommand(createMovieGenreTableQuery, conn))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SqliteCommand(createUserTable, conn))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void InsertMovie(TmdbMovie movie)
    {
        using (var conn = new SqliteConnection(_connectionString))
        {
            conn.Open();

            string insertMovieQuery = @"
                INSERT OR REPLACE INTO Movie(
                    PosterPath,
                    Adult,
                    Overview,
                    ReleaseDate,
                    MovieID,
                    OriginalTitle,
                    Title,
                    BackdropPath,
                    Popularity,
                    VoteCount,
                    Video,
                    VoteAverage,
                    Runtime
                ) VALUES (
                    @PosterPath, @Adult, @Overview, @ReleaseDate, @MovieId, @OriginalTitle, 
                    @Title, @BackdropPath, @Popularity, @VoteCount, @Video, @VoteAverage, @Runtime
                )
            ";

            using (var command = new SqliteCommand(insertMovieQuery, conn))
            {
                command.Parameters.AddWithValue("@PosterPath", movie.posterPath);
                command.Parameters.AddWithValue("@Adult", movie.adult);
                command.Parameters.AddWithValue("@Overview", movie.overview);
                command.Parameters.AddWithValue("@ReleaseDate", movie.releaseDate);
                command.Parameters.AddWithValue("@MovieId", movie.id);
                command.Parameters.AddWithValue("@OriginalTitle", movie.originalTitle);
                command.Parameters.AddWithValue("@Title", movie.title);
                command.Parameters.AddWithValue("@BackdropPath", movie.backdropPath);
                command.Parameters.AddWithValue("@Popularity", movie.popularity);
                command.Parameters.AddWithValue("@VoteCount", movie.voteCount);
                command.Parameters.AddWithValue("@Video", movie.video);
                command.Parameters.AddWithValue("@VoteAverage", movie.voteAverage);
                command.Parameters.AddWithValue("@Runtime", movie.runtime);

                command.ExecuteNonQuery();
            }

            if (movie.genreIds != null)
            {
                string insertMovieGenreQuery = @"
                    INSERT OR REPLACE INTO MovieGenre(
                        MovieId,
                        GenreId
                    ) VALUES (
                        @MovieID, @GenreID
                    )
                ";
                foreach (var genreId in movie.genreIds)
                {
                    if (genreId == 0) continue;
                    using (var command = new SqliteCommand(insertMovieGenreQuery, conn))
                    {
                        command.Parameters.AddWithValue("@MovieId", movie.id);
                        command.Parameters.AddWithValue("@GenreId", genreId);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
    public void InsertGenre(TmdbGenre genre)
    {
        using (var conn = new SqliteConnection(_connectionString))
        {
            conn.Open();

            string insertGenreQuery = @"
                INSERT OR REPLACE INTO Genre(
                    GenreID,
                    Name
                ) VALUES (
                    @GenreID, @Name
                )
            ";

            using (var command = new SqliteCommand(insertGenreQuery, conn))
            {
                command.Parameters.AddWithValue("@GenreID", genre.id);
                command.Parameters.AddWithValue("@Name", genre.name);

                command.ExecuteNonQuery();
            }
        }
    }
    public void InsertUserLogin(Users user)
    {
        using (var conn = new SqliteConnection(_connectionString))
        {
            conn.Open();

            string insertUserQuery = @"
                INSERT OR REPLACE INTO userTable(
                    username,
                    password,
                    first_name
                    last_name,
                    salt,
                ) VALUES (
                     @username, @password, @first_name, @last_name, @salt
                )
            ";

            using (var command = new SqliteCommand(insertUserQuery, conn))
            {
                //command.Parameters.AddWithValue("@UserID", user.UserID);
                command.Parameters.AddWithValue("@username", user.username);
                command.Parameters.AddWithValue("@password", user.passwordHash);
                command.Parameters.AddWithValue("@first_name", user.first_name);
                command.Parameters.AddWithValue("@last_name", user.last_name);
                command.Parameters.AddWithValue("@salt", user.salt);

                command.ExecuteNonQuery();
            }
        }

    }
    public Users FindUserByUsername(string username_login)
    {
        using (var conn = new SqliteConnection(_connectionString))
        {
            conn.Open();

            string findUserName = @"
                SELECT * FROM userTable 
                WHERE username = @username_login
            ";
            using (var cmd = new SqliteCommand(findUserName, conn))
            {
                cmd.Parameters.AddWithValue("@username_login", username_login);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var user = new Users()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            username = reader.GetString(reader.GetOrdinal("username")),
                            passwordHash = reader.GetString(reader.GetOrdinal("password")),
                            first_name = reader.GetString(reader.GetOrdinal("first_name")),
                            last_name = reader.GetString(reader.GetOrdinal("last_name")),
                            salt = reader.GetString(reader.GetOrdinal("salt"))
                        };
                        return user;
                    }

                }

            }

        }
        return null;
    }
    // Search Bar Look up 
    public async Task<List<TmdbMovie>> GetMediaListAsync(string title, List<TmdbMovie> movies)
    {
        
        using (var conn = new SqliteConnection(_connectionString))
        {
            await conn.OpenAsync();
            string query = @" SELECT * FROM Movie
                            WHERE Title LIKE @title";
            using (var cmd = new SqliteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@title", title);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var movie = new TmdbMovie()
                        {
                            id = reader.GetInt32(reader.GetOrdinal("MovieId")),
                            title = reader.GetString(reader.GetOrdinal("Title")),
                            posterPath = reader.GetString(reader.GetOrdinal("PosterPath"))
                        };
                        movies.Add(movie);
                    }
                }


            }

        }
        return movies;

    }
}
