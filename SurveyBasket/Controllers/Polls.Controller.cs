

using SurveyBasket.Services.Interfaces;

namespace SurveyBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class Polls(IPollService _pollService) : ControllerBase
    {

        [HttpGet]
        public IActionResult GetAll()
        {
            var polls = _pollService.GetAll();
            return Ok(polls);
        }






        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var Poll = _pollService.GetById(id);
            return Ok(Poll);
        }



        [HttpPost("Add")]
        public IActionResult Add(Poll poll)
        {
            var Poll = _pollService.Add(poll);
            return CreatedAtAction(nameof(Add), new { Poll.Id }, Poll);
        }


        [HttpPut("{id}")]
        public IActionResult Update(int id, Poll poll)
        {
            var isUpdated = _pollService.Update(id, poll);
            if (!isUpdated)
            {

                return BadRequest();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
          var isDeleted = _pollService.Delete(id);  
            if (!isDeleted)
            {

                return BadRequest();
            }
            return NoContent();
        }
    }
}
