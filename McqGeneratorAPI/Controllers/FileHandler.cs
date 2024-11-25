using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser; // iText7 for PDF processing

namespace McqGeneratorAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FileHandler: ControllerBase {
    [HttpGet("getnumber")]
    public async Task<IActionResult> getRandomNumber(){
        return Ok(1);
    }

    [HttpPost("uploadFile/")]
    public async Task<IActionResult> uploadFiles(IFormFile file){
        try{
            var fileName = file.FileName + Guid.NewGuid().ToString();
            Console.WriteLine(fileName);
            using (var client = new AmazonS3Client("", "", RegionEndpoint.USEast1))
            {
                
                using (var newMemoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(newMemoryStream);
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileName,
                        BucketName = "my-first-bucket-s3-akash"
                    };

                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }
            return Ok(fileName);
        }
        catch(Exception e){
            Console.WriteLine(e);
            return BadRequest("No found");
        }
    }

    [HttpPost("deleteFile/{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName){
        try{
            using (var client = new AmazonS3Client("", "", RegionEndpoint.USEast1))
            {
                var deleteObjectRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = "my-first-bucket-s3-akash",
                    Key = fileName
                };

                await client.DeleteObjectAsync(deleteObjectRequest);
            }
            return Ok();
        }catch(Exception e){
            Console.WriteLine("Error Occured");
            Console.WriteLine(e.Message);
            return BadRequest();
        }
    }

    [HttpPost("{fileName}")]
    public async Task<bool> readFileFromS3(string fileName){
          try{
            string responseBody = "";
            using (var client = new AmazonS3Client("", "", RegionEndpoint.USEast1))
            {
               var getObjectRequest = new Amazon.S3.Model.GetObjectRequest
               {
                    BucketName = "my-first-bucket-s3-akash",
                    Key = fileName
                };

                var readObjectResponse = await client.GetObjectAsync(getObjectRequest);
                using (Stream responseStream = readObjectResponse.ResponseStream)
                {
                    using (var pdfReader = new PdfReader(responseStream))
                    using (var pdfDocument = new PdfDocument(pdfReader))
                    {
                        var numberOfPages = pdfDocument.GetNumberOfPages();
                        Console.WriteLine("I ran");
                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            var page = pdfDocument.GetPage(i);
                            var text = PdfTextExtractor.GetTextFromPage(page);
                            System.Console.WriteLine($"Page {i} Text: {text}");
                        }
                    }
                }
            }
            Console.WriteLine(responseBody);
            return true;

        }catch(Exception e){
            Console.WriteLine("Error Occured");
            Console.WriteLine(e.Message);
            return false;
        }
    }
}