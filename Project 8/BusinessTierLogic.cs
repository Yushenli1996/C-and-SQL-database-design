//
// BusinessTier:  business logic, acting as interface between UI and data store.
// 
// <<Yushen Li>> 
// U. of Illinois, Chicago 
// CS341, Spring 2018 
// Final Project 
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

    //
    // Business:
    //
    public class Business
    {
        //
        // Fields:
        //
        private string _DBFile;
        private DataAccessTier.Data dataTier;


        //
        // Constructor:
        //
        public Business(string DatabaseFilename)
        {
            _DBFile = DatabaseFilename;

            dataTier = new DataAccessTier.Data(DatabaseFilename);
        }


        //
        // TestConnection:
        //
        // Returns true if we can establish a connection to the database, false if not.
        //
        public bool TestConnection()
        {
            return dataTier.TestConnection();
        }


        //
        // GetNamedUser:
        //
        // Retrieves User object based on USER NAME; returns null if user is not
        // found.
        //
        // NOTE: there are "named" users from the Users table, and anonymous users
        // that only exist in the Reviews table.  This function only looks up "named"
        // users from the Users table.
        //
        public User GetNamedUser(string UserName)
        {
            UserName = UserName.Replace("'", "''");
            string sql = String.Format(@"SELECT UserID, Occupation from Users where UserName = '{0}'", UserName);
            var user = dataTier.ExecuteNonScalarQuery(sql);
            User users = null;
            if (user != null)
            {
                foreach (DataRow row in user.Tables["TABLE"].Rows)
                {
                    users = new User(Convert.ToInt32(row["UserID"]), UserName, Convert.ToString(row["Occupation"]));
                }
                return users;
            }

            else
                return null;

        }


        //
        // GetAllNamedUsers:
        //
        // Returns a list of all the users in the Users table ("named" users), sorted 
        // by user name.
        //
        // NOTE: the database also contains lots of "anonymous" users, which this 
        // function does not return.
        //
        public IReadOnlyList<User> GetAllNamedUsers()
        {
            List<User> users = new List<User>();

            string sql1 = String.Format(@"SELECT * From Users order by UserName");
            if (dataTier.TestConnection() == false)
                return null;
            var username = dataTier.ExecuteNonScalarQuery(sql1);

            foreach (DataRow row in username.Tables["TABLE"].Rows)
            {
                User user = new User(Convert.ToInt32(row["UserID"]), Convert.ToString(row["UserName"]), Convert.ToString(row["Occupation"]));
                users.Add(user);
            }
            return users;
        }


        //
        // GetMovie:
        //
        // Retrieves Movie object based on MOVIE ID; returns null if movie is not
        // found.
        //
        public Movie GetMovie(int MovieID)
        {
            string sql = String.Format(@"SELECT MovieName FROM Movies Where MovieID = '{0}';", MovieID);

            var data = dataTier.ExecuteScalarQuery(sql);
            if (data != null)
                return new Movie(MovieID, Convert.ToString(data));
            else 
                return null;
        }


        //
        // GetMovie:
        //
        // Retrieves Movie object based on MOVIE NAME; returns null if movie is not
        // found.
        //
        public Movie GetMovie(string MovieName)
        {
            MovieName = MovieName.Replace("'", "''");
            string sql = String.Format(@"SELECT MovieID FROM Movies Where MovieName = '{0}';", MovieName);

            var data = dataTier.ExecuteScalarQuery(sql); //(ID, name)
            if (data != null)
                return new Movie(Convert.ToInt32(data), MovieName);
            else
                return null;
        }


        //
        // AddReview:
        //
        // Adds review based on MOVIE ID, returning a Review object containing
        // the review, review's id, etc.  If the add failed, null is returned.
        //
        public Review AddReview(int MovieID, int UserID, int Rating)
        {
            var moviename = GetMovie(MovieID);
            string sql = String.Format(@"INSERT INTO Reviews(MovieID, UserID, Rating) VALUES ({0}, {1}, {2}) 
                                       SELECT ReviewID FROM Reviews WHERE ReviewID = SCOPE_IDENTITY();", MovieID, UserID, Rating);

            var result = dataTier.ExecuteScalarQuery(sql);
            
            if (result != null)
              return new Review(Convert.ToInt32(result), MovieID, UserID, Rating);
            else
                return null;
        }


        //
        // GetMovieDetail:
        //
        // Given a MOVIE ID, returns detailed information about this movie --- all
        // the reviews, the total number of reviews, average rating, etc.  If the 
        // movie cannot be found, null is returned.
        //
        public MovieDetail GetMovieDetail(int MovieID)
        {
            string sql1 = String.Format(@"SELECT ReviewID, MovieID, UserID, Rating From Reviews Where MovieID = {0} order by rating DESC, UserId ASC;", MovieID);
            string sql2 = String.Format(@"SELECT SUM(Rating) as totalRating From Reviews Where movieID = {0}", MovieID);
            string sql3 = String.Format(@"SELECT Count(*) AS Count From Reviews Where movieID = {0}", MovieID);
            double avgRating = 0;
            var data = dataTier.ExecuteNonScalarQuery(sql1);
            var sum = dataTier.ExecuteScalarQuery(sql2);
            var Count = dataTier.ExecuteScalarQuery(sql3);
            if (Convert.ToInt32(Count) == 0)
                avgRating = 0;
            else
                avgRating = Convert.ToDouble(sum) / Convert.ToDouble(Count);

            Movie m = GetMovie(MovieID);

            var L = new List<Review>();

            if (m != null)
            {
                foreach (DataRow row in data.Tables["TABLE"].Rows)
                {
                    Review reviewObj = new Review(Convert.ToInt32(row["ReviewID"]), Convert.ToInt32(row["MovieID"]), Convert.ToInt32(row["UserID"]), Convert.ToInt32(row["Rating"]));
                    L.Add(reviewObj);
                }

                return new MovieDetail(m, avgRating, Convert.ToInt32(Count), L);
            }
            else
                return null;
        }


        //
        // GetUserDetail:
        //
        // Given a USER ID, returns detailed information about this user --- all
        // the reviews submitted by this user, the total number of reviews, average 
        // rating given, etc.  If the user cannot be found, null is returned.
        //
        public UserDetail GetUserDetail(int UserID)
        {
            string sql  = String.Format(@"SELECT Movies.MovieName, Reviews.ReviewID, Reviews.MovieID, Reviews.UserID, Reviews.Rating
                                        From Reviews INNER JOIN Movies On Reviews.MovieID = Movies.MovieID where UserID = {0} 
                                        order by MovieName ASC", UserID);

            string sql1 = String.Format(@"SELECT UserName FROM Users WHERE UserID = {0}", UserID);
            string sql2 = String.Format(@"SELECT SUM(Rating) as totalRating From Reviews Where UserID = {0}", UserID);
            string sql3 = String.Format(@"SELECT Count(*) AS Count From Reviews Where UserID = {0}", UserID);

            var review = dataTier.ExecuteNonScalarQuery(sql); //select all reviews from a particular user

            var user = dataTier.ExecuteScalarQuery(sql1); //get user information

            User u = GetNamedUser(Convert.ToString(user)); //setting  user object

            var sum = dataTier.ExecuteScalarQuery(sql2);
            var Count = dataTier.ExecuteScalarQuery(sql3);

            double avgRating = 0.0;

            if (Convert.ToInt32(Count) == 0)
                avgRating = 0;
            else
                avgRating = Convert.ToInt32(sum) / Convert.ToInt32(Count);

            var L = new List<Review>();

            if (u != null)
            {
                foreach (DataRow row in review.Tables["TABLE"].Rows)
                {
                    Review reviewObj = new Review(Convert.ToInt32(row["ReviewID"]), Convert.ToInt32(row["MovieID"]), Convert.ToInt32(row["UserID"]), Convert.ToInt32(row["Rating"]));
                    L.Add(reviewObj);
                }
                return new UserDetail(u , avgRating, Convert.ToInt32(Count), L);
            }
            else
                return null;
        }


        //
        // GetTopMoviesByAvgRating:
        //
        // Returns the top N movies in descending order by average rating.  If two
        // movies have the same rating, the movies are presented in ascending order
        // by name.  If N < 1, an EMPTY LIST is returned.
        //
        public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
        {
            List<Movie> movies = new List<Movie>();

            if (N < 1)
                return movies;
            else
            {
                string sql = String.Format(@"SELECT Top {0} Reviews.MovieID, AVG(CONVERT(float, Reviews.Rating)) as RatingAvg From Reviews Group by Reviews.MovieID order by RatingAvg DESC, Reviews.MovieID ASC;", N);
                var TopN = dataTier.ExecuteNonScalarQuery(sql);

                if (TopN == null)
                    return movies;
                else
                {
                    foreach (DataRow row in TopN.Tables["TABLE"].Rows)
                    {
                        string sql2 = String.Format(@"SELECT MovieName from Movies Where MovieID = {0}", Convert.ToInt32(row["MovieID"]));
                        var name = dataTier.ExecuteScalarQuery(sql2);
                       // Console.Write(Convert.ToString(name));
                        Movie m = new Movie(Convert.ToInt32(row["MovieID"]), Convert.ToString(name));
                        movies.Add(m);
                    }
                    return movies;
                }
            }
        }

        public IReadOnlyList<Movie> GetAllMovies()
        {
            List<Movie> movies = new List<Movie>();

            string sql = string.Format(@"SELECT MovieID, MovieName FROM Movies Order by MovieName ASC;");
            var movie = dataTier.ExecuteNonScalarQuery(sql);

            if (movie == null)
                return movies;
            else
            {
                foreach (DataRow row in movie.Tables["TABLE"].Rows)
                {
                    Movie m = new Movie(Convert.ToInt32(row["MovieID"]), Convert.ToString(row["MovieName"]));
                    movies.Add(m);
                }
                return movies;
            }
        }


    }//class
}//namespace
