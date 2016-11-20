using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
namespace WBeParkingPDA.Classes
{
    internal class WebClientHelper
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(WebClientHelper));

        public WebClientHelper()
        {          
        }

        public T GetData<T>(string url) where T : class
        {
            try
            {
                WebRequest request = WebRequest.Create(url);

                request.Method = "GET";
                // Create POST data and convert it to a byte array.
                
                byte[] byteArray = Encoding.UTF8.GetBytes("");
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                //*********************
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                logger.Debug(responseFromServer);

                T Rtn = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseFromServer);
                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                return Rtn; 
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public T PostData<T>(T data, string url) where T : class
        {           
            try
            {
                WebRequest request = WebRequest.Create(url);

                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                string postData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/json";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();

                //*********************
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                logger.Debug(responseFromServer);

                T Rtn = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseFromServer);
                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();

                return Rtn; 
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);

                throw;
            }
        }

        public void uploadfile(string path,string url)
        {          
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "PUT";
            req.AllowWriteStreamBuffering = true;
            
            Stream reqStream = req.GetRequestStream();
            StreamWriter wrtr = new StreamWriter(reqStream);

        }
    }
}
