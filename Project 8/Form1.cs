// 
//Netflix Database Application using N-Tier Design
// 
// <<Yushen Li>> 
// U. of Illinois, Chicago 
// CS341, Spring 2018 
// Final Project 
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Project_8
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.clearForm();
        }

        private void clearForm()
        {
            this.listBox1.Items.Clear();
            this.listBox2.Items.Clear();
            this.listBox2.Items.Clear();
            this.listBox3.Items.Clear();
            this.listBox4.Items.Clear();
            this.textBox1.Clear();
            this.textBox2.Clear();
            this.textBox3.Clear();
            this.textBox4.Clear();
            this.textBox5.Clear();
            this.textBox6.Clear();
            this.textBox7.Clear();
            this.textBox8.Clear();
            for (int i = 1; i <= 5; i++)
            {
                this.listBox4.Items.Add(Convert.ToString(i));
            }
        }

        private bool fileExists(string filename)
        {
            if (!System.IO.File.Exists(filename))
            {
                string msg = string.Format("Input file not found: '{0}'",
                  filename);

                MessageBox.Show(msg);
                return false;
            }
            // exists!
            return true;
        }

        private void button1_Click(object sender, EventArgs e) //Movie button
        {
            string filename = this.textBox1.Text;

            if (filename == "")
            {
                MessageBox.Show("Please Input a database connection");
            }
            else
            {
                if (!fileExists(filename))
                    return;

                BusinessTier.Business bizter = new BusinessTier.Business(filename);
                var movielist = bizter.GetAllMovies();
                foreach (var movie in movielist)
                {
                    this.listBox1.Items.Add(Convert.ToString(movie.MovieName));
                }
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //movie List
        {
            string dbfilename = this.textBox1.Text;
            this.textBox2.Clear();
            this.textBox3.Clear();
            this.textBox6.Clear();
            this.textBox6.Text = this.listBox1.Text;

            BusinessTier.Business biztier = new BusinessTier.Business(dbfilename); //open object
            string moviename = this.listBox1.Text;
            BusinessTier.Movie movie = biztier.GetMovie(moviename); //obtain movie object

            if (movie == null)
            {
                MessageBox.Show("Error 404!");
            }

            else
            {
                if (!fileExists(dbfilename))
                    return;
                this.textBox2.Text = movie.MovieID.ToString();
                BusinessTier.MovieDetail details = biztier.GetMovieDetail(Convert.ToInt32(this.textBox2.Text));
                string rating = details.AvgRating.ToString();
                double result = Convert.ToDouble(rating);
                this.textBox3.Text = Convert.ToString(Math.Round(result, 1)); 
            }
        }

        private void button2_Click(object sender, EventArgs e) //movie review
        {
            string dbfilename = this.textBox1.Text;
            this.listBox3.Items.Clear();
            if (dbfilename == "")
            {
                MessageBox.Show("Please Input a database connection");
            }
            else
            {
                if (!fileExists(dbfilename))
                    return;
                BusinessTier.Business biztier = new BusinessTier.Business(dbfilename);
                if (this.listBox1.Text == "")
                {
                    MessageBox.Show("Please select a movie from the list");
                    return;
                }
                string moviename = this.listBox1.Text;
                var movieid = biztier.GetMovie(moviename);
                BusinessTier.MovieDetail detail = biztier.GetMovieDetail(movieid.MovieID);

                if (detail == null)
                {
                    MessageBox.Show("Invalid Input");
                }
                else
                {
                    this.listBox3.Items.Add(moviename);
                    this.listBox3.Items.Add("");
                    foreach (BusinessTier.Review review in detail.Reviews)
                    {
                        string info = String.Format("{0}: {1}", review.UserID, review.Rating);
                        this.listBox3.Items.Add(info);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // Each Rating
        {
            string dbfilename = this.textBox1.Text;
            this.listBox3.Items.Clear();
            if (dbfilename == "")
            {
                MessageBox.Show("Please Input a database connection");
            }
            else
            {
                if (!fileExists(dbfilename))
                    return;
                BusinessTier.Business biztier = new BusinessTier.Business(dbfilename);
                if (this.listBox1.Text == "")
                {
                    MessageBox.Show("Please select a movie from the list");
                    return;
                }
                string moviename = this.listBox1.Text;

                this.listBox3.Items.Add(moviename);
                this.listBox3.Items.Add("");
                var movieid = biztier.GetMovie(moviename);

                var details = biztier.GetMovieDetail(movieid.MovieID);
                var review = details.Reviews;
                int Count = 0;

                var query = from r in details.Reviews
                            group r by r.Rating into grp
                            orderby grp.Key descending
                            select new
                            {
                                Rating = grp.Key,
                                Count = grp.Count()
                            };

                foreach (var tuple in query)
                {
                    string line = String.Format("{0}: {1}", tuple.Rating, tuple.Count);
                    Count += Convert.ToInt32(tuple.Count);
                    this.listBox3.Items.Add(line);
                }
                this.listBox3.Items.Add("");
                string line2 = string.Format("Total: {0}", Convert.ToString(Count));
                this.listBox3.Items.Add(line2);
            }
        }

        private void button4_Click(object sender, EventArgs e) //Username list
        {
            string dbfilename = this.textBox1.Text;
            this.textBox4.Clear();
            this.textBox5.Clear();
            this.textBox7.Clear();

            if (dbfilename == "")
            {
                MessageBox.Show("Please Input a database connection");
            }
            else
            {
                if (!fileExists(dbfilename))
                    return;
                BusinessTier.Business biztier = new BusinessTier.Business(dbfilename);
                var users = biztier.GetAllNamedUsers();

                foreach (var user in users)
                    this.listBox2.Items.Add(user.UserName);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) //Username Listbox
        {
            string dbfilename = this.textBox1.Text;
            this.listBox3.Items.Clear();
            this.textBox4.Clear();
            this.textBox5.Clear();
            this.textBox7.Text = this.listBox2.Text;
            BusinessTier.Business biztier = new BusinessTier.Business(dbfilename); //open object
            string username = this.listBox2.Text;
            var tempname = username;

            BusinessTier.User users = biztier.GetNamedUser(username); //obtain movie object

            var userReviewinfo = biztier.GetUserDetail(users.UserID);
            
            if (users == null)
            {
                MessageBox.Show("Error 404!");
            }
            else
            {
                this.listBox3.Items.Add(username);
                this.listBox3.Items.Add("");
                
                foreach (var review in userReviewinfo.Reviews)
                {
                    if (review == null)
                    {
                        String temp = String.Format("No Review");
                        this.listBox3.Items.Add(temp);
                    }
                    else
                    {
                        var movie = biztier.GetMovie(review.MovieID);
                        string info = String.Format("{0} -> {1}", movie.MovieName, review.Rating);
                        this.listBox3.Items.Add(info);
                    }
                }
                this.textBox4.Text = users.UserID.ToString();
                this.textBox5.Text = users.Occupation.ToString();
            }
        }

        private void button5_Click(object sender, EventArgs e) //clear
        {
            clearForm();
        }

        private void button6_Click(object sender, EventArgs e) //insert review
        {
            string dbfilename = this.textBox1.Text;
            string rating = this.listBox4.Text;
            if (dbfilename == "")
            {
                MessageBox.Show("Please Input a database connection");
            }
            else
            {
                if (!fileExists(dbfilename))
                    return;
                BusinessTier.Business biztier = new BusinessTier.Business(dbfilename);
                var movie = biztier.GetMovie(this.textBox6.Text);
                var user = biztier.GetNamedUser(this.textBox7.Text);
                if (user == null || movie == null || rating == "")
                {
                    MessageBox.Show("Failed Insertion, Possibly Input Has Invalid Movie Name, Username, or Rating score.");
                }
                else
                {
                    var review = biztier.AddReview(movie.MovieID, user.UserID, Convert.ToInt32(rating));
                    if (review == null)
                        MessageBox.Show("Failed Adding");
                    else
                        MessageBox.Show("Successfully Inserted.");
                }
            }
        }

        public bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        private void button7_Click(object sender, EventArgs e) //Top-N averge rating
        {
            string dbfilename = this.textBox1.Text;
            this.listBox3.Items.Clear();
   

            BusinessTier.Business biztier = new BusinessTier.Business(dbfilename);
            string topN = this.textBox8.Text;

            //string infotemp = String.Format("{0}", Convert.ToString(IsNumeric(topN)));
            //MessageBox.Show(infotemp);

            if (!fileExists(dbfilename))
                return;
            if (topN == "" || IsNumeric(topN) == false)
            {
                MessageBox.Show("Input Invalid, Please Input An Integer >= 1");
                return;
            }
            else
            {
                if (Convert.ToInt64(topN) > 9223372036854775807 || Convert.ToInt64(topN) < 1)
                {
                    MessageBox.Show("Input Invalid, Please Input An Integer >= 1");
                    return;
                }
                var topNList = biztier.GetTopMoviesByAvgRating(Convert.ToInt32(topN));
                if (topNList == null)
                {
                    MessageBox.Show("Error 404");
                }
                else
                {
                    foreach (var top in topNList)
                    {
                        string movieid = top.MovieID.ToString();
                        var name = biztier.GetMovie(Convert.ToInt32(movieid));
                        var details = biztier.GetMovieDetail(Convert.ToInt32(movieid));
                        string AvgRating = details.AvgRating.ToString();
                        string info = String.Format("{0}: {1}", Convert.ToString(name.MovieName), Convert.ToString(Math.Round(Convert.ToDouble(AvgRating), 4)));
                        this.listBox3.Items.Add(info);
                    }
                }
            }

        }
    }
}
