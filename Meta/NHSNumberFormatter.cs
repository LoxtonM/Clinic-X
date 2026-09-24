namespace ClinicX.Meta
{
    public interface INHSNumberForm
    {
        public string FormatNHSNumber(string rawNHSNumber);
    }

    public class NHSNumberFormatter : INHSNumberForm
    {
        public string FormatNHSNumber(string? rawNHSNumber)
        {
            string formattedNHS = "";

            if (rawNHSNumber != null)
            {
                if (string.IsNullOrWhiteSpace(rawNHSNumber) || rawNHSNumber.Length != 10)
                {
                    return rawNHSNumber; // Return fallback if data is malformed
                }

                formattedNHS = $"{rawNHSNumber.Substring(0, 3)} {rawNHSNumber.Substring(3, 3)} {rawNHSNumber.Substring(6, 4)}";
            }
            
            return formattedNHS;
        }

    }
}
