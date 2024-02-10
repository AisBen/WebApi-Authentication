namespace WebApiAuthentication.DataAccess.Models.DTOs.Responses
{
	public class AisResponseDto
	{
		public string Message { get; set; }
		public bool IsSuccess { get; set; }
		public IEnumerable<string> Errors { get; set; }
	}

	public class AisResponseDto<T> : AisResponseDto
	{
		public T? Data { get; set; }
	}

}
