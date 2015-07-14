using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechQuickCode.Models.Manager;

namespace TechQuickCode.Controllers
{
    public class DownLoadController : Controller
    {
        public void Attachment(string ID)
        {
            string path = AttachmentManager.Instance.GetAttachmentPath(ID);
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.Stream iStream = null;
                // Buffer to read 10K bytes in chunk:
                byte[] buffer = new Byte[10000];
                // Length of the file:
                int length;
                // Total bytes to read.
                long dataToRead;
                // Identify the file to download including its path.
                string filepath = Server.MapPath("/") + "./" + path;
                // Identify the file name.
                string filename = System.IO.Path.GetFileName(filepath);
                try
                {
                    // Open the file.
                    iStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open,
                                System.IO.FileAccess.Read, System.IO.FileShare.Read);
                    // Total bytes to read.
                    dataToRead = iStream.Length;
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();
                    Response.ContentType = "application/octet-stream"; // Set the file type
                    Response.AddHeader("Content-Length", dataToRead.ToString());
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
                    // Read the bytes.
                    while (dataToRead > 0)
                    {
                        // Verify that the client is connected.
                        if (Response.IsClientConnected)
                        {
                            // Read the data in buffer.
                            length = iStream.Read(buffer, 0, 10000);
                            // Write the data to the current output stream.
                            Response.OutputStream.Write(buffer, 0, length);
                            // Flush the data to the HTML output.
                            Response.Flush();
                            buffer = new Byte[10000];
                            dataToRead = dataToRead - length;
                        }
                        else
                        {
                            // Prevent infinite loop if user disconnects
                            dataToRead = -1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Trap the error, if any.
                    Response.Write("Error : " + ex.Message);
                }
                finally
                {
                    if (iStream != null)
                    {
                        //Close the file.
                        iStream.Close();
                    }
                    Response.End();
                }
            }
            else
            {
                Response.Write("<script>alert('文件不存在！');window.close();</script>");
            }
        }
    }
}
