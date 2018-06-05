using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ArffParser
{
    class Program
    {
        private static void TestSql()
        {
            String serverString = "Server = localhost\\SQLEXPRESS01; Database = master; Trusted_Connection = True;";
            SqlConnection sqlConnection = new SqlConnection(serverString);
            SqlCommand sqlCommand = new SqlCommand("CREATE TABLE test_table(name varchar(255))");
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandType = System.Data.CommandType.Text;

            sqlConnection.Open();

            SqlDataReader reader = sqlCommand.ExecuteReader();

            sqlConnection.Close();
        }

        enum MyEnum
        {
            ONE = 1,
            TWO = 2
        }
           
        public static void Main()
        {
            ArffParser arffParser = new ArffParser();
            ArffRecord arffRecord = arffParser.Parse("../../../ArffParser/TestArffFiles/SmallTest.arff");
            Console.WriteLine(arffRecord.ToString());
            Console.ReadLine();
        }
    }
}
