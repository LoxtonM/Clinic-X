namespace ClinicX.Meta
{
    public class URLChecker
    {


        public async Task<bool> CheckURL(string url) //checks to see if the URL given resolves
        {
            try
            {
                HttpClient client = new HttpClient();
                var checkingResponse = await client.GetAsync(url);
                if (!checkingResponse.IsSuccessStatusCode)
                {
                    return false;
                }
                return true;
            }
            catch(Exception ex) //bit of an obnoxious way to do it, but the above method won't actually work if the host doesn't exist at all
            {                   //so we have to stop it bombing out by using a try/catch
                return false;
            }
        }

        public string urlLatestVersion(string url) //Routine to check the next ten version numbers to see if it's a simple change
        {
            string urlConfirmed = "";

            string urlSuffix = url.Substring(url.Length - 5);
            string urlStart = url.Substring(0, url.IndexOf("-v"));
            string lastKnownVersionStr = url.Substring(url.IndexOf("-v") + 2, 2); //try double digits first

            int lastKnownVersion = 0;
            if (Int32.TryParse(lastKnownVersionStr, out int j))
            {
                lastKnownVersion = j;
            }
            else if (Int32.TryParse(lastKnownVersionStr.Substring(0, 1), out int k)) //if that fails, try a single digit
            {
                lastKnownVersion = k;
            }

            for (int i = lastKnownVersion; i< lastKnownVersion + 10; i++) //cycle through the next 10 numbers, check both variations for each
            {
                //string urlToTest = url + "-v" + i + ".0" + urlSuffix;
                string urlToTest = urlStart + "-v" + i + urlSuffix;
                string urlToTest2 = urlStart + "-v" + i + ".0" + urlSuffix;
                if (CheckURL(urlToTest).Result)
                {
                    urlConfirmed = urlToTest;
                    break;
                }
                if (CheckURL(urlToTest2).Result)
                {
                    urlConfirmed = urlToTest2;
                    break;
                }

            }
            

            return urlConfirmed;
        }

    }
}
