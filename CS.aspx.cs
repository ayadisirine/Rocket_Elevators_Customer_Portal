using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System;
using System.IO;
using System.Net;
using System.Text;


public enum HttpVerb
{
    GET,
    POST,
    PUT,
    DELETE
};
public partial class CS : System.Web.UI.Page
{
    protected void RegisterUser(object sender, EventArgs e)
    {
        int userId = 0;

        //Check user existence in mysql database as customer 
        //??????

        var email = txtEmail.Text.Trim();
        var uri = "https://127.0.0.1:5001/api/Customers/" + email;
        var client = new RestClient(uri, HttpVerb.GET);
        var json = client.MakeRequest();

        if (json.Count() > 0)
        {

            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("Insert_User"))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Connection = con;
                        con.Open();
                        userId = Convert.ToInt32(cmd.ExecuteScalar());
                        con.Close();
                    }
                }
                string message = string.Empty;
                switch (userId)
                {
                    case -1:
                        message = "Username already exists.\\nPlease choose a different username.";
                        break;
                    case -2:
                        message = "Supplied email address has already been used.";
                        break;
                    default:
                        message = "Registration successful.\\nUser Id: " + userId.ToString();
                        break;
                }
                ClientScript.RegisterStartupScript(GetType(), "alert", "alert('" + message + "');", true);
            }
        } else
            ClientScript.RegisterStartupScript(GetType(), "alert", "alert('Customer doesn't exist ');", true);

    }
};





public class RestClient
{
    public string EndPoint { get; set; }
    public HttpVerb Method { get; set; }
    public string ContentType { get; set; }
    public string PostData { get; set; }

    public RestClient()
    {
        EndPoint = "";
        Method = HttpVerb.GET;
        ContentType = "text/xml";
        PostData = "";
    }
    public RestClient(string endpoint)
    {
        EndPoint = endpoint;
        Method = HttpVerb.GET;
        ContentType = "text/xml";
        PostData = "";
    }
    public RestClient(string endpoint, HttpVerb method)
    {
        EndPoint = endpoint;
        Method = method;
        ContentType = "text/xml";
        PostData = "";
    }

    public RestClient(string endpoint, HttpVerb method, string postData)
    {
        EndPoint = endpoint;
        Method = method;
        ContentType = "text/xml";
        PostData = postData;
    }


    public string MakeRequest()
    {
        return MakeRequest("");
    }

    public string MakeRequest(string parameters)
    {
        var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

        request.Method = Method.ToString();
        request.ContentLength = 0;
        request.ContentType = ContentType;

        if (!string.IsNullOrEmpty(PostData) && Method == HttpVerb.POST)
        {
            var encoding = new UTF8Encoding();
            var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
            request.ContentLength = bytes.Length;

            using (var writeStream = request.GetRequestStream())
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            var responseValue = string.Empty;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                throw new ApplicationException(message);
            }

            // grab the response
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
            }

            return responseValue;
        }
    }


}
