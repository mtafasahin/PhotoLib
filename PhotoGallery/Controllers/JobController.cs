using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoGallery.Dtos;

namespace PhotoGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public JobController(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpPost("process-images")]
        public IActionResult ProcessImages([FromBody] List<RangeDto> ranges)
        {
            foreach (var range in ranges)
            {
                _backgroundJobClient.Enqueue<ImageProcessingService>(service => service.ProcessImages(range.StartId, range.EndId));
            }
            return Ok("Image processing job has been enqueued.");
        }

        [HttpPost("create-thumbs")]
        public IActionResult CreateThumbs([FromBody] List<RangeDto> ranges)
        {
            foreach (var range in ranges)
            {
                _backgroundJobClient.Enqueue<ImageProcessingService>(service => service.CreateThumbnails(range.StartId, range.EndId));
            }
            return Ok("Image processing job has been enqueued.");
        }


        [HttpPost("calculate-similarities")]
        public IActionResult CalculateSimilarities()
        {
            // Hangfire job'ını tetikle
            BackgroundJob.Enqueue<ImageSimilarityService>(job => job.CalculateSimilarities());

            return Ok("Similarity calculation job has been enqueued.");
        }

        [HttpPost("convert-images")]
        public IActionResult ConvertImages([FromBody] List<RangeDto> ranges)
        {
            // Hangfire job'ını tetikle
            foreach (var range in ranges)
            {
                BackgroundJob.Enqueue<ImageSimilarityService>(job => job.ConvertHeicToJpeg(range.StartId, range.EndId));
            }
            
            return Ok("Similarity calculation job has been enqueued.");
        }
    }
}
