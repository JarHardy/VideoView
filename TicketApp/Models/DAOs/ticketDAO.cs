using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace TicketApp.Models
{
    public class ticketDAO : Controller
    {
       // Ticket ticket = new Ticket();

        SqlCommand command = new SqlCommand();
        public static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        SqlConnection SQL = new SqlConnection(connectionString);

        private bool OpenConnection()
        {


            try
            {
                SQL.Open();
                Console.WriteLine("Connect to server.");
                return true;
            }
            catch
            {
                Console.WriteLine("Cannot connect to server.");
                return false;
            }

        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                SQL.Close();
                return true;
            }
            catch
            {
                Console.WriteLine("Cannot close connect to server.");
                return false;
            }
        }



        public void test(Ticket ticket)
        {
            string addticket = "INSERT INTO JOB_TICKET (ASSIGNED_TECH_ID) VALUES (@ASSIGNED_TECH_ID)";

            if(ticket != null)
            {
                if(this.OpenConnection() == true)
                {
                    try
                    {
                        using (SqlCommand query = new SqlCommand(addticket, SQL))
                        {
                            query.Parameters.AddWithValue("@ASSIGNED_TECH_ID", SqlDbType.Int).Value = ticket.ASSIGNED_TECH_ID;


                            query.ExecuteNonQuery();

                        }
                    }
                    catch
                    {
                        Console.WriteLine("Could not enter information into the DB");
                    }
                }
            }


            
        }

       
    }
}
