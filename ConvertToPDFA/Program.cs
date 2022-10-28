using PassportPDF.Api;
using PassportPDF.Client;
using PassportPDF.Model;

namespace PDFAConversion
{

    public class PDFAConverter
    {
        static async Task Main(string[] args)
        {
            GlobalConfiguration.ApiKey = "YOUR-PASSPORT-CODE";

            PassportManagerApi apiManager = new();
            PassportPDFPassport passportData = await apiManager.PassportManagerGetPassportInfoAsync(GlobalConfiguration.ApiKey);

            if (passportData == null)
            {
                throw new ApiException("The Passport number given is invalid, please set a valid passport number and try again.");
            }
            else if (passportData.IsActive is false)
            {
                throw new ApiException("The Passport number given not active, please go to your PassportPDF dashboard and active your plan.");
            }

            string uri = "https://passportpdfapi.com/test/invoice_with_barcode.pdf";

            DocumentApi api = new();

            Console.WriteLine("Loading document into PassportPDF...");
            DocumentLoadResponse document = await api.DocumentLoadFromURIAsync(new LoadDocumentFromURIParameters(uri));
            Console.WriteLine("Document loaded.");

            PDFApi pdfApi = new();


            // Convert PDF to PDF/A format with PDF/A-2a conformance level
            Console.WriteLine("Launching PDF/A conversion process...");
            PdfConvertToPDFAResponse pdfConvertResponse = await pdfApi.ConvertToPDFAAsync(new PdfConvertToPDFAParameters(document.FileId)
            {
                Conformance = PdfAConformance.PDFA2a
            });

            if (pdfConvertResponse.Error is not null)
            {
                throw new ApiException(pdfConvertResponse.Error.ExtResultMessage);
            }
            else
            {
                Console.WriteLine("Conversion process ended.");
            }

            // Download file with PDF/A-2a conformance level
            Console.WriteLine("Downloading PDF/A file...");
            try
            {
                PdfSaveDocumentResponse saveDocResponse = await pdfApi.SaveDocumentAsync(new PdfSaveDocumentParameters(document.FileId));

                string savePath = Path.Join(Directory.GetCurrentDirectory(), "pdfa_file.pdf");

                File.WriteAllBytes(savePath, saveDocResponse.Data);

                Console.WriteLine("Done downloading PDF/A file. Document has been saved in : {0}", savePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save PDF/A file! : {0}", ex);
            }

        }
    }
}


