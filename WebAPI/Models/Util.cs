public class Util
{
    public static void WriteErrorToFile(string message, string errorDetail, string filename)
    {
        string path = @".\ErrorLogs\" + filename + ".txt";
        // Delete the file if it exists.
        if(File.Exists(path))
        {
            File.Delete(path);
        }

        try
        {
            // Create a new file and write the error message to it.
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Message: " + message + "\nErrorDetail: " + errorDetail + "\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to write error to file: " + ex.Message);
        }
    }
}